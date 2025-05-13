using UnityEngine;
using System.Collections.Generic;
using FrameLabs.AI.Nodes;
using FrameLabs.AI.Runtime;

using System;

/*
 * File: StateMachine.cs
 * Author; Thanh Hon
 * Date: 29/04/2025
 */
namespace FrameLabs.AI.System
{
    public enum State
    {
        UnIntialized,
        Started,
        Running,
        Pause
    }

    /// <summary>
    /// Represents a behavior tree-based state machine system.
    /// Manages node execution, tree traversal, interrupt handling, and node queries.
    /// </summary>
    public partial class StateMachine : MonoBehaviour
    {
        internal static List<StateMachine> allStateMachines = new List<StateMachine>();

        public Action<bool> OnEndOfBranch;
        public Transform GetTransform => transform;

        public State StateMachineState = State.UnIntialized;


#if UNITY_EDITOR
        [SerializeField] private bool EnableDebugging = false;
        private TelemetryCollector telemetryCollector;
#endif

        [SerializeField] internal bool Pause;
        [SerializeField] internal StateMachineAsset behaviourTree;         // The compiled tree asset

        internal int InterruptCapacity = 3;                                // Maximum number of interrupt nodes that can be registered
        internal Node treeInstance;
        internal Node currentNode;

        internal Queue<Node> interruptNodes = new Queue<Node>();            // Queue of nodes waiting for execution after an interrupt
        internal Queue<Node> breadCrumbTrails = new Queue<Node>();          // Tracks previously visited nodes
        internal HashSet<string> visitedNodes = new HashSet<string>();      // Tracks nodes that have been added to the breachcrumbs

        internal NodeExecutionManager nodeExecutionManager;
        internal NodeNavigationManager nodeNavigationManager;
        internal NodeQueryManager nodeQueryManager;

        private InterruptManager interruptManager;
        private NodeTreeController treeManager;

        internal NodeQueryManager NodeQuery => nodeQueryManager;
        internal NodeNavigationManager NodeNavigator => nodeNavigationManager;

        public void Awake()
        {
            allStateMachines.Add( this );

            nodeExecutionManager = new NodeExecutionManager( this );
            nodeNavigationManager = new NodeNavigationManager( this );
            interruptManager = new InterruptManager( this );
            treeManager = new NodeTreeController( this );
            nodeQueryManager = new NodeQueryManager( this );

            nodeQueryManager.GenerateMachineLookUpTable();

#if UNITY_EDITOR

            if (EnableDebugging)
            {
                telemetryCollector = gameObject.AddComponent<TelemetryCollector>();
                telemetryCollector.Hook(this);
            }
#endif
        }

        public void UpdateState()
        {
            if( StateMachineState == State.UnIntialized )
            {
                Debug.LogError( "State Machine is UnInitialized, failed to load Behavior Tree." );
                return;
            }

            if (StateMachineState == State.Pause)
                return;
          
            interruptManager.CheckForInterrupts();

            if( currentNode != null )
            {
                nodeExecutionManager.ExecuteCurrentNode();
            }

            StateMachineState = State.Running;
        }

        public void ClearBreadCrumbs()
        {
            visitedNodes.Clear();
            breadCrumbTrails.Clear();
        }

        public void PauseStateMachine()
        {
            StateMachineState = State.Pause;

            currentNode?.Pause();
        }

        public void ResumeStateMachine()
        {
            StateMachineState = State.Running;

            currentNode?.Resume();
        }


        public bool RegisterInterruptNode( Node node )
        {
            return interruptManager.RegisterInterruptNode( node );
        }

        public bool CheckForInterrupts()
        {
            return interruptManager.CheckForInterrupts();
        }

        public void UnRegisterInterruptNode( Node node )
        {
            interruptManager.UnRegisterInterruptNode( node );
        }

        public Node GetPreviousNode()
        {
            return nodeNavigationManager.GetPreviousNode();
        }

        public StateMachine GetStateMachineByNode( Node node )
        {
            return nodeQueryManager.GetStateMachineByNode( node );
        }

        public Node GetNode<T>() where T : Node
        {
            return nodeQueryManager.FindNodeByType<T>();
        }

        public Node GetNode( Node search, Node parent = null )
        {
            return nodeQueryManager.FindNode( treeInstance, search );
        }

        public T FindNodeByType<T>() where T : Node
        {
            return nodeQueryManager.FindNodeByType<T>();
        }

        public bool EvaluateNode( Node node )
        {
            return nodeExecutionManager.EvaluateNode( node );
        }

        public void RestartNode( Node node )
        {
            treeManager.RestartNode( node );
        }

        public void ResetTree()
        {
            treeManager.ResetTree();
        }

        public void LoadTree()
        {
            treeManager.InitializeTree();
            treeManager.LoadTree();
            nodeExecutionManager.Initialize();

            StateMachineState = State.Started;
        }

        internal void HandleFailure()
        {
            // If the current node is an ExitNode, we need to handle it differently.
            if( currentNode is ExitNode exitNode )
            {
                Debug.Log( "StateMachine: Running ExitNode Logic" );
                exitNode.Execute();
                return;
            }

            // Regular node failure handling
            bool shuffled = nodeNavigationManager.ShuffleNodePriority( currentNode );

            if( shuffled )
            {
                nodeNavigationManager.MoveToNextNodeInBranch();
            }
            else
            {
                // If unable to shuffle, restart the current node or reset the entire tree.
                if( !treeManager.RestartNode( currentNode ) )
                {
                    treeManager.ResetTree();
                }
            }
        }

        internal void EndBranchExecution()
        {
            // Check if the current node's state is set to 'EndBranch'
            if( currentNode != null && currentNode.State == NodeState.EndBranch )
            {
                currentNode = null;
                ClearBreadCrumbs();

                // Signal the end of the branch
                OnEndOfBranch?.Invoke( true );
                return;
            }

            // Indicate we're restarting the tree
            OnEndOfBranch?.Invoke( false ); 
            ClearBreadCrumbs();
            RestartOrResetTree();
        }

        /// <summary>
        /// Utility method to either restart or reset the behavior tree when the current node is null.
        /// </summary>
        internal void RestartOrResetTree()
        {
            ClearBreadCrumbs();
            treeManager.ResetTree();            
        }
    }

}