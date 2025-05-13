using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class EdgeSerializer
    {
        private readonly List<EdgeData> deferredEdges = new();

        public EdgeSerializer() { }

        /// <summary>
        /// Serializes all edges in the graph view.
        /// </summary>
        public List<EdgeData> SerializeEdges(StateGraphView graphView)
        {
            return graphView.edges
                .OfType<EdgeElement>()
                .Where(edge => edge.OutputConnector != null && edge.InputConnector != null)
                .Select(CreateEdgeData)
                .ToList();
        }

        /// <summary>
        /// Deserializes edges and reconstructs connections between nodes.
        /// If nodes are missing, edges are stored for later resolution.
        /// </summary>
        public void DeserializeEdges(NodeGraphData graphData, Dictionary<string, NodeView> nodeViewsById, StateGraphView graphView)
        {
            deferredEdges.Clear();
            graphView.RemoveAllEdges();

            // Cache pin lookup by ID
            var pinById = graphView.visualElements
                .OfType<ReroutePinElement>()
                .ToDictionary(p => p.ID, p => p);

            foreach (EdgeData edgeData in graphData.edges)
            {
                bool outputExists = !string.IsNullOrEmpty(edgeData.outputPinId) || nodeViewsById.ContainsKey(edgeData.outputNodeId);
                bool inputExists = !string.IsNullOrEmpty(edgeData.inputPinId) || nodeViewsById.ContainsKey(edgeData.inputNodeId);

                if (!outputExists || !inputExists)
                {
                    graphView.OwningTab.ShowEditorMessage($"[EdgeSerializer] Edge '{edgeData.outputNodeId}' -> '{edgeData.inputNodeId}' could not be restored. Connectors missing.", MessageCategory.Error);
                    deferredEdges.Add(edgeData);
                    continue;
                }

                TryCreateEdge(edgeData, nodeViewsById, pinById, graphView);
            }

            AttemptDeferredEdgeRestoration(nodeViewsById, graphView);
        }



        /// <summary>
        /// Attempts to reconnect edges that were missing nodes during initial deserialization.
        /// </summary>
        public void AttemptDeferredEdgeRestoration(Dictionary<string, NodeView> nodeViewsById, StateGraphView graphView)
        {
            List<EdgeData> successfullyRestored = new();

            // Cache reroute pin map from the graph
            var pinById = graphView.visualElements
                .OfType<ReroutePinElement>()
                .ToDictionary(p => p.ID, p => p);


            foreach (var edgeData in deferredEdges)
            {
                bool outputExists = !string.IsNullOrEmpty(edgeData.outputPinId) || nodeViewsById.ContainsKey(edgeData.outputNodeId);
                bool inputExists = !string.IsNullOrEmpty(edgeData.inputPinId) || nodeViewsById.ContainsKey(edgeData.inputNodeId);

                if (!outputExists || !inputExists)
                    continue;

                if (TryCreateEdge(edgeData, nodeViewsById, pinById, graphView))
                {
                    successfullyRestored.Add(edgeData);
                }
            }

            foreach (var restored in successfullyRestored)
            {
                deferredEdges.Remove(restored);
            }
        }


        /// <summary>
        /// Attempts to create an edge between two nodes using edge data.
        /// </summary>
        private bool TryCreateEdge(EdgeData edgeData, Dictionary<string, NodeView> nodeViewsById, 
            Dictionary<string, ReroutePinElement> pinById,StateGraphView graphView)
        {
            // Resolve the output and input connectors independently
            IEdgeConnector outputConnector = ResolveOutputConnector(edgeData, pinById, nodeViewsById);
            IEdgeConnector inputConnector = ResolveInputConnector(edgeData, pinById, nodeViewsById);

            // If any connector is null, log and return false
            if (outputConnector == null || inputConnector == null)
            {
                Debug.LogWarning($"[EdgeSerializer] Missing connector for edge. Output: {edgeData.outputNodeId} -> Input: {edgeData.inputNodeId}");
                return false;
            }

            // Prevent duplicate edges
            if (IsDuplicateEdge(outputConnector))
            {
                return false;
            }

            // Create the edge and add it to the graph
            return CreateAndAddEdge(outputConnector, inputConnector, graphView);
        }

        private IEdgeConnector ResolveOutputConnector(EdgeData edgeData,
                                                      Dictionary<string, ReroutePinElement> pinById,
                                                      Dictionary<string, NodeView> nodeViewsById)
        {
            // Check if the output connector is a pin
            if (!string.IsNullOrEmpty(edgeData.outputPinId) && pinById.TryGetValue(edgeData.outputPinId, out var outputPin))
            {
                return outputPin.GetOutputAnchor() as IEdgeConnector;
            }

            // If it's not a pin, check if it's a node
            if (!string.IsNullOrEmpty(edgeData.outputNodeId) && nodeViewsById.TryGetValue(edgeData.outputNodeId, out var outputNode))
            {
                return outputNode.GetPort(edgeData.outputPortName, PortDirection.Output);
            }

            return null;
        }

        private IEdgeConnector ResolveInputConnector(EdgeData edgeData,
                                                     Dictionary<string, ReroutePinElement> pinById,
                                                     Dictionary<string, NodeView> nodeViewsById)
        {
            // Check if the input connector is a pin
            if (!string.IsNullOrEmpty(edgeData.inputPinId) && pinById.TryGetValue(edgeData.inputPinId, out var inputPin))
            {
                return inputPin.GetInputAnchor() as IEdgeConnector;
            }

            // If it's not a pin, check if it's a node
            if (!string.IsNullOrEmpty(edgeData.inputNodeId) && nodeViewsById.TryGetValue(edgeData.inputNodeId, out var inputNode))
            {
                return inputNode.GetPort(edgeData.inputPortName, PortDirection.Input);
            }

            return null;
        }

        private bool IsDuplicateEdge(IEdgeConnector outputConnector)
        {
            if (outputConnector is PortElement port && port.Connected)
            {
                Debug.LogWarning($"[EdgeSerializer] Skipping duplicate edge. Output port {port.PortName} is already connected.");
                return true;
            }

            return false;
        }

        private bool CreateAndAddEdge(IEdgeConnector outputConnector, IEdgeConnector inputConnector, StateGraphView graphView)
        {
            var edge = new EdgeElement(outputConnector, inputConnector);

            // Connect the output and input connectors
            ConnectEdgeToConnector(outputConnector, edge);
            ConnectEdgeToConnector(inputConnector, edge);

            // Add the edge to the graph
            graphView.AddEdge(edge);
            edge.RegisterWithPins();
            edge.schedule.Execute(edge.UpdateEdge);

            return true;
        }

        private void ConnectEdgeToConnector(IEdgeConnector connector, EdgeElement edge)
        {
            if (connector is PortElement port)
            {
                port.Connect(edge);
            }
            else if (connector is PinAnchor pin)
            {
                pin.Connect(edge);
            }
        }

        /// <summary>
        /// Converts an EdgeElement into EdgeData for serialization.
        /// </summary>
        public EdgeData CreateEdgeData(EdgeElement edge)
        {
            var data = new EdgeData();

            if (edge.OutputPort != null)
            {
                data.outputNodeId = edge.OutputPort.ParentView?.NodeID;
                data.outputPortName = edge.OutputPort.PortName;
            }
            else if (edge.OutputConnector is PinAnchor outputPin)
            {
                data.outputPinId = outputPin.ParentPin?.ID;
            }

            if (edge.InputPort != null)
            {
                data.inputNodeId = edge.InputPort.ParentView?.NodeID;
                data.inputPortName = edge.InputPort.PortName;
            }
            else if (edge.InputConnector is PinAnchor inputPin)
            {
                data.inputPinId = inputPin.ParentPin?.ID;
            }

            return data;
        }

    }

}