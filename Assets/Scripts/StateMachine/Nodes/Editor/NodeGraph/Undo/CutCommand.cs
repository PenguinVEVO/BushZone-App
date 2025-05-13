using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class CutCommand : Command
    {
        private StateGraphView graphView;
        private NodeGraphData clipboardData;

        private List<NodeView> cutNodes;
        private List<ReroutePinElement> cutPins;
        private List<EdgeElement> cutEdges;

        public CutCommand(StateGraphView graphView)
        {
            this.graphView = graphView;
        }

        public override void Execute()
        {
            cutNodes = graphView.Selection.OfType<NodeView>().ToList();
            cutPins = graphView.Selection.OfType<ReroutePinElement>().ToList();

            if (cutNodes.Count == 0 && cutPins.Count == 0)
                return;

            clipboardData = ScriptableObject.CreateInstance<NodeGraphData>();

            foreach (var node in cutNodes)
            {
                var nodeData = GraphSerialization.Instance.CreateNodeData(node);
                if (nodeData != null)
                    clipboardData.nodes.Add(nodeData);
            }

            foreach (var pin in cutPins)
            {
                var visualData = GraphSerialization.Instance.CreateVisualElementData(pin);
                if (visualData != null)
                    clipboardData.visualElements.Add(visualData);
            }

            cutEdges = graphView.GetAllConnectedEdges(cutNodes.Cast<NodeElement>().Concat(cutPins.Cast<NodeElement>()).ToList());

            foreach (var edge in cutEdges)
            {
                var edgeData = GraphSerialization.Instance.CreateEdgeData(edge);
                if (edgeData != null)
                    clipboardData.edges.Add(edgeData);
            }

            graphView.SetClipboardData(clipboardData);

            graphView.DeleteElements(cutNodes.Concat<GraphElement>(cutPins).Concat(cutEdges));
        }

        public override void Undo()
        {
            graphView.AddElements(cutNodes.Cast<GraphElement>().Concat(cutPins.Cast<GraphElement>()));

            graphView.ReconnectEdges(cutEdges);
            graphView.AddElements(cutEdges.Cast<GraphElement>());

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        public override void Redo()
        {
            graphView.DeleteElements(cutNodes.
                Concat<GraphElement>(cutPins).Concat(cutEdges));
        }
    }
}
