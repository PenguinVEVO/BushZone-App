using FrameLabs.AI.Nodes;
using System.Linq;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public static class NodeViewExtension
    {
        /// <summary>
        /// Updates the visual background color of the node based on its execution state.
        /// </summary>
        public static void UpdateState(this NodeView nodeView)
        {
            if (nodeView?.NodeData == null)
                return;

            nodeView.style.backgroundColor = nodeView.NodeData.State switch
            {
                NodeState.Initialized => Color.green,
                NodeState.Running => Color.yellow,
                NodeState.Success => Color.blue,
                NodeState.Failure => Color.red,
                _ => Color.gray
            };
        }

        /// <summary>
        /// Handles logic when an edge is connected to this node.
        /// </summary>
        public static void OnEdgeCreated(this NodeView nodeView, EdgeElement edge)
        {
            if (edge.OutputPort == null || edge.InputPort == null)
                return;

            var parentPort = edge.OutputPort;
            var childPort = edge.InputPort;

            var parentNode = parentPort.ParentView;
            var childNode = childPort.ParentView;

            if (parentNode == null || childNode == null)
                return;

            childNode.ParentView = parentNode;

            int branchIndex = parentPort.BranchIndex;

            if (branchIndex < 0)
            {
                Debug.LogError($"[NodeViewExtension] Invalid branch index {branchIndex} for {parentNode.Title}");
                return;
            }

            parentNode.EnsureChildNodeListSize(branchIndex);
            parentNode.NodeData.childNodes[branchIndex] = childNode.NodeData;

            edge.SendToBack();
        }

        /// <summary>
        /// Handles logic when an edge is removed from this node.
        /// </summary>
        public static void OnEdgeRemoved(this NodeView nodeView, EdgeElement edge)
        {
            if (edge.OutputPort == null || edge.InputPort == null)
                return;

            var parentPort = edge.OutputPort;
            var childPort = edge.InputPort;

            var parentNode = parentPort.ParentView;
            var childNode = childPort.ParentView;

            if (parentNode == null || childNode == null)
                return;

            int branchIndex = parentPort.BranchIndex;

            parentNode.EnsureChildNodeListSize(branchIndex);

            parentNode.NodeData.childNodes[branchIndex] = null;
            childNode.ParentView = null;

            parentPort.RefreshElement();
            childPort.RefreshElement();
        }

        /// <summary>
        /// Ensures the childNodes list has room for the given branch index.
        /// </summary>
        public static void EnsureChildNodeListSize(this NodeView nodeView, int branchIndex)
        {
            while (nodeView.NodeData.childNodes.Count <= branchIndex)
                nodeView.NodeData.childNodes.Add(null);
        }

        /// <summary>
        /// Finds a port by name and direction.
        /// </summary>
        public static PortElement GetPort(this NodeView nodeView, string portName, PortDirection direction)
        {
            return direction == PortDirection.Input
                ? nodeView.GetInputPorts().FirstOrDefault(p => p.PortName == portName)
                : nodeView.GetOutputPorts().FirstOrDefault(p => p.PortName == portName);
        }
    }
}