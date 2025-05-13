using FrameLabs.AI.Nodes;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace FrameLabs.AI.System
{
    /// <summary>
    /// Manages the execution and evaluation of nodes in the state machine's behavior tree.
    /// Handles node initialization, starting, tracking, and state management during execution.
    /// Interrupt handling is integrated into the execution process.
    /// </summary>
    internal partial class NodeExecutionManager
    {
        private readonly StateMachine stateMachine;
        private readonly Dictionary<Node, int> nodeExecutionCounts = new Dictionary<Node, int>();

#if UNITY_EDITOR
        public event Action<Node> OnNodeExecution;
#endif

        public NodeExecutionManager(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// Initializes execution trackers for all nodes in the behavior tree.
        /// </summary>
        public void Initialize()
        {
            List<Node> nodes = stateMachine.NodeQuery.GetAllNodes(stateMachine.treeInstance);

            for (int i = 0;i < nodes.Count; i++)
            {
                EvaluateNode(nodes[i], initializeOnly: true);
            }
        }

        /// <summary>
        /// Evaluates whether a node should be initialized and executed.
        /// </summary>
        internal bool EvaluateNode(Node node, bool initializeOnly = false)
        {
            if (initializeOnly)
            {
                return true;
            }

            // Initialize the node if it hasn't been initialized yet
            if (node.State == NodeState.Uninitialized)
            {
                node.Initialize(stateMachine);
            }

            if (node.executeOnce)
            {
                if (nodeExecutionCounts.TryGetValue(node, out var count))
                {
                    if (count > 0)
                        return false;
                }
            }

            return true;
        }

        internal void ResetExecutionCounter()
        {
            nodeExecutionCounts.Clear();
        }

        /// <summary>
        /// Executes the current node and manages node transitions.
        /// Checks for interrupts before normal execution.
        /// </summary>
        public void ExecuteCurrentNode()
        {
            // Skip if paused
            if (stateMachine.StateMachineState == State.Pause)
                return;

            if (HandleInterrupts())
                return;

            HashSet<Node> visitedNodes = new HashSet<Node>();

            while (stateMachine.currentNode != null)
            {
                stateMachine.NodeQuery.TrackNodeInBreadcrumbs(stateMachine.currentNode);

                if (!visitedNodes.Add(stateMachine.currentNode))
                {
                    Debug.LogError($"[NodeExecutionManager] Infinite loop detected at node '{stateMachine.currentNode.name}'. Branch execution terminated.");
                    stateMachine.EndBranchExecution();
                    break;
                }

                if (EvaluateNode(stateMachine.currentNode))
                {
                    if (!stateMachine.currentNode.HasStarted)
                    {
                        stateMachine.currentNode.Start();
                    }

                    stateMachine.currentNode.Execute();

#if UNITY_EDITOR
                    OnNodeExecution?.Invoke(stateMachine.currentNode);
#endif

                    if (!nodeExecutionCounts.TryAdd(stateMachine.currentNode, 1))
                    {
                        nodeExecutionCounts[stateMachine.currentNode]++;
                    }

                    switch (stateMachine.currentNode.State)
                    {
                        case NodeState.Success:
                            {
                                Node nextNode = stateMachine.currentNode.GetNextNode();
                                if (nextNode != null)
                                {
                                    stateMachine.currentNode.Exit();
                                    stateMachine.currentNode = nextNode;
                                }
                                else
                                {
                                    stateMachine.RestartOrResetTree();
                                    return;
                                }
                            }
                            break;

                        case NodeState.Failure:
                            stateMachine.HandleFailure();
                            return;

                        case NodeState.EndBranch:
                            stateMachine.EndBranchExecution();
                            return;

                        case NodeState.Running:
                            return;
                    }
                }
                else
                {
                    stateMachine.NodeNavigator.MoveToNextNodeInBranch();
                }
            }
        }

        /// <summary>
        /// Handles interrupt nodes before normal node execution.
        /// </summary>
        private bool HandleInterrupts()
        {
            if (stateMachine.CheckForInterrupts())
            {
                return true; // Interrupt handled
            }
            return false;
        }
    }

}