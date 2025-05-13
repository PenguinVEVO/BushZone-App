using FrameLabs.AI.Nodes;
using System.Collections.Generic;
using System.Linq;


namespace FrameLabs.AI.System
{
    internal class InterruptManager
    {
        private readonly StateMachine stateMachine;
        private Queue<Node> interruptNodes = new Queue<Node>();

        public InterruptManager(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public bool RegisterInterruptNode(Node node)
        {
            if (interruptNodes.Count < stateMachine.InterruptCapacity && !interruptNodes.Contains(node))
            {
                interruptNodes.Enqueue(node);
                return true;
            }
            return false;
        }

        public void UnRegisterInterruptNode(Node node)
        {
            interruptNodes = new Queue<Node>(interruptNodes.Where(n => n != node));
        }

        /// <summary>
        /// Checks if there are any interrupt nodes to execute.
        /// Returns true if an interrupt is being processed.
        /// </summary>
        public bool CheckForInterrupts()
        {
            if (interruptNodes.Count > 0)
            {
                ExecuteInterruptNode();
                return true;
            }
            return false;
        }

        private void ExecuteInterruptNode()
        {
            Node interruptNode = interruptNodes.Dequeue();

            if (interruptNode != null && interruptNode.State != NodeState.Uninitialized)
            {
                stateMachine.currentNode = interruptNode;
                interruptNode.Execute();
            }
            else
            {
                interruptNode?.Initialize(stateMachine);
                stateMachine.currentNode = interruptNode;
            }
        }
    }


}