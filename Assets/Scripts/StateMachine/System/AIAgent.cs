using FrameLabs.AI.Blackboard;
using FrameLabs.AI.System;
using UnityEngine;
using UnityEngine.AI;

namespace FrameLabs.AI.System
{
    [RequireComponent( typeof( StateMachine ) )]
    public class AIAgent : MonoBehaviour
    {
        private StateMachine stateMachine;
        private NavMeshAgent agent;


        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();

            stateMachine = GetComponent<StateMachine>();
            stateMachine.LoadTree();
        }

        /// <summary>
        /// Temp Method for testing, remove later
        /// </summary>
        public void StopAgent()
        {
            agent.isStopped = true;
        }

        /// <summary>
        /// Temp Method for testing, remove later
        /// </summary>
        public void ResumeAgent()
        {
            if (agent.isStopped)
                agent.isStopped = false;
        }

        public StateMachine GetStateMachine
        {
            get { return stateMachine; }
        }

        private void Update()
        {
            stateMachine.UpdateState();
        }
    }
}