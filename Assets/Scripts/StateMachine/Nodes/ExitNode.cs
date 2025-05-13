using FrameLabs.AI.System;
using UnityEngine;

namespace FrameLabs.AI.Nodes
{
    [CreateAssetMenu( fileName = "ExitNode", menuName = "FrameLabs/AI/Nodes/Exit Node" )]
    public class ExitNode : Node
    {
        public bool RestartTree;

        public override void Initialize(StateMachine stateMachine)
        {
            base.Initialize(stateMachine);
        }

        /// <summary>
        /// Executes the ExitNode's logic to set the next node in the state machine.
        /// </summary>
        public override void Execute()
        {
            if (currentState == NodeState.Uninitialized)
            {
                Debug.LogError("ExitNode: Node is in an uninitialized state and cannot be executed.");
                currentState = NodeState.Failure;
                return;
            }

            // Determine the next step based on the RestartTree flag
            if (RestartTree)
            {
                Debug.Log("ExitNode: Restarting the tree.");
                stateMachine.ResetTree();
                stateMachine.ClearBreadCrumbs(); // Ensure breadcrumbs are cleared on restart
                currentState = NodeState.Success; // Mark node as successful
            }
            else
            {
                // Attempt to find the next node in the current branch/path
                Node nextNode = GetNextNode();
                if (nextNode != null)
                {
                    // Set the state machine's currentNode to the next node without executing it directly
                    Debug.Log($"ExitNode: Transitioning to next node: {nextNode.GetType().Name}");
                    stateMachine.currentNode = nextNode;
                    currentState = NodeState.Success; // Mark node as successful
                }
                else
                {
                    // No valid next node, so mark the branch as ended
                    Debug.LogWarning($"ExitNode: No valid next node found. Exit code: {exitCode}");
                    currentState = NodeState.EndBranch;
                }
            }
        }

        /// <summary>
        /// Resets the state of the ExitNode when exiting.
        /// </summary>
        public override void Exit()
        {
            currentState = NodeState.Uninitialized;
        }

        /// <summary>
        /// Sets whether the tree should restart when this node is executed.
        /// </summary>
        public void SetRestartTree(bool restart)
        {
            RestartTree = restart;
        }
    }
}