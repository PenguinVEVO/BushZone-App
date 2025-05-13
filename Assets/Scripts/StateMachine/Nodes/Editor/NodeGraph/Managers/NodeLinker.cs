using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class NodeLinker
    {
        private StateGraphView graphView;
        private EdgeElement activeEdge;
        private Dictionary<string, EdgeElement> edgeDictionary = new();

        private Vector2 initialMousePos;
        private IEdgeConnector pendingStartConnector;
        private const float dragThreshold = 5f;
        public bool IsLinking => activeEdge != null;

        public NodeLinker( StateGraphView graphView )
        {
            this.graphView = graphView;
        }

        public void StartLinkFromPort(PortElement port, Vector2 mousePosition)
        {
            if (port == null) return;
            StartLink(port, mousePosition);
        }

        public void StartLinkFromAnchor(PinAnchor anchor, Vector2 mousePosition)
        {
            if (anchor == null) return;
            StartLink(anchor, mousePosition);
        }

        private void StartLink(IEdgeConnector connector, Vector2 mousePosition)
        {
            if (activeEdge != null) return;

            pendingStartConnector = connector;
            initialMousePos = mousePosition;
        }

        public void UpdateLinkPosition( Vector2 mousePosition )
        {
            if( pendingStartConnector == null ) return;

            if( activeEdge == null )
            {
                if( Vector2.Distance( mousePosition, initialMousePos ) < dragThreshold ) return;

                activeEdge = new EdgeElement(pendingStartConnector, null, isTemporary: true );
                graphView.AddEdge( activeEdge );
            }

            activeEdge.UpdateFloatingEnd( mousePosition, graphView );
            activeEdge.UpdateEdge();
            graphView.MarkDirtyRepaint();
        }

        public void RegisterEdge( string key, EdgeElement edge )
        {
            if( !string.IsNullOrEmpty( key ) )
                edgeDictionary[ key ] = edge;
        }

        public void UnregisterEdge( string key )
        {
            if( !string.IsNullOrEmpty( key ) )
                edgeDictionary.Remove( key );
        }

        public void FinalizeLink(IEdgeConnector target)
        {
            if (activeEdge == null || target == null)
            {
                CancelActiveLink();
                return;
            }

            if (TryCreateFinalizedCommand(pendingStartConnector, target, out var command))
            {
                activeEdge.RemoveFromHierarchy();
                graphView.Undo.ExecuteCommand(command);
            }
            else
            {
                graphView.OwningTab.ShowEditorMessage("[NodeLinker] Invalid connection attempt.", MessageCategory.Warning);
            }

            ClearState();
        }

        public void CancelActiveLink()
        {
            if (activeEdge != null)
            {
                graphView.RemoveEdge(activeEdge);
            }

            ClearState();
        }

        private bool TryCreateFinalizedCommand(IEdgeConnector from, IEdgeConnector to, out Command command)
        {
            command = null;

            switch (from, to)
            {
                case (PortElement a, PortElement b) when b.Direction == PortDirection.Input:
                    command = new CreateLinkCommand(a, b, graphView);
                    return true;

                case (PortElement a, PinAnchor b):
                    command = new CreateLinkCommand(a, b, graphView);
                    return true;

                case (PinAnchor a, PortElement b) when b.Direction == PortDirection.Input:
                    command = new CreateLinkCommand(a, b, graphView);
                    return true;

                case (PinAnchor a, PinAnchor b):
                    command = new CreateLinkCommand(a, b, graphView);
                    return true;
            }

            return false;
        }


        private void ClearState()
        {
            activeEdge = null;
            pendingStartConnector = null;
        }

    }


}

