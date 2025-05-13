using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class CopyCommand : Command
    {
        private StateGraphView graphView;
        private NodeGraphData clipboardData;
        private List<ICopyableElement> selectedCopyables = new();

        private Dictionary<string, string> idMapping = new Dictionary<string, string>();

        public CopyCommand(StateGraphView graphView)
        {
            this.graphView = graphView;
        }

        public override void Execute()
        {
            graphView.SetClipboardData(null);
            clipboardData = ScriptableObject.CreateInstance<NodeGraphData>();
            idMapping.Clear();
            selectedCopyables.Clear();

            selectedCopyables = graphView.Selection.OfType<ICopyableElement>().ToList();

            if (selectedCopyables.Count == 0)
                return;

            foreach (var copyable in selectedCopyables)
            {
                var data = copyable.CreateCopyData(copyId: false);

                if (data is NodeData nodeData && copyable is NodeView originalNode)
                {
                    clipboardData.nodes.Add(nodeData);
                    idMapping[originalNode.NodeID] = nodeData.id;
                }
                else if (data is VisualElementData visualData && copyable is ReroutePinElement originalPin)
                {
                    clipboardData.visualElements.Add(visualData);
                    idMapping[originalPin.ElementID] = visualData.Id;
                }
                else
                {
                    Debug.LogWarning($"[CopyCommand] Unknown or unsupported copyable type: {data?.GetType().Name}");
                }
            }

            var selectedNodes = selectedCopyables.OfType<NodeView>().ToList();
            var selectedPins = selectedCopyables.OfType<ReroutePinElement>().ToList();
            var selectedEdges = GetSelectedEdgesBetweenElements(selectedNodes, selectedPins);

            foreach (var oldEdge in selectedEdges)
            {
                var remappedEdge = RemapEdge(oldEdge);
                if (remappedEdge != null)
                    clipboardData.edges.Add(remappedEdge);
            }

            graphView.SetClipboardData(clipboardData);
        }

        public override void Undo()
        {
            graphView.SetClipboardData(null);
        }

        public override void Redo()
        {
            graphView.SetClipboardData(clipboardData);
        }


        private EdgeData RemapEdge(EdgeData oldEdge)
        {
            return new EdgeData
            {
                outputNodeId = !string.IsNullOrEmpty(oldEdge.outputNodeId) && idMapping.TryGetValue(oldEdge.outputNodeId, out var newOutNodeId)
                    ? newOutNodeId
                    : oldEdge.outputNodeId,

                inputNodeId = !string.IsNullOrEmpty(oldEdge.inputNodeId) && idMapping.TryGetValue(oldEdge.inputNodeId, out var newInNodeId)
                    ? newInNodeId
                    : oldEdge.inputNodeId,

                outputPortName = oldEdge.outputPortName,
                inputPortName = oldEdge.inputPortName,

                outputPinId = !string.IsNullOrEmpty(oldEdge.outputPinId) && idMapping.TryGetValue(oldEdge.outputPinId, out var newOutPinId)
                    ? newOutPinId
                    : oldEdge.outputPinId,

                inputPinId = !string.IsNullOrEmpty(oldEdge.inputPinId) && idMapping.TryGetValue(oldEdge.inputPinId, out var newInPinId)
                    ? newInPinId
                    : oldEdge.inputPinId
            };
        }

        private List<EdgeData> GetSelectedEdgesBetweenElements(List<NodeView> selectedNodes, List<ReroutePinElement> selectedPins)
        {
            var edgeDataList = new List<EdgeData>();
            var nodeSet = new HashSet<NodeView>(selectedNodes);
            var pinSet = new HashSet<ReroutePinElement>(selectedPins);

            // Scan node output ports
            foreach (var node in selectedNodes)
            {
                foreach (var outputPort in node.GetOutputPorts())
                {
                    if (outputPort?.Connections == null)
                        continue;

                    foreach (var edge in outputPort.Connections)
                    {
                        TryAddEdge(edge, nodeSet, pinSet, edgeDataList);
                    }
                }
            }

            // Scan reroute pin edges
            foreach (var pin in selectedPins)
            {
                foreach (var edge in pin.GetConnectedEdges())
                {
                    TryAddEdge(edge, nodeSet, pinSet, edgeDataList);
                }
            }

            return edgeDataList;
        }

        private void TryAddEdge(EdgeElement edge, HashSet<NodeView> nodeSet, HashSet<ReroutePinElement> pinSet, List<EdgeData> edgeList)
        {
            NodeView outputNode = null;
            ReroutePinElement outputPin = null;
            if (edge.OutputConnector is PortElement outputPort)
                outputNode = outputPort.ParentView;
            else if (edge.OutputConnector is PinAnchor outputAnchor)
                outputPin = outputAnchor.ParentPin;

            NodeView inputNode = null;
            ReroutePinElement inputPin = null;
            if (edge.InputConnector is PortElement inputPort)
                inputNode = inputPort.ParentView;
            else if (edge.InputConnector is PinAnchor inputAnchor)
                inputPin = inputAnchor.ParentPin;

            bool outputSelected = (outputNode != null && nodeSet.Contains(outputNode)) || (outputPin != null && pinSet.Contains(outputPin));
            bool inputSelected = (inputNode != null && nodeSet.Contains(inputNode)) || (inputPin != null && pinSet.Contains(inputPin));

            if (outputSelected && inputSelected)
            {
                var edgeData = GraphSerialization.Instance.CreateEdgeData(edge);
                if (edgeData != null)
                    edgeList.Add(edgeData);
            }
        }
    }
}
