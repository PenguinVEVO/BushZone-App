using FrameLabs.Utilities.NodeEditor.Layer;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;
using System;
using System.Linq;
using FrameLabs.Utilities.NodeEditor.Generation;
using FrameLabs.NodeEditor.Utilities;

namespace FrameLabs.Utilities.NodeEditor
{
    public class NodeGraphEditor : EditorWindow
    {
        public static string EDITOR_VERSION = "2.0.0a2";
        private EditorMessageBox messageBox;
        private static bool autoSave = false;

        private bool isCacheLoaded = false;
        private string styleSheetPath;
        private StyleSheet editorStyleSheet;

        [MenuItem( "Window/FrameLabs/FlowState Graph Editor" )]
        public static void Open()
        {
            NodeGraphEditor window = GetWindow<NodeGraphEditor>();
            window.titleContent = new GUIContent( "FlowState Editor" );
            window.minSize = new Vector2( 960, 540 );           
        }

        public static bool IsAutoSave => autoSave;

        internal void TryLoadCache()
        {
            if( isCacheLoaded ) return;

            Debug.Log( "[NodeGraph] Attempting to load cache..." );

            GraphSessionManager.Instance.RestoreFromCache(
                SwitchToTab,
                name =>
                {
                    var tab = TabManager.Instance.GetTabByName( name );
                    WorkspaceManager.Instance.SetActiveTab( tab );
                    return tab?.GraphView;
                }
            );

            isCacheLoaded = true;
        }

        private void OnEnable()
        {
            isCacheLoaded = false;
            autoSave = EditorPrefs.GetBool( "autoSave" );

            InitializeEditorLayers();
            InitializeToolbar();
            InitializeWorkspace();
            LoadEditorStyles();

            messageBox = new EditorMessageBox();
            rootVisualElement.Add( messageBox );
        }

        private void ShowEditorMessage(string msg, MessageCategory category = MessageCategory.Info, float duration = 3f)
        {
            messageBox?.ShowMessage(msg, duration, category);
        }


        private void OnDisable()
        {
            GraphSessionManager.Instance.SaveToCache();

            TabManager.Instance.ClearAllTabs();
            WorkspaceManager.Instance.ClearWorkspace();
            ToolbarManager.Instance.Clear();
        }

        private void OnDestroy()
        {
            LayerManager.Instance.ClearLayers();
            GraphSessionManager.Instance.CleanUp();
        }
     
        private void LoadEditorStyles()
        {
            styleSheetPath = $"{Application.dataPath}";
            string file = Utility.Instance.FindFileInPath( styleSheetPath, "GraphEditor.uss" );

            editorStyleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>( file );

            if( editorStyleSheet != null )
            {
                rootVisualElement.styleSheets.Add( editorStyleSheet );
            }
            else
            {
                Debug.LogError( "Could not find GraphEditor.uss stylesheet. Please check the path." );
            }
        }

        private void InitializeEditorLayers()
        {
            LayerManager.Instance.Initialize(rootVisualElement);

            LayerManager.Instance.AddLayer("Toolbar", new LayerConfig
            {
                Order = 0,
                Position = Position.Relative,
                Overflow = Overflow.Visible,
                FlexGrow = false,
                StyleClasses = new[] { "toolbar-layer" }
            });

            LayerManager.Instance.AddLayer("Tabs", new LayerConfig
            {
                Order = 1,
                Position = Position.Relative,
                Overflow = Overflow.Visible,
                FlexGrow = false,
                StyleClasses = new[] { "tabs-layer" }
            });

            LayerManager.Instance.AddLayer("Workspace", new LayerConfig
            {
                Order = 2,
                Position = Position.Relative,
                Overflow = Overflow.Hidden,
                FlexGrow = true 
            });
        }

