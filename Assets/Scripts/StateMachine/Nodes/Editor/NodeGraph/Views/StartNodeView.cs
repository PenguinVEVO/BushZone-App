using FrameLabs.AI.Nodes;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{

    public class StartNodeView : NodeView
    {
        private StartNode startNode;

        public override float Height => 105;
        public override float Width => 165;

        public StartNodeView( StartNode node, string id = null ) : base( node, "Start Node", id )
        {
            startNode = node ?? throw new ArgumentNullException( nameof( node ) );

            // Customize title appearance
            titleContainer.style.backgroundColor = new Color( 0.055f, 0.788f, 0.302f );

            var titleLabel = this.Q<Label>( "title-label" );
            if( titleLabel != null )
            {
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.fontSize = 16;
                titleLabel.style.color = Color.black;
            }

            CreateStartOutputPort();
        }

        /// <summary>
        /// Creates the "Start" output port.
        /// </summary>
        private void CreateStartOutputPort()
        {
            var outputPort = AddPort(this, "Start", PortDirection.Output, PortCapacity.Single, 0);
            outputPort.OnEdgeConnected += HandleEdgeConnected;
            outputPort.OnEdgeDisconnected += HandleEdgeDisconnected;
        }

        /// <summary>
        /// Called when an edge is connected to the output port.
        /// </summary>
        private void HandleEdgeConnected(EdgeElement edge)
        {
            var childView = edge.InputPort?.ParentView;
            if (childView != null)
            {
                startNode.MapExitCodeToChild(0, childView.NodeData);
            }
        }

        /// <summary>
        /// Called when an edge is removed from the output port.
        /// </summary>
        private void HandleEdgeDisconnected(EdgeElement edge)
        {
            startNode.RemoveExitCodeEntry(0);
        }
    }



}