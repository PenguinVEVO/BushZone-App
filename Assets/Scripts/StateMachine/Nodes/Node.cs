using UnityEngine;
using System.Collections.Generic;
using FrameLabs.AI.System;

namespace FrameLabs.AI.Nodes
{
    public enum NodeState
    {
        EndBranch,
        Uninitialized,
        Initialized,
        Running,
        Success,
        Failure
    };

    public abstract class Node : ScriptableObject
    {

        [SerializeField] protected Node parent = null;
        [SerializeField] protected NodeState currentState = NodeState.Uninitialized;
        [SerializeField] protected int priority;
        [SerializeField] public List<Node> childNodes = new List<Node>();

        protected Dictionary<int, Node> exitCodeToChildNode = new Dictionary<int, Node>();

        protected StateMachine stateMachine;
        protected int exitCode;
        protected bool isStarted = false;

        public string Id { get; set; }
        public bool executeOnce = false;
        public Node Parent => parent;
        public int ExitCode => exitCode;

        public NodeState State
        {
            get { return currentState; }
            protected set { currentState = value; }
        }

        public bool HasStarted
        {
            get { return isStarted; }
        }

        // New method to map exit codes to child nodes
        public void MapExitCodeToChild( int code, Node child )
        {
            if( child != null && !exitCodeToChildNode.ContainsKey( code ) )
            {
                exitCodeToChildNode[ code ] = child;
                if( !childNodes.Contains( child ) )
                {
                    childNodes.Add( child );
                }
            }
        }

        public Node GetChildAtBranch(int branchIndex)
        {
            exitCodeToChildNode.TryGetValue(branchIndex, out Node childNode);
            return childNode;
        }

        public IEnumerable<int> GetAllBranchIndices()
        {
            return exitCodeToChildNode.Keys;
        }

        // Method to remove exit code entries (for cleanup)
        public void RemoveExitCodeEntry( int code )
        {
            if( exitCodeToChildNode.ContainsKey( code ) )
            {
                Node child = exitCodeToChildNode[ code ];
                if( childNodes.Contains( child ) )
                {
                    childNodes.Remove( child );
                }

                exitCodeToChildNode.Remove( code );
            }
        }

        public int Priority
        {
            get { return priority; }
            private set
            {
                if (value >= 0)
                {
                    priority = value;
                }
                else
                {
                    Debug.LogWarning("Priority cannot be negative. Setting priority to 0.");
                    priority = 0;
                }
            }
        }

        /// <summary>
        /// Adjusts the priority of this node by a specified amount.
        /// </summary>
        public void AdjustPriority(int amount)
        {
            Priority += amount;
            Debug.Log($"Node {Id}: Adjusted priority by {amount}. New priority: {Priority}");
        }

        /// <summary>
        /// Sets the priority of this node to a specific value.
        /// </summary>
        public void SetPriority(int newPriority)
        {
            Priority = newPriority;
            // Debug.Log($"Node {Id}: Priority set to {Priority}");
        }

        /// <summary>
        /// Compares the priority of this node with another node.
        /// </summary>
        public bool HasHigherPriority(Node other)
        {
            return Priority > other.Priority;
        }

        public virtual void Initialize(StateMachine StateMachine) 
        {
            if (StateMachine != null)
                stateMachine = StateMachine;
            else if (StateMachine == null)
            {
                Debug.LogError($"Node of type {GetType()}::[{Id}]. Failed Initialization, State Machine is null");
            }

            if ( currentState == NodeState.Uninitialized )
            {
                stateMachine = StateMachine;

                currentState = NodeState.Initialized;

                InitSubNodes();
            }
        }

        public virtual void InitSubNodes()
        {
            for (int i = 0; i < childNodes.Count; i++)
            {
                if (childNodes[i] != null)
                {
                    childNodes[i].parent = this;
                    childNodes[i].Initialize(stateMachine);
                    childNodes[i].SetPriority(i);
                }
                else
                {
                    Debug.LogWarning($"Child node is null during initialization in {GetType().Name}");
                }
            }
        }

        public virtual Node GetNextNode()
        {
            // Handle case where there is only one child node
            if (childNodes.Count == 1)
            {
                if (childNodes[0] != null)
                {
                    stateMachine.NodeQuery.TrackNodeInBreadcrumbs(childNodes[0]);
                    return childNodes[0];  // Return the only child if it's valid
                }
                else
                {
                    Debug.LogWarning($"Single child node is null for node {GetType().Name}.");
                    return null;  // Handle case where the single child node is null
                }
            }

            // Handle case with multiple child nodes
            if (exitCode >= 0 && exitCode < childNodes.Count)
            {
                Node nextNode = childNodes[exitCode];

                if (nextNode != null)                
                {
                    stateMachine.NodeQuery.TrackNodeInBreadcrumbs(nextNode);
                    return nextNode;  // Return the valid node
                }
                else
                {
                    return null;  // Handle case where the node is null
                }
            }
            else
            {
                // Handle case where exitCode is out of bounds
                return null;
            }
        }

        public virtual void Start() { }
        public virtual void Execute() { }

        public virtual void Pause() { }
        public virtual void Resume() { }

        public abstract void Exit();

    }

}