using FrameLabs.AI.Extension;
using FrameLabs.AI.System;
using UnityEngine;


namespace FrameLabs.AI.Nodes
{
    [CreateAssetMenu(fileName = "ActionNode", menuName = "FrameLabs/AI/Nodes/Action Node")]
    public class ActionNode : Node
    {
        public StateExtension stateExtension;
        private StateExtension instance;

        // Initializes the ActionNode and sets up the StateExtension
        public override void Initialize( StateMachine stateMachine )
        {
            base.Initialize( stateMachine );  // Call the base class Initialize

            if( stateExtension != null )
            {
                // Instantiate a new instance of the state extension for this node
                instance = Instantiate( stateExtension );
            }
            else
            {
                Debug.LogWarning( "ActionNode: StateExtension not set for " + GetType().Name );
                currentState = NodeState.Failure;
            }

            isStarted = false;  // Reset isStarted flag during initialization
        }

        // Start the node and begin the state extension's logic
        public override void Start()
        {
            if( currentState == NodeState.Failure || instance == null )
            {
                Debug.LogWarning( "ActionNode: Cannot start, either failed or missing instance." );
                return;
            }

            if( !isStarted )
            {
                instance.Begin( stateMachine );  // Begin the state extension's logic
                currentState = NodeState.Running;
                isStarted = true;  // Mark the node as started
            }
        }

        // Execute the node's logic in the behavior tree
        public override void Execute()
        {
            if( currentState == NodeState.Initialized || currentState == NodeState.Running )
            {
                if( instance == null )
                {
                    Debug.LogError( "ActionNode: Instance is null during Execute." );
                    currentState = NodeState.Failure;
                    return;
                }

                // Execute the logic in StateExtension and obtain the exit code
                exitCode = instance.Evaluate();

                if( exitCode != -1 )  // Exit code -1 means to keep running
                {
                    Node nextNode = GetNextNode();

                    if( nextNode != null )
                    {
                        // Set the state machine's current node to the next node without executing it directly
                        stateMachine.currentNode = nextNode;
                        currentState = NodeState.Success;
                    }
                    else
                    {
                        // No valid next node, mark as failure
                        currentState = NodeState.Failure;
                        Debug.LogError( "ActionNode: No valid next node found, marking as failure." );
                    }
                }
                else
                {
                    // Continue running the current node
                    currentState = NodeState.Running;
                }
            }
        }

        public override void Pause()
        {
            if (instance != null ) 
            {
                instance.Pause();
            }

            Debug.Log( $"ActionNode Paused" );
        }

        public override void Resume()
        {

            if( instance != null )
            {
                instance.Resume();
            }

            Debug.Log( "ActionNode resumed." );
        }

        // Exit the node, cleaning up any running state
        public override void Exit()
        {
            if( instance != null )
            {
                instance.Exit();  // Call the Exit method on the state extension
            }

            isStarted = false;  // Reset the started flag
            currentState = NodeState.Uninitialized;  // Mark this node as uninitialized
        }
    }
}