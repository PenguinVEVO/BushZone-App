using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphManager
    {
        private static readonly GraphManager instance = new();

        private GraphManager() { }

        public static GraphManager Instance => instance;

        public void SaveGraph( GraphTab tab, Action<string> onGraphSaved )
        {
            if( tab == null || tab.GraphView == null )
            {
                Debug.LogError( "Cannot save graph. Tab or GraphView is null." );
                return;
            }

            string path = EditorUtility.SaveFilePanelInProject(
                "Save Node Graph",
                tab.Name,
                "asset",
                "Enter a file name to save the graph"
            );

            if( !string.IsNullOrEmpty( path ) )
            {
                var data = GraphSerialization.Instance.SerializeGraph(tab.GraphView);
                GraphWriter.Instance.WriteGraph( data, path );

                // Cache the saved data back into the tab
                tab.GraphData = data;

                onGraphSaved?.Invoke( path );
            }
        }

        public void LoadGraph( Action<GraphTab> onCreateNewTab, Action<GraphTab> onGraphLoaded )
        {
            // Open file panel for graph selection
            string path = EditorUtility.OpenFilePanel( "Load Node Graph", "Assets", "asset" );
            if( string.IsNullOrEmpty( path ) ) return;

            // Convert to project-relative path
            path = FileUtil.GetProjectRelativePath( path );

            // Generate a unique tab name based on the file name
            string tabName = GenerateUniqueGraphName( Path.GetFileNameWithoutExtension( path ) );

            GraphTab activeTab = TabManager.Instance.GetActiveTab();

            if (activeTab != null && activeTab.Name == tabName ) 
            {
                WorkspaceManager.Instance.ClearWorkspace();
                activeTab.GraphView.ClearGraph();
            }

            // Create a new tab and configure its callbacks
            var newTab = TabManager.Instance.CreateTab(
                tabName,
                tabName => WorkspaceManager.Instance.SetActiveTab( TabManager.Instance.GetTabByName( tabName ) ),
                tab => WorkspaceManager.Instance.ClearWorkspace()
            );

            // Ensure the tab was created successfully
            if( newTab == null )
            {
                Debug.LogError( $"Failed to create tab for '{tabName}'." );
                return;
            }

            // Initialize the graph view for the tab
            StateGraphView graphView = new StateGraphView(newTab)
            {
                name = tabName
            };

            NodeGraphData graphData;
            // Deserialize the graph data into the graph view
            GraphSerialization.Instance.DeserializeGraph( path, graphView, out graphData  );
            
            newTab.GraphView = graphView;
            newTab.GraphData = graphData;
            
            newTab.ContentContainer.Clear();
            newTab.ContentContainer.Add( graphView );

            // Set the new tab as active in the workspace
            WorkspaceManager.Instance.SetActiveTab( newTab );

            // Notify the provided callbacks
            onCreateNewTab?.Invoke( newTab );
            onGraphLoaded?.Invoke( newTab );
        }


        public void NewGraph( string baseName, Action<GraphTab> onGraphCreated )
        {
            // Generate a unique graph name
            string graphName = GenerateUniqueGraphName( baseName );

            // Create a new tab with callbacks for switching and closing
            var newTab = TabManager.Instance.CreateTab(
                graphName,
                tabName => WorkspaceManager.Instance.SetActiveTab( TabManager.Instance.GetTabByName( tabName ) ),
                tab => WorkspaceManager.Instance.ClearWorkspace()
            );

            // Ensure the tab was created successfully
            if( newTab == null )
            {
                Debug.LogError( $"Failed to create new tab for '{graphName}'." );
                return;
            }

            // Create and assign a new graph view to the tab
            StateGraphView graphView = new StateGraphView(newTab)
            {
                name = graphName
            };

            newTab.GraphView = graphView;

            // Set the new tab as active in the workspace
            WorkspaceManager.Instance.SetActiveTab( newTab );

            // Notify the caller that the graph has been created
            onGraphCreated?.Invoke( newTab );
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
    }



}


