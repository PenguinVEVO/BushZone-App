using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class NodeSerializer
    {
        public NodeSerializer() { }

        /// <summary>
        /// Serializes all nodes in a given graph view.
        /// </summary>
        public List<NodeData> SerializeNodes(StateGraphView graph)
        {
            return graph.nodes
                .OfType<NodeView>()
                .Select(GraphSerialization.Instance.CreateNodeData)
                .ToList();
        }

        /// <summary>
        /// Deserializes nodes from NodeGraphData and restores them into a StateGraphView.
        /// </summary>
        public Dictionary<string, NodeView> DeserializeNodes(NodeGraphData graphData, StateGraphView graphView)
        {
            Dictionary<string, NodeView> nodeViewsById = new();

            foreach (NodeData nodeData in graphData.nodes)
            {
                NodeView nodeView = GraphSerialization.Instance.CreateNodeView(nodeData);
                if (nodeView != null)
                {
                    nodeViewsById[nodeData.id] = nodeView;
                    graphView.AddNode(nodeView);
                }
            }

            return nodeViewsById;
        }

        /// <summary>
        /// Deserializes visual-only elements (e.g., reroute pins) from NodeGraphData.
        /// </summary>
        public void DeserializeVisualElements(NodeGraphData graphData, StateGraphView graphView)
        {
            foreach (var data in graphData.visualElements)
            {
                GraphWidget visualElement = GraphElementFactory.CreateElementFromData(data);

                if (visualElement != null)
                {
                    graphView.AddVisualGraphElement(visualElement);
                }
            }
        }

        public List<VisualElementData> SerializeVisualElements(StateGraphView graphView)
        {
            return graphView.visualElements
                .OfType<GraphWidget>()
                .Select(GraphSerialization.Instance.CreateVisualElementData)
                .Where(data => data != null)
                .ToList();
        }

        /// <summary>
        /// Deserializes a single node from NodeData and adds it to the graph.
        /// </summary>
        public NodeView DeserializeNodeFromData(NodeData nodeData, StateGraphView graphView, bool assignNewId = false)
        {
            if (nodeData == null)
            {
                Debug.LogError("[NodeSerializer] NodeData is null. Cannot deserialize.");
                return null;
            }

            NodeView nodeView = GraphSerialization.Instance.CreateNodeView(nodeData);
            if (nodeView == null)
            {
                Debug.LogError("[NodeSerializer] Failed to create NodeView from node data.");
                return null;
            }

            if (assignNewId)
            {
                nodeView.NodeID = Guid.NewGuid().ToString(); 
            }

            graphView.AddNode(nodeView);
            return nodeView;
        }
    }

}