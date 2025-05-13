using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;


namespace FrameLabs.Utilities.NodeEditor
{
    public class DeleteNodeCommand : Command
    {
        private readonly StateGraphView graphView;
        private List<NodeView> deletedNodes;
        private List<EdgeElement> deletedEdges;

        public DeleteNodeCommand(StateGraphView graphView)
        {
            this.graphView = graphView;
        }

        public override void Execute()
        {
            var selectedNodes = graphView.Selection.OfType<NodeView>().ToList();
            if (selectedNodes.Count == 0) return;

            deletedNodes = new List<NodeView>();
            deletedEdges = new List<EdgeElement>();
            var seenEdges = new HashSet<EdgeElement>();

            foreach (var node in selectedNodes)
            {
                foreach (var outputPort in node.GetOutputPorts())
                {
                    foreach (var edge in outputPort.Connections.ToList())
                    {
                        if (seenEdges.Add(edge))
                        {
                            var deleteLink = new DeleteLinkCommand(graphView, edge);
                            graphView.Undo.ExecuteCommand(deleteLink);
                            deletedEdges.Add(edge);
                        }
                    }
                }

                foreach (var inputPort in node.GetInputPorts())
                {
                    foreach (var edge in inputPort.Connections.ToList())
                    {
                        if (seenEdges.Add(edge))
                        {
                            var deleteLink = new DeleteLinkCommand(graphView, edge);
                            graphView.Undo.ExecuteCommand(deleteLink);
                            deletedEdges.Add(edge);
                        }
                    }
                }

                graphView.RemoveNode(node);
                deletedNodes.Add(node);
            }

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }
    
        public override void Undo()
        {
            graphView.AddElements(deletedNodes.Cast<GraphElement>());
            graphView.ReconnectEdges(deletedEdges);

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }


        public override void Redo()
        {
            foreach (var edge in deletedEdges)
            {
                graphView.RemoveEdge(edge);
                edge.Disconnect();
            }

            foreach (var node in deletedNodes)
            {
                graphView.RemoveNode(node);
            }

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }
    }

}