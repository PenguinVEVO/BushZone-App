using FrameLabs.AI.System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.AI.Nodes
{
    [CreateAssetMenu(fileName = "StartNode", menuName = "FrameLabs/AI/Nodes/Start Node")]
    public class StartNode : Node
    {
        public int EntryPoint = 0;

        public override void Initialize( StateMachine StateMachine )
        {
            base.Initialize( StateMachine );
        }

        public override void Start()
        {
            exitCode = EntryPoint;

            Node nextNode = GetNextNode();

            if( nextNode != null )
            {
                Debug.Log( $"STARTNODE:: Starting Behavior Tree" );

                stateMachine.currentNode = nextNode;
                currentState = NodeState.Success;
                isStarted = true;
                return;
            }
            else
            {
                Debug.LogWarning( $"StartNode [{Id}]: No node to start." );
                currentState = NodeState.EndBranch;
                return;
            }
        }
      
        public override void Exit() {}
    }

}