        private void InitializeToolbar()
        {
            Toolbar toolStrip = ToolbarManager.Instance.CreateToolBar("MainMenu");

            var exportIcon = EditorGUIUtility.IconContent("d_SaveAs").image;
            var jsonIcon = EditorGUIUtility.IconContent("TextAsset Icon").image;

            var actions = new List<ContextMenuAction>
        {
            new("New/Graph", CreateNewGraph),
            new("Load Graph", LoadGraph),
            new("Save Graph", SaveGraph),
            new("Import/Custom Node Processor", RegisterCustomCompiler, true),
            new("Build/Generate Tree", GenerateTree, true),
        };

            // Create context menu button first
            Button contextMenuBtn = ToolbarManager.Instance.CreateContextMenuButton("File", actions);
            toolStrip.Add(contextMenuBtn);

            var spacer = new VisualElement();
            spacer.AddToClassList("toolbar-spacer");
            toolStrip.Add(spacer);

            Toggle autoSaveToggle = new Toggle("Auto Save");
            autoSaveToggle.RegisterValueChangedCallback(evt =>
            {
                EditorPrefs.SetBool( "autoSave", evt.newValue );
            });
            autoSaveToggle.SetValueWithoutNotify( autoSave );

            autoSaveToggle.style.flexGrow = 0;
            autoSaveToggle.labelElement.style.minWidth = 0;
            toolStrip.Add(autoSaveToggle);

            var dropdown = new DropdownField("Editor Layers:", Enum.GetNames(typeof(GraphViewRenderMode)).ToList(), 0);
            dropdown.RegisterValueChangedCallback(evt =>
            {
                var mode = (GraphViewRenderMode)Enum.Parse(typeof(GraphViewRenderMode), evt.newValue);
                var graphView = WorkspaceManager.Instance.GetActiveTab()?.GraphView;
                graphView?.ApplyRenderMode(mode);
            });

            dropdown.style.flexGrow = 0;
            dropdown.style.width = StyleKeyword.Auto;
            dropdown.labelElement.style.minWidth = 0;

            toolStrip.Add(dropdown);
        }

        private void RegisterCustomCompiler()
        {
            string scriptPath = EditorUtility.OpenFilePanel("Select Custom Node Processor Script", "Assets", "cs");
            if (string.IsNullOrEmpty(scriptPath)) return;

            string assetPath = "Assets" + scriptPath.Substring(Application.dataPath.Length);

            Type compilerType = GraphEditorUtility.Instance.GetTypeFromScriptPath(assetPath);

            if (compilerType == null)
            {
                ShowEditorMessage("Could not resolve a class from the selected script file.", MessageCategory.Error);
                return;
            }

            var baseCompilerInterface = compilerType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INodeProcessor<>));

            if (baseCompilerInterface == null)
            {
                ShowEditorMessage("Selected class does not implement INodeProcessor<>.", MessageCategory.Warning);
                return;
            }

            Type nodeType = baseCompilerInterface.GetGenericArguments().FirstOrDefault();
            if (nodeType == null) return;

            var instance = Activator.CreateInstance(compilerType);
            var registerMethod = typeof(NodeCompilerRegistry).GetMethod("RegisterCustomCompiler")?.MakeGenericMethod(nodeType);
            registerMethod?.Invoke(null, new object[] { instance, true });

            // Save registration
            string registryPath = "Assets/Cache/CustomCompilerRegistry.asset";
            var registry = AssetDatabase.LoadAssetAtPath<CompilerRegistryAsset>(registryPath);
            if (registry == null)
            {
                registry = ScriptableObject.CreateInstance<CompilerRegistryAsset>();
                AssetDatabase.CreateAsset(registry, registryPath);
            }

            if (!registry.compilerTypeNames.Contains(compilerType.AssemblyQualifiedName))
            {
                registry.compilerTypeNames.Add(compilerType.AssemblyQualifiedName);
                EditorUtility.SetDirty(registry);
                AssetDatabase.SaveAssets();
            }

