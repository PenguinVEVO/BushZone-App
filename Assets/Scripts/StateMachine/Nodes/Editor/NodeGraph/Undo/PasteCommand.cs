using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class PasteCommand : Command
    {
        private StateGraphView graphView;
        private NodeGraphData clipboardData;
        private Vector2 pastePosition;

        private List<NodeView> pastedNodes;
        private List<ReroutePinElement> pastedPins;
        private List<EdgeElement> pastedEdges;

        private Dictionary<string, string> idMapping;

        public PasteCommand(StateGraphView graphView, Vector2 position)
        {
            this.graphView = graphView;
            this.pastePosition = position;
            this.clipboardData = graphView.GetClipboardData();
        }

        public override void Execute()
        {
            if (clipboardData == null)
                return;

            pastedNodes = new List<NodeView>();
            pastedPins = new List<ReroutePinElement>();
            pastedEdges = new List<EdgeElement>();
            idMapping = new Dictionary<string, string>();

            Vector2 minPosition = GetMinPosition(clipboardData);
            Vector2 positionOffset = pastePosition - minPosition;

            foreach (var nodeData in clipboardData.nodes)
            {
                var adjustedNodeData = new NodeData
                {
                    id = nodeData.id,
                    name = nodeData.name,
                    nodeType = nodeData.nodeType,
                    position = nodeData.position + positionOffset,
                    priority = nodeData.priority,
                    executeOnce = nodeData.executeOnce,
                    properties = new List<Property>(nodeData.properties)
                };

                var newNode = GraphSerialization.Instance.DeserializeNodeFromData(adjustedNodeData, graphView, assignNewId: true);
                pastedNodes.Add(newNode);
                graphView.AddNode(newNode);

                idMapping[nodeData.id] = newNode.NodeID;
            }

            foreach (var visualData in clipboardData.visualElements)
            {
                var element = GraphElementFactory.CreateElementFromData(visualData);
                if (element is ReroutePinElement reroutePin)
                {
                    reroutePin.SetPosition(new Rect(visualData.position + positionOffset, reroutePin.layout.size));
                    pastedPins.Add(reroutePin);
                    graphView.AddVisualGraphElement(reroutePin);

                    idMapping[visualData.Id] = reroutePin.ElementID;
                }
                else
                {
                    Debug.LogWarning($"[PasteCommand] Unknown visual element type: {visualData.ElementType}");
                }
            }

           
            var nodeLookup = pastedNodes.ToDictionary(n => n.NodeID, n => n);
            var pinLookup = pastedPins.ToDictionary(p => p.ElementID, p => p);

            foreach (var edgeData in clipboardData.edges)
            {
                var remappedEdge = RemapEdge(edgeData, idMapping);

                var edge = graphView.CreateEdgeFromData(remappedEdge, nodeLookup, pinLookup);
                if (edge != null)
                {
                    pastedEdges.Add(edge);
                    graphView.AddEdge(edge);
                }
                else
                {
                    Debug.LogWarning($"[PasteCommand] Failed to create edge for: {edgeData.outputNodeId ?? edgeData.outputPinId} -> {edgeData.inputNodeId ?? edgeData.inputPinId}");
                }
            }

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        public override void Undo()
        {
            foreach (var edge in pastedEdges)
                graphView.RemoveEdge(edge);

            foreach (var pin in pastedPins)
                graphView.RemoveVisualGraphElement(pin);

            foreach (var node in pastedNodes)
                graphView.RemoveNode(node);

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        public override void Redo()
        {
            foreach (var node in pastedNodes)
                graphView.AddNode(node);

            foreach (var pin in pastedPins)
                graphView.AddVisualGraphElement(pin);

            foreach (var edge in pastedEdges)
                graphView.AddEdge(edge);

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        private Vector2 GetMinPosition(NodeGraphData data)
        {
            if (data.nodes.Count > 0)
                return new Vector2(data.nodes.Min(n => n.position.x), data.nodes.Min(n => n.position.y));
            if (data.visualElements.Count > 0)
                return new Vector2(data.visualElements.Min(v => v.position.x), data.visualElements.Min(v => v.position.y));
            return Vector2.zero;
        }

        private EdgeData RemapEdge(EdgeData original, Dictionary<string, string> idMap)
        {
            return new EdgeData
            {
                outputNodeId = !string.IsNullOrEmpty(original.outputNodeId) && idMap.TryGetValue(original.outputNodeId, out var newOutNode)
                    ? newOutNode
                    : original.outputNodeId,

                inputNodeId = !string.IsNullOrEmpty(original.inputNodeId) && idMap.TryGetValue(original.inputNodeId, out var newInNode)
                    ? newInNode
                    : original.inputNodeId,

                outputPortName = original.outputPortName,
                inputPortName = original.inputPortName,

                outputPinId = !string.IsNullOrEmpty(original.outputPinId) && idMap.TryGetValue(original.outputPinId, out var newOutPin)
                    ? newOutPin
                    : original.outputPinId,

                inputPinId = !string.IsNullOrEmpty(original.inputPinId) && idMap.TryGetValue(original.inputPinId, out var newInPin)
                    ? newInPin
                    : original.inputPinId
            };
        }
    }

}

