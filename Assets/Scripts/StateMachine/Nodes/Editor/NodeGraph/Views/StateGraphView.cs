using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;

namespace FrameLabs.Utilities.NodeEditor
{
    public class StateGraphView : NodeGraphView
    {
        private UndoManager undoManager = new();
        private Vector2 mousePosition = Vector2.zero;
        private NodeGraphData clipboardData;
        private GraphViewEventHandler inputHandler;
        private NodeLinker nodeLinker;
        private GraphTab owningTab;

        public void SetClipboardData(NodeGraphData data) => clipboardData = data;
        public NodeGraphData GetClipboardData() => clipboardData;
        public NodeLinker Linker => nodeLinker;

        public StateGraphView(GraphTab tab)
        {
            this.AddManipulator( new ContextualMenuManipulator( evt =>
            {
                evt.menu.AppendAction( "Create Action Node", action => inputHandler.CreateActionNode() );
                evt.menu.AppendAction( "Create Start Node", selector => inputHandler.CreateStartNode() );
                evt.menu.AppendAction( "Create Exit Node", selector => inputHandler.CreateExitNode() );
                evt.menu.AppendAction("Create Reroute Pin", selector => inputHandler.CreatePin());

            }) );

            owningTab = tab;
            graphViewChanged = OnGraphViewChanged;

            nodeLinker = new NodeLinker( this );

            inputHandler = new GraphViewEventHandler( this );
            InputManager.Instance.RegisterElement(Grid);
            InputManager.Instance.SetEventHandler(Grid, inputHandler);

        }

        public GraphViewEventHandler GraphViewEvent => inputHandler;

        public GraphTab OwningTab
        {
            get {  return owningTab; }
        }

        public Vector2 MousePosition
        {
            get { return mousePosition; }
            set { mousePosition = value; }
        }

        public UndoManager Undo
        {
            get { return undoManager; }
        }

        private NodeGraphViewChanged OnGraphViewChanged( NodeGraphViewChanged graphViewChange )
        {
            if( graphViewChange.edgesToCreate != null )
            {
                foreach( EdgeElement edge in graphViewChange.edgesToCreate )
                {
                    var createEdgeCommand = new CreateLinkCommand( edge.OutputPort, edge.InputPort, this );
                    undoManager.ExecuteCommand( createEdgeCommand );
                }
                graphViewChange.edgesToCreate.Clear();
            }

            if( graphViewChange.elementsToRemove != null )
            {
                foreach( GraphElement element in graphViewChange.elementsToRemove )
                {
                    if( element is EdgeElement edge )
                    {
                        var removeEdgeCommand = new DeleteLinkCommand(this, edge);
                        undoManager.ExecuteCommand( removeEdgeCommand );
                    }
                }
            }

            return graphViewChange;
        }

        public void RemoveAllEdges()
        {
            foreach( var edge in edges.ToList() )
            {
                edge.OutputPort?.Disconnect( edge );
                edge.InputPort?.Disconnect( edge );
                RemoveEdge( edge );
            }

            edges.Clear();
            MarkDirtyRepaint();
        }

        public void ReconnectEdges(IEnumerable<EdgeElement> edges)
        {
            if (edges == null) return;

            foreach (var edge in edges)
            {
                if (edge == null)
                    continue;


                if (edge.OutputConnector == null || edge.InputConnector == null)
                {
                    Debug.LogWarning("[ReconnectEdges] Skipping edge with missing connectors.");
                    continue;
                }

                var recreateCommand = new CreateLinkCommand(
                    edge.OutputConnector,
                    edge.InputConnector,
                    this
                );

                Undo.ExecuteCommand(recreateCommand);

                edge.UpdateEdge();
                edge.MarkDirtyRepaint();
            }
        }