            ShowEditorMessage($"Registered compiler: {compilerType.Name} for {nodeType.Name}", MessageCategory.Info);
        }


        private void GenerateTree()
        {
            var activeTab = WorkspaceManager.Instance.GetActiveTab();
            if (activeTab == null || activeTab.GraphView == null)
            {
                ShowEditorMessage("[NodeGraphEditor] No active graph to compile.", MessageCategory.Error,4);

                return;
            }

            string fullPath = EditorUtility.SaveFilePanel(
                "Save Behavior Tree",
                "Assets",
                activeTab.Name,
                "asset"
            );

            if (string.IsNullOrEmpty(fullPath) || !fullPath.StartsWith(Application.dataPath))
            {
                ShowEditorMessage("[NodeGraphEditor] Save path was invalid or cancelled.", MessageCategory.Warning);
                return;
            }

            string treeName = Path.GetFileNameWithoutExtension(fullPath);
            string assetDirectory = "Assets" + Path.GetDirectoryName(fullPath).Substring(Application.dataPath.Length);

            var result = TreeGeneration.Generate(treeName, assetDirectory, activeTab.GraphView);
            if (!result.Success)
            {
                EditorUtility.DisplayDialog(
                    "Tree Build Failed",
                    "Errors occurred during tree generation. Check the console for details.",
                    "OK"
                );
                return;
            }

            ShowEditorMessage($"[NodeGraphEditor] Tree compiled successfully. Root node: {result.CompiledAsset.RootNode.name}");
            EditorUtility.DisplayDialog("Tree Build Complete", $"Tree '{result.CompiledAsset.RootNode.name}' compiled successfully!", "OK");
        }

        private void InitializeWorkspace()
        {
            TabManager.Instance.Initialize();
            WorkspaceManager.Instance.Initialize();
        }

        private void CreateNewGraph()
        {
            string tabName = GenerateUniqueGraphName( "New Graph" );

            GraphTab newTab = TabManager.Instance.CreateTab( tabName, SwitchToTab, CloseTab );
            StateGraphView newGraph = newTab.GraphView;

            WorkspaceManager.Instance.SetActiveTab(newTab);

            NodeGraphData graphData = GraphSerialization.Instance.SerializeGraph( newGraph );
            GraphSessionManager.Instance.RegisterTab( tabName, graphData );

            Debug.Log( $"Created new graph with tab name: {tabName}" );
        }

        private void SaveGraph()
        {
            var activeTab = WorkspaceManager.Instance.GetActiveTab();
            if( activeTab == null || activeTab.GraphView == null )
            {
                ShowEditorMessage("[NodeGraphEditor] Cannot save graph. No active tab or graph view.", MessageCategory.Error);
                return;
            }

            // Prompt user for save location
            string savePath = EditorUtility.SaveFilePanel(
                "Save Graph As...",
                "Assets/GraphCache",
                $"{activeTab.Name}",
                "asset"
            );

            // User cancelled the dialog
            if( string.IsNullOrEmpty( savePath ) )
            {
                Debug.Log( "[NodeGraphEditor] Save cancelled by user." );
                return;
            }

            // Ensure the selected path is inside the project's Assets folder
            if( !savePath.StartsWith( Application.dataPath ) )
            {
                ShowEditorMessage("[NodeGraphEditor] Save path must be within the project's Assets folder.", MessageCategory.Error);
                return;
            }

            // Convert absolute path to relative path
            string assetPath = "Assets" + savePath.Substring( Application.dataPath.Length );

            // Serialize graph
            NodeGraphData graphData = GraphSerialization.Instance.SerializeGraph( activeTab.GraphView );
            
            // assign the current editor version
            graphData.EditorVersion = EDITOR_VERSION;

            GraphSessionManager.Instance.RegisterTab( activeTab.Name, graphData );

            // Write to disk
            bool success = GraphWriter.Instance.SaveGraphForTab( activeTab.Name, graphData, assetPath );

            if( success )
            {
                ShowEditorMessage($"[NodeGraphEditor] Graph successfully saved to: {assetPath}");
                GraphSessionManager.Instance.SaveToCache();
            }
            else
            {
                ShowEditorMessage("[NodeGraphEditor] Failed to save graph to disk.");
            }
        }

        private void LoadGraph()
        {
            GraphManager.Instance.LoadGraph(
                newTab =>
                {
                    TabManager.Instance.CreateTab( newTab.Name, SwitchToTab, CloseTab );
                    WorkspaceManager.Instance.SetActiveTab( newTab );

                    SwitchToTab(newTab.Name);
                },
                loadedGraphView =>
                {
                    var activeTab = WorkspaceManager.Instance.GetActiveTab();
                    if( activeTab != null )
                    {
                        activeTab.GraphView = loadedGraphView.GraphView;
                        WorkspaceManager.Instance.SetActiveTab( activeTab );

                        GraphSessionManager.Instance.RegisterTab( activeTab.Name, activeTab.GraphData );

                        activeTab.GraphView.UpdateAllEdges();
                        activeTab.GraphView.MarkDirtyRepaint();
                    }
                    else
                    {
                        ShowEditorMessage("Failed to load graph: No active tab found to assign the graph view.");
                    }
                }
            );
        }

        private void SwitchToTab( string tabName )
        {
            TabManager.Instance.SwitchToTab(
                tabName,
                () => WorkspaceManager.Instance.ClearWorkspace(),
                name =>
                {
                    var tab = TabManager.Instance.GetTabByName( name );
                    WorkspaceManager.Instance.SetActiveTab( tab );
                    return tab?.GraphView;
                }
            );
        }

        private string GenerateUniqueGraphName( string baseName )
        {
            int suffix = 1;
            string uniqueName = baseName;

            while( TabManager.Instance.HasTab( uniqueName ) )
            {
                uniqueName = $"{baseName} ({suffix++})";
            }

            return uniqueName;
        }

        private void CloseTab( GraphTab tab )
        {
            TabManager.Instance.CloseTab( tab, SwitchToTab, WorkspaceManager.Instance.ClearWorkspace );
            GraphSessionManager.Instance.RemoveTab( tab.Name );
        }
    }

}