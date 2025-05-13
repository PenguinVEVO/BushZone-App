using FrameLabs.AI.Nodes;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class ExitNodeView : NodeView
    {
        private ExitNode exitNode;
        private Toggle restartTreeToggle;

        public override float Height => 105;
        public override float Width => 175;

        public ExitNodeView( ExitNode node, string id = null ) : base( node, "Exit Node", id )
        {
            exitNode = node ?? throw new ArgumentNullException( nameof( node ) );

            // Customize the title container
            titleContainer.style.backgroundColor = new Color( 0.761f, 0.055f, 0.055f );

            var titleLabel = this.Q<Label>( "title-label" );
            if( titleLabel != null )
            {
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.fontSize = 14;
                titleLabel.style.color = Color.black;
            }

            // Create input/output ports
            CreateExitInputPort();
            CreateExitOutputPort();

            
            settingsContainer = this.Q("node-settings");
            settingsContainer.style.fontSize = 12;

            // Create Restart Tree toggle
            AddSettingsProperties(CreateRestartTreeToggle());
        }

        /// <summary>
        /// Creates the "Input" port for entering the ExitNode.
        /// </summary>
        private void CreateExitInputPort()
        {
            var inputPort = AddPort(this, "Input", PortDirection.Input, PortCapacity.Multi, 0);
            inputPort.OnEdgeConnected += OnEdgeCreated;
            inputPort.OnEdgeDisconnected += OnEdgeRemoved;
        }

        /// <summary>
        /// Creates the "Exit" output port.
        /// </summary>
        private void CreateExitOutputPort()
        {
            var outputPort = AddPort(this, "Exit", PortDirection.Output, PortCapacity.Single, 0);
            outputPort.OnEdgeConnected += OnEdgeCreated;
            outputPort.OnEdgeDisconnected += OnEdgeRemoved;
        }

        /// <summary>
        /// Creates the toggle UI element for restarting the tree.
        /// </summary>
        private Toggle CreateRestartTreeToggle()
        {
            restartTreeToggle = new Toggle( "Restart Tree" )
            {
                value = exitNode.RestartTree
            };

            restartTreeToggle.style.marginLeft = -1;
            restartTreeToggle.style.paddingLeft = 0;
            restartTreeToggle.style.marginTop = 2;

            restartTreeToggle.RegisterValueChangedCallback(evt =>
            {
                exitNode.SetRestartTree(evt.newValue);
            });

            return restartTreeToggle;
        }

        public void OnEdgeCreated( EdgeElement edge )
        {
            if (edge.OutputPort == null || edge.InputPort == null) 
                return;

            var childView = edge.InputPort.ParentView;

            if (childView != null)
            {
                int branchIndex = edge.OutputPort.BranchIndex;
                exitNode.MapExitCodeToChild(branchIndex, childView.NodeData);
            }
        }

        public void OnEdgeRemoved( EdgeElement edge )
        {
            if (edge.OutputPort == null)
                return;

            int branchIndex = edge.OutputPort.BranchIndex;
            exitNode.RemoveExitCodeEntry(branchIndex);         
        }
    }


}