        public List<EdgeElement> GetAllConnectedEdges(List<NodeElement> elements)
        {
            var connectedEdges = new List<EdgeElement>();

            foreach (var element in elements)
            {
                switch (element)
                {
                    case NodeView nodeView:
                        foreach (var outputPort in nodeView.GetOutputPorts())
                        {
                            if (outputPort?.Connections != null)
                                connectedEdges.AddRange(outputPort.Connections);
                        }
                        foreach (var inputPort in nodeView.GetInputPorts())
                        {
                            if (inputPort?.Connections != null)
                                connectedEdges.AddRange(inputPort.Connections);
                        }
                        break;

                    case ReroutePinElement reroutePin:
                        foreach (var edge in reroutePin.GetConnectedEdges())
                        {
                            if (edge != null)
                                connectedEdges.Add(edge);
                        }
                        break;
                }
            }

            return connectedEdges.Distinct().ToList();
        }

        public EdgeElement GetEdgeByData( EdgeData edgeData )
        {
            return edges
                .OfType<EdgeElement>()
                .FirstOrDefault( edge =>
                {
                    var outputNodeView = edge.OutputPort.ParentView;
                    var inputNodeView = edge.InputPort.ParentView;

                    if( outputNodeView == null || inputNodeView == null )
                        return false;

                    return outputNodeView.NodeID == edgeData.outputNodeId &&
                           edge.OutputPort.PortName == edgeData.outputPortName &&
                           inputNodeView.NodeID == edgeData.inputNodeId &&
                           edge.InputPort.PortName == edgeData.inputPortName;
                } );
        }

     
        public EdgeElement CreateEdgeFromData(EdgeData edgeData,
            Dictionary<string, NodeView> nodeViewsById,
            Dictionary<string, ReroutePinElement> pinsById)
        {
            IEdgeConnector outputConnector = null;
            IEdgeConnector inputConnector = null;

            if (!string.IsNullOrEmpty(edgeData.outputNodeId) && nodeViewsById.TryGetValue(edgeData.outputNodeId, out var outputNode))
            {
                outputConnector = outputNode.GetOutputPorts()
                    .FirstOrDefault(port => port.PortName == edgeData.outputPortName);
            }

            if (!string.IsNullOrEmpty(edgeData.inputNodeId) && nodeViewsById.TryGetValue(edgeData.inputNodeId, out var inputNode))
            {
                inputConnector = inputNode.GetInputPorts()
                    .FirstOrDefault(port => port.PortName == edgeData.inputPortName);
            }

            if (outputConnector == null && !string.IsNullOrEmpty(edgeData.outputPinId) && pinsById.TryGetValue(edgeData.outputPinId, out var outputPin))
            {
                outputConnector = outputPin.GetOutputAnchor() as IEdgeConnector;
            }

            if (inputConnector == null && !string.IsNullOrEmpty(edgeData.inputPinId) && pinsById.TryGetValue(edgeData.inputPinId, out var inputPin))
            {
                inputConnector = inputPin.GetInputAnchor() as IEdgeConnector;
            }

            if (outputConnector != null && inputConnector != null)
            {
                var edge = new EdgeElement(outputConnector, inputConnector);

                outputConnector.Connect(edge);
                inputConnector.Connect(edge);

                edge.UpdateEdge();
                return edge;
            }

            Debug.LogWarning($"[CreateEdgeFromData] Could not create edge for EdgeData: output={edgeData.outputNodeId ?? edgeData.outputPinId}, input={edgeData.inputNodeId ?? edgeData.inputPinId}");
            return null;
        }


        public List<EdgeData> GetSelectedEdges( List<NodeView> selectedNodes )
        {
            return edges
                .OfType<EdgeElement>()
                .Where( edge => selectedNodes.Contains( edge.OutputPort.ParentView ) && selectedNodes.Contains( edge.InputPort.ParentView ) )
                .Select( edge => new EdgeData
                {
                    outputNodeId = edge.OutputPort.ParentView.NodeID,
                    outputPortName = edge.OutputPort.PortName,
                    inputNodeId = edge.InputPort.ParentView.NodeID,
                    inputPortName = edge.InputPort.PortName
                } ).ToList();
        }

        public void ClearView() => ClearGraph();
        public bool HasContent() => Children().Any();
    }

}