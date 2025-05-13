using FrameLabs.AI.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public class ParsedGraph
    {
        public Dictionary<string, ParsedNode> nodes;
        public ParsedNode RootNode;
    }

    public class ParsedNode
    {
        public string ID;
        public string Title;
        public Type NodeType;
        public NodeView View;
        public Node RuntimeNode;
        public List<(int BranchIndex, string ChildID)> Children = new();
    }

    public class GraphParser
    {
        private readonly Dictionary<string, ParsedNode> parsedNodes = new Dictionary<string, ParsedNode>();
        private readonly HashSet<string> visited = new HashSet<string>();

        private void Traverse(NodeView nodeView, StateGraphView graphView)
        {
            if (nodeView == null || visited.Contains(nodeView.NodeID))
                return;

            visited.Add(nodeView.NodeID);

            var parsedNode = new ParsedNode
            {
                ID = nodeView.NodeID,
                Title = nodeView.Title,
                NodeType = nodeView.NodeData.GetType(),
                View = nodeView,
                RuntimeNode = nodeView.NodeData,
                Children = new List<(int BranchIndex, string ChildID)>()
            };

            int branchIndex = 0;
            foreach (var outputPort in nodeView.GetOutputPorts())
            {
                foreach (var edge in outputPort.Connections)
                {
                    var terminalNode = ResolveTerminalNodeFromConnector(edge.InputConnector, new HashSet<IEdgeConnector>());
                    if (terminalNode != null)
                    {
                        parsedNode.Children.Add((branchIndex, terminalNode.NodeID));
                        Traverse(terminalNode, graphView);
                    }
                }
                branchIndex++;
            }

            parsedNodes[nodeView.NodeID] = parsedNode;
        }

        private NodeView ResolveTerminalNodeFromConnector(IEdgeConnector connector, HashSet<IEdgeConnector> visited)
        {
            if (connector == null || !visited.Add(connector))
                return null;

            if (connector is PortElement port && port.Direction == PortDirection.Input)
                return port.ParentView;

            if (connector is PinAnchor pinAnchor)
            {
                foreach (var edge in pinAnchor.ParentPin.GetConnectedEdges())
                {
                    var next = edge.OutputConnector == pinAnchor
                        ? edge.InputConnector
                        : edge.OutputConnector;

                    var resolved = ResolveTerminalNodeFromConnector(next, visited);
                    if (resolved != null)
                        return resolved;
                }
            }

            return null;
        }


        public Dictionary<string, ParsedNode> Parse( StateGraphView graphView )
        {
            parsedNodes.Clear();
            visited.Clear();

            var startNode = graphView.nodes.OfType<NodeView>()
                .FirstOrDefault( n => n.NodeData is StartNode );

            if( startNode == null )
            {
                graphView.OwningTab.ShowEditorMessage( "[GraphParser] Missing StartNode. Parsing failed.", MessageCategory.Error );                
                return null;
            }

            Traverse( startNode, graphView );
            return parsedNodes;
        }
    }
}