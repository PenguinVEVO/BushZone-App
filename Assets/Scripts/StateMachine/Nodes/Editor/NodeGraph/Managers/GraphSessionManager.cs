using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphSessionManager
    {
        private static readonly GraphSessionManager instance = new();
        public static GraphSessionManager Instance => instance;

        private const string CacheAssetPath = "Assets/Cache/GraphEditorCache.asset";

        private readonly Dictionary<string, NodeGraphData> openGraphs = new();
        private EditorCache editorCache;

        private GraphSessionManager()
        {
            LoadOrCreateEditorCache();          
        }

        private void CloseTab( GraphTab tab )
        {
            TabManager.Instance.CloseTab(
                tab,
                name => TabManager.Instance.SwitchToTab(
                    name,
                    WorkspaceManager.Instance.ClearWorkspace,
                    _ => WorkspaceManager.Instance.GetActiveTab()?.GraphView ),
                WorkspaceManager.Instance.ClearWorkspace
            );

            RemoveTab( tab.Name );
        }

        private void LoadOrCreateEditorCache()
        {
            if( !AssetDatabase.IsValidFolder( "Assets/Cache" ) )
                AssetDatabase.CreateFolder( "Assets", "Cache" );

            editorCache = AssetDatabase.LoadAssetAtPath<EditorCache>( CacheAssetPath );
            if( editorCache == null )
            {
                editorCache = ScriptableObject.CreateInstance<EditorCache>();
                AssetDatabase.CreateAsset( editorCache, CacheAssetPath );
                AssetDatabase.SaveAssets();
            }
        }

        public void RecordSnapShot()
        {
            foreach( string tabName in GraphChangeNotifier.Instance.GetDirtyTabs() )
            {
                if( !openGraphs.TryGetValue( tabName, out var graphData ) || graphData == null )
                    continue;

                Debug.Log( $"[GraphSessionManager] Saving recovery snapshot for tab '{tabName}'..." );

                GraphRecoveryManager.SaveRecoverySnapshot( tabName, graphData );
            }
        }

        public void RenameTabAsset(string oldName, string newName, NodeGraphData data)
        {
            // Get current asset path
            string oldPath = AssetDatabase.GetAssetPath(data);
            if (!string.IsNullOrEmpty(oldPath))
            {
                string newFileName = newName + Path.GetExtension(oldPath);
                string newPath = Path.Combine(Path.GetDirectoryName(oldPath), newFileName).Replace("\\", "/");
                
                AssetDatabase.RenameAsset(oldPath, newName);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            // Update the openGraphs dictionary
            if (openGraphs.TryGetValue(oldName, out var graphData))
            {
                openGraphs.Remove(oldName);
                openGraphs[newName] = graphData;
            }

            // Update EditorCache tab name references
            for (int i = 0; i < editorCache.TabData.Count; i++)
            {
                if (editorCache.TabData[i].Name == oldName)
                {
                    editorCache.TabData[i].Name = newName;
                    break;
                }
            }

            for (int i = 0; i < editorCache.TabNames.Count; i++)
            {
                if (editorCache.TabNames[i] == oldName)
                {
                    editorCache.TabNames[i] = newName;
                    break;
                }
            }

            EditorUtility.SetDirty(editorCache);
            AssetDatabase.SaveAssets();

            GraphChangeNotifier.Instance.MarkDirty(newName);
            Debug.Log($"[GraphSessionManager] Renamed graph '{oldName}' to '{newName}' (file and cache updated).");
        }


        public void DeleteTabAndAsset(string tabName)
        {
            if (openGraphs.TryGetValue(tabName, out var data))
            {
                GraphWriter.Instance.DeleteGraph(data);
                openGraphs.Remove(tabName);
            }

            int index = editorCache.TabNames.IndexOf(tabName);
            if (index != -1)
            {
                editorCache.TabNames.RemoveAt(index);
                editorCache.TabData.RemoveAt(index);
                EditorUtility.SetDirty(editorCache);
                AssetDatabase.SaveAssets();
            }

            GraphChangeNotifier.Instance.MarkDirty(tabName);
            Debug.Log($"[GraphSessionManager] Deleted graph and removed tab '{tabName}'.");
        }

        // Called during editor shutdown
        public void SaveToCache()
        {
            if (editorCache == null) return;

            editorCache.TabData.Clear();
            editorCache.TabNames.Clear();

            foreach (var pair in openGraphs)
            {
                if (pair.Value == null)
                {
                    Debug.LogWarning($"[GraphSessionManager] Skipping null graph data for tab '{pair.Key}'");
                    continue;
                }

                string tabName = pair.Key;
                NodeGraphData graphData = pair.Value;
                graphData.EditorVersion = NodeGraphEditor.EDITOR_VERSION;

                string assetPath = AssetDatabase.GetAssetPath(graphData);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    editorCache.TabData.Add(new TabEntry { Name = tabName, GraphData = graphData });
                    editorCache.TabNames.Add(tabName);
                    GraphChangeNotifier.Instance.MarkDirty(tabName);
                    continue;
                }

                if (GraphWriter.Instance.SaveGraphForTab(tabName, graphData))
                {
                    editorCache.TabData.Add(new TabEntry { Name = tabName, GraphData = graphData });
                    editorCache.TabNames.Add(tabName);
                    GraphChangeNotifier.Instance.MarkDirty(tabName);
                }
            }

            editorCache.ActiveTabName = TabManager.Instance.GetActiveTabName();
            editorCache.TabHistory.Clear();
            editorCache.TabHistory.AddRange(TabManager.Instance.GetTabHistory());

            EditorUtility.SetDirty(editorCache);
            AssetDatabase.SaveAssets();

            Debug.Log($"[GraphSessionManager] Saved EditorCache {editorCache.name}");
        }



        // Called on editor startup
        public void RestoreFromCache(Action<string> onSwitchToTab, Func<string, StateGraphView> getGraphView)
        {
            if (editorCache == null) return;

            // Restore cached tabs with fallback to recovery
            foreach (var entry in editorCache.TabData)
            {
                var tab = TabManager.Instance.CreateTab(entry.Name, onSwitchToTab, CloseTab);
                NodeGraphData data = entry.GraphData;

                if (data == null)
                {
                    Debug.LogWarning($"[GraphSessionManager] No cached data for '{entry.Name}', trying recovery...");
                    data = GraphRecoveryManager.LoadRecovery().GetValueOrDefault(entry.Name);
                    if (data != null)
                    {
                        GraphChangeNotifier.Instance.MarkDirty(entry.Name);
                    }
                }
                else
                {
                    GraphChangeNotifier.Instance.ClearDirty(entry.Name);
                }

                openGraphs[entry.Name] = data;

                if (data != null)
                {
                    GraphSerialization.Instance.DeserializeGraph(data, tab.GraphView);
                }
            }

            // Restore recovery-only graphs (never saved)
            var recoveryGraphs = GraphRecoveryManager.LoadRecovery();
            foreach (var kvp in recoveryGraphs)
            {
                if (!openGraphs.ContainsKey(kvp.Key))
                {
                    var tab = TabManager.Instance.CreateTab(kvp.Key, onSwitchToTab, CloseTab);
                    openGraphs[kvp.Key] = kvp.Value;

                    if (kvp.Value != null)
                    {
                        GraphSerialization.Instance.DeserializeGraph(kvp.Value, tab.GraphView);
                        Debug.Log($"[GraphSessionManager] Recovered graph tab '{kvp.Key}' from snapshot.");
                        GraphChangeNotifier.Instance.MarkDirty(kvp.Key);
                    }
                }
            }

            // Restore tab history
            if (editorCache.TabHistory != null)
            {
                TabManager.Instance.SetTabHistory(editorCache.TabHistory);
            }

            // Restore active tab
            string activeTab = editorCache.ActiveTabName;
            if (!string.IsNullOrEmpty(activeTab) && TabManager.Instance.HasTab(activeTab))
            {
                TabManager.Instance.SwitchToTab(activeTab, null, getGraphView);
            }
        }


        public void RegisterTab(string tabName, NodeGraphData data)
        {
            openGraphs[tabName] = data;

            string path = AssetDatabase.GetAssetPath(data);
            if (string.IsNullOrEmpty(path))
            {
                GraphChangeNotifier.Instance.MarkDirty(tabName);
            }
            else
            {
                GraphChangeNotifier.Instance.ClearDirty(tabName);
            }
        }

        public void CleanUp()
        {
            GraphRecoveryManager.ClearRecovery();
        }

        public void UnregisterTab( string tabName )
        {
            openGraphs.Remove( tabName );
            GraphChangeNotifier.Instance.MarkDirty(tabName );
        }

        public void RemoveTab( string tabName )
        {
            openGraphs.Remove(tabName);

            int index = editorCache.TabNames.IndexOf( tabName );
            if( index != -1 )
            {
                editorCache.TabNames.RemoveAt( index );
                editorCache.TabData.RemoveAt( index );
                EditorUtility.SetDirty( editorCache );
                AssetDatabase.SaveAssets();
            }

            GraphChangeNotifier.Instance.ClearDirty(tabName);
        }

        public NodeGraphData GetGraph( string tabName )
        {
            openGraphs.TryGetValue( tabName, out var data );
            return data;
        }

        public IEnumerable<KeyValuePair<string, NodeGraphData>> GetAllRegisteredTabs() => openGraphs;
    }
}
