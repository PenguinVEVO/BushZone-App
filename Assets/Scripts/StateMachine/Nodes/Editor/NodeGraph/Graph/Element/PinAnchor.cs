using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class PinAnchor : VisualElement, IEdgeConnector
    {
        public ReroutePinElement ParentPin { get; private set; }
        public EdgeElement ConnectedEdge { get; private set; }

        private readonly VisualElement connectorCap;
        private readonly HashSet<EdgeElement> incomingEdges = new();
        private readonly PinAnchorEventHandler inputHandler;

        public IEnumerable<EdgeElement> Connections =>
            ConnectedEdge != null ? new[] { ConnectedEdge } : incomingEdges;

        public bool Connected => ConnectedEdge != null || incomingEdges.Count > 0;

        public VisualElement ConnectorCap => connectorCap;

        public PinAnchor(ReroutePinElement parent)
        {
            ParentPin = parent ?? throw new ArgumentNullException(nameof(parent));

            AddToClassList("reroute-anchor");
            pickingMode = PickingMode.Position;
            focusable = true;

            connectorCap = new VisualElement { name = "connector-cap" };
            connectorCap.AddToClassList("connector-cap");
            Add(connectorCap);

            inputHandler = new PinAnchorEventHandler(this);
            InputManager.Instance.RegisterElement(connectorCap);
            InputManager.Instance.SetEventHandler(connectorCap, inputHandler);
        }

        public Vector2 GetAnchorWorldPosition()
        {
            var graphView = this.GetFirstAncestorOfType<StateGraphView>();
            return graphView != null
                ? graphView.WorldToLocal(connectorCap.worldBound.center)
                : worldBound.center;
        }

        public void Connect( EdgeElement edge )
        {
            if( edge == null ) return;

            // Handle as Output (one connection allowed)
            if( edge.OutputPort == null )
            {
                // Disconnect existing outgoing edge if it exists
                if( ConnectedEdge != null )
                {

                    ConnectedEdge.Disconnect();
                }

                ConnectedEdge = edge;
            }
            else
            {
                foreach ( var existing in incomingEdges.ToList() ) 
                {
                    existing.Disconnect();
                }

                incomingEdges.Clear();
                incomingEdges.Add( edge );
            }

            ParentPin?.AddEdge( edge );
            MarkDirtyRepaint();
        }


        public void Disconnect(EdgeElement edge)
        {
            if (edge == null) return;

            if (ConnectedEdge == edge)
            {
                ConnectedEdge = null;
            }
            else
            {
                incomingEdges.Remove(edge);
            }

            ParentPin?.RemoveEdge(edge);
            MarkDirtyRepaint();
        }

        public void SetAsInput()
        {
            RemoveFromClassList( "pin-output" );
            AddToClassList( "pin-input" );
        }

        public void SetAsOutput()
        {
            RemoveFromClassList( "pin-input" );
            AddToClassList( "pin-output" );
        }

        public void ResetVisualStyle()
        {
            RemoveFromClassList( "pin-input" );
            RemoveFromClassList( "pin-output" );
        }

        public void DisconnectAll()
        {
            ConnectedEdge = null;
            incomingEdges.Clear();
            MarkDirtyRepaint();
        }

        public void RefreshElement()
        {
            connectorCap.MarkDirtyRepaint();
        }
    }
}
