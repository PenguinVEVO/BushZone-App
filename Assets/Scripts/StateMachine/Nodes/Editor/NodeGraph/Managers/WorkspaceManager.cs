using FrameLabs.Utilities.NodeEditor.Layer;
using System;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class WorkspaceManager
    {
        private static WorkspaceManager instance = new();
        private VisualElement workspaceElement;
        private GraphTab activeTab;

        public static WorkspaceManager Instance => instance;

        private WorkspaceManager() { }

        public void Initialize()
        {
            workspaceElement = LayerManager.Instance.GetLayer( "Workspace" );
            
            if( workspaceElement == null )
                throw new ArgumentNullException( nameof( workspaceElement ) );

            workspaceElement.style.position = Position.Relative;
            workspaceElement.style.flexGrow = 1;
            workspaceElement.style.overflow = Overflow.Hidden;

            LayerManager.Instance.AddLayer("Workspace/GraphView/grid-layer", new LayerConfig { Order = 0 });
            LayerManager.Instance.AddLayer("Workspace/GraphView/edge-layer", new LayerConfig { Order = 1 });
            LayerManager.Instance.AddLayer("Workspace/GraphView/node-layer", new LayerConfig { Order = 2 });
            LayerManager.Instance.AddLayer("Workspace/GraphView/overlay-layer", new LayerConfig { Order = 3 });

        }

        /// <summary>
        /// Sets the active tab and displays its workspace.
        /// </summary>
        public void SetActiveTab( GraphTab tab )
        {
            if( tab == null ) throw new ArgumentNullException( nameof( tab ) );

            if( activeTab != null )
            {
                activeTab.SetActive( false );
                if( workspaceElement.Contains( activeTab.ContentContainer ) )
                {
                    workspaceElement.Remove( activeTab.ContentContainer );
                }
            }

            activeTab = tab;
            workspaceElement.Add( activeTab.ContentContainer );
            activeTab.SetActive( true );

            if( activeTab.GraphView != null )
            {
                activeTab.GraphView.style.position = Position.Absolute;
                activeTab.GraphView.style.left = 0;
                activeTab.GraphView.style.right = 0;
                activeTab.GraphView.style.top = 0;
                activeTab.GraphView.style.bottom = 0;
                activeTab.GraphView.style.overflow = Overflow.Hidden;
            }
        }

        /// <summary>
        /// Clears the active tab's workspace without deleting its data.
        /// </summary>
        public void ClearWorkspace()
        {
            if( activeTab != null )
            {
                activeTab.SetActive( false );
                if( workspaceElement.Contains( activeTab.ContentContainer ) )
                {
                    workspaceElement.Remove( activeTab.ContentContainer );
                }
            }

            activeTab = null;
        }

        /// <summary>
        /// Gets the currently active tab.
        /// </summary>
        public GraphTab GetActiveTab()
        {
            return activeTab;
        }

        /// <summary>
        /// Checks if a specific tab exists.
        /// </summary>
        public bool HasTab( string name )
        {
            return activeTab != null && activeTab.Name == name;
        }

        /// <summary>
        /// Toggles the visibility of the workspace.
        /// </summary>
        public void ToggleWorkspaceVisibility()
        {
            workspaceElement.style.display =
                workspaceElement.style.display == DisplayStyle.Flex
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
        }
    }


}