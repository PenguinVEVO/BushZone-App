using FrameLabs.AI.Extension;
using FrameLabs.AI.Nodes;
using System;
using System.Linq;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class NodeFactory
    {
        public NodeFactory() { }

        /// <summary>
        /// Serializes a node into NodeData format.
        /// </summary>
        public NodeData CreateNodeData(NodeView nodeView)
        {
            NodeData nodeData = new NodeData
            {
                id = nodeView.NodeID,
                name = nodeView.Title,
                nodeType = nodeView.NodeData.GetType().AssemblyQualifiedName,
                position = nodeView.GetPosition().position,
                priority = nodeView.NodeData.Priority,
                executeOnce = nodeView.NodeData.executeOnce
            };

            AddCustomNodeProperties(nodeData, nodeView);

            return nodeData;
        }

        /// <summary>
        /// Copies NodeData from an existing NodeView.
        /// </summary>
        public NodeData CopyNodeData(NodeView nodeView, bool copyId = true)
        {
            if (nodeView == null) return null;

            NodeData copiedNodeData = CreateNodeData(nodeView);

            // Optionally generate a new unique ID for the copied node
            if (!copyId)
            {
                copiedNodeData.id = Guid.NewGuid().ToString();
            }

            return copiedNodeData;
        }

        /// <summary>
        /// Adds custom properties to NodeData based on node type.
        /// </summary>
        private void AddCustomNodeProperties(NodeData nodeData, NodeView nodeView)
        {
            switch (nodeView.NodeData)
            {
                case ActionNode actionNode:
                    nodeData.AddProperty(new ScriptableObjectProperty("StateExtension", actionNode.stateExtension));
                    nodeData.AddProperty(new IntProperty("DropDownOption", ((ActionNodeView)nodeView).CurrentSelectedIndex));
                    break;
                case ExitNode exitNode:
                    nodeData.AddProperty(new BoolProperty("Restart", exitNode.RestartTree));
                    break;
            }
        }

        /// <summary>
        /// Creates a NodeView based on NodeData.
        /// </summary>
        public NodeView CreateNodeView(NodeData nodeData)
        {
            Type nodeType = Type.GetType(nodeData.nodeType);
            if (nodeType == null)
            {
                Debug.LogError($"[NodeFactory] Failed to find node type: {nodeData.nodeType}");
                return null;
            }

            Node nodeInstance = (Node)ScriptableObject.CreateInstance(nodeType);
            if (nodeInstance == null)
            {
                Debug.LogError($"[NodeFactory] Failed to instantiate node of type: {nodeData.nodeType}");
                return null;
            }

            InitializeNodeProperties(nodeInstance, nodeData);

            NodeView view = CreateSpecificNodeView(nodeInstance, nodeData);
            view.SetPosition(new Rect(nodeData.position, new Vector2(view.Width, view.Height)));

            return view;
        }

        /// <summary>
        /// Initializes node instance properties from serialized data.
        /// </summary>
        public void InitializeNodeProperties(Node nodeInstance, NodeData nodeData)
        {
            nodeInstance.Id = nodeData.id;
            nodeInstance.SetPriority(nodeData.priority);
            nodeInstance.executeOnce = nodeData.executeOnce;

            switch (nodeInstance)
            {
                case ActionNode actionNode:
                    InitializeActionNode(actionNode, nodeData);
                    break;
                case ExitNode exitNode:
                    InitializeExitNode(exitNode, nodeData);
                    break;
            }
        }

        /// <summary>
        /// Returns the correct NodeView type based on node instance.
        /// </summary>
        public NodeView CreateSpecificNodeView(Node nodeInstance, NodeData nodeData)
        {
            switch (nodeInstance)
            {
                case ActionNode actionNode:
                    int selectedIndex = nodeData.GetProperty<int>("DropDownOption");
                    return new ActionNodeView(actionNode, null, selectedIndex);

                case ExitNode exitNode:
                    return new ExitNodeView(exitNode);

                case StartNode startNode:
                    return new StartNodeView(startNode);

                default:
                    return new NodeView(nodeInstance, nodeData.id);
            }
        }

        /// <summary>
        /// Initializes ActionNode-specific properties.
        /// </summary>
        private void InitializeActionNode(ActionNode actionNode, NodeData nodeData)
        {
            StateExtension stateExtension = nodeData.GetProperty<ScriptableObject>("StateExtension") as StateExtension;

            if (stateExtension != null)
            {
                actionNode.stateExtension = stateExtension;
            }
            else
            {
                Debug.LogWarning($"[NodeFactory] StateExtension for ActionNode '{nodeData.name}' is missing.");
            }
        }

        /// <summary>
        /// Initializes ExitNode-specific properties.
        /// </summary>
        private void InitializeExitNode(ExitNode exitNode, NodeData nodeData)
        {
            exitNode.RestartTree = nodeData.GetProperty<bool>("Restart");
        }

        /// <summary>
        /// Finds a port by name in a node.
        /// </summary>
        public PortElement FindPortByName(NodeView nodeView, string portName, PortDirection portDirection)
        {
            var ports = portDirection == PortDirection.Input ? nodeView.GetInputPorts() : nodeView.GetOutputPorts();
            var port = ports.FirstOrDefault(p => p.PortName == portName);

            if (port == null)
            {
                Debug.LogWarning($"[NodeFactory] Port '{portName}' not found on node '{nodeView.Title}'.");
            }

            return port;
        }
    }

}