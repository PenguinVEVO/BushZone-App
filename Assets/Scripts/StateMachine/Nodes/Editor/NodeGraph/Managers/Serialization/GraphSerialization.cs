using FrameLabs.AI.Nodes;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphSerialization
    {
        private static readonly GraphSerialization instance = new();
        public static GraphSerialization Instance => instance;

        private readonly NodeSerializer nodeSerializer;
        private readonly EdgeSerializer edgeSerializer;
        private readonly NodeFactory nodeFactory;

        private GraphSerialization()
        {
            nodeSerializer = new NodeSerializer();
            edgeSerializer = new EdgeSerializer();
            nodeFactory = new NodeFactory();
        }

        /// <summary>
        /// Serializes the current state of the graph into NodeGraphData.
        /// </summary>
        public NodeGraphData SerializeGraph(StateGraphView graphView)
        {
            NodeGraphData graphData = CreateGraphData();

            graphData.nodes.AddRange(nodeSerializer.SerializeNodes(graphView));
            graphData.edges.AddRange(edgeSerializer.SerializeEdges(graphView));
            graphData.visualElements = nodeSerializer.SerializeVisualElements(graphView);

            graphData.ZoomScale = graphView.transform.scale.x;
            graphData.ScrollOffset = graphView.transform.position;

            return graphData;
        }

        /// <summary>
        /// Loads a graph from a file and deserializes it.
        /// </summary>
        public void DeserializeGraph(string path, StateGraphView graphView, out NodeGraphData data)
        {
            if (!File.Exists(path))
            {
                Debug.LogError($"[GraphSerialization] File not found: {path}");
                data = null;
                return;
            }

            NodeGraphData graphData = AssetDatabase.LoadAssetAtPath<NodeGraphData>(path);
            data = graphData;

            DeserializeGraph(graphData, graphView);
        }

        public VisualElementData CreateVisualElementData(GraphWidget element)
        {
            var data = new VisualElementData
            {
                Id = element.ElementID,
                ElementType = element.ElementType,
                position = element.GetPosition().position
            };

            // Handle visual elements like ReroutePinElement
            if (element is ReroutePinElement reroute)
            {
                data.inputIsLeft = reroute.GetInputAnchor() == reroute.GetLeftAnchor();
            }
            // If necessary, add other visual elements or conditions here

            return data;
        }



        /// <summary>
        /// Deserializes NodeGraphData into the given graph view.
        /// </summary>
        public void DeserializeGraph(NodeGraphData graphData, StateGraphView graphView, bool clearGraph = true)
        {
            if (graphData == null)
            {
                Debug.LogError("[GraphSerialization] Invalid or null graph data.");
                return;
            }

            if (clearGraph)
            {
                graphView.ClearGraph();
            }

            Dictionary<string, NodeView> nodeViewsById = nodeSerializer.DeserializeNodes(graphData, graphView);
            nodeSerializer.DeserializeVisualElements(graphData, graphView);

            graphView.schedule.Execute(() =>
            {
                edgeSerializer.DeserializeEdges(graphData, nodeViewsById, graphView);
                graphView.transform.position = graphData.ScrollOffset;
                graphView.transform.scale = new Vector3(graphData.ZoomScale, graphData.ZoomScale, 1f);
            });
        }

        /// <summary>
        /// Creates an empty NodeGraphData instance.
        /// </summary>
        private NodeGraphData CreateGraphData()
        {
            return ScriptableObject.CreateInstance<NodeGraphData>();
        }

        /// <summary>
        /// Exposes EdgeSerializer's CreateEdgeData method.
        /// </summary>
        public EdgeData CreateEdgeData(EdgeElement edge)
        {
            return edgeSerializer.CreateEdgeData(edge);
        }

        /// <summary>
        /// Exposes NodeSerializer's SerializeNodes method.
        /// </summary>
        public List<NodeData> SerializeNodes(StateGraphView graphView)
        {
            return nodeSerializer.SerializeNodes(graphView);
        }

        /// <summary>
        /// Exposes NodeSerializer's DeserializeNodes method.
        /// </summary>
        public Dictionary<string, NodeView> DeserializeNodes(NodeGraphData graphData, StateGraphView graphView)
        {
            return nodeSerializer.DeserializeNodes(graphData, graphView);
        }

        /// <summary>
        /// Exposes NodeSerializer's DeserializeNodeFromData method.
        /// </summary>
        public NodeView DeserializeNodeFromData(NodeData nodeData, StateGraphView graphView, bool assignNewId = false)
        {
            return nodeSerializer.DeserializeNodeFromData(nodeData, graphView, assignNewId);
        }

        /// <summary>
        /// Creates a NodeView from NodeData using NodeFactory.
        /// </summary>
        public NodeView CreateNodeView(NodeData nodeData)
        {
            return nodeFactory.CreateNodeView(nodeData);
        }

        /// <summary>
        /// Creates NodeData from a NodeView using NodeFactory.
        /// </summary>
        public NodeData CreateNodeData(NodeView nodeView)
        {
            return nodeFactory.CreateNodeData(nodeView);
        }


        /// <summary>
        /// Creates a copy of the given NodeView's data.
        /// </summary>
        /// <param name="nodeView">The NodeView to copy data from.</param>
        /// <param name="copyId">
        /// </param>
        /// <returns>A new NodeData instance containing the copied node's properties.</returns>
        public NodeData CopyNodeData(NodeView nodeView, bool copyId = true)
        {
            return nodeFactory.CopyNodeData(nodeView, copyId);
        }


        public VisualElementData CopyPinData(ReroutePinElement pin, bool copyId = true)
        {
            var data = GraphElementFactory.CopyGraphWidgetData(pin, copyId);
            data.inputIsLeft = pin.IsInputLeft();

            return data;
        }


        /// <summary>
        /// Initializes a Node instance with properties from NodeData.
        /// </summary>
        public void InitializeNodeProperties(Node nodeInstance, NodeData nodeData)
        {
            nodeFactory.InitializeNodeProperties(nodeInstance, nodeData);
        }

        /// <summary>
        /// Finds a port by name using NodeFactory.
        /// </summary>
        public PortElement FindPortByName(NodeView nodeView, string portName, PortDirection portDirection)
        {
            return nodeFactory.FindPortByName(nodeView, portName, portDirection);
        }
    }
}


