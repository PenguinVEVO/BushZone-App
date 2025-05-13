using FrameLabs.AI.System;
using System.Collections.Generic;
using UnityEngine;
namespace FrameLabs.AI.Extension
{
    /// <summary>
    /// Base class for defining state logic that can be evaluated within a behavior tree.
    /// This class is designed to be extended to create custom states.
    /// </summary>
    public abstract class StateExtension : ScriptableObject
    {
        protected StateMachine stateMachine;
        private readonly Dictionary<string, int> exitCodes = new Dictionary<string, int>();

        /// <summary>
        /// Add an Exit Code to the code dictionary
        /// </summary>
        /// <param name="name"> the name of the code</param>
        /// <param name="value">the code value</param>
        protected void AddExitCode(string name, int value)
        {
            if (exitCodes.ContainsKey(name)) 
            {
                exitCodes[name] = value;
            }
            else
            {
                exitCodes.Add(name, value);
            }
        }

        /// <summary>
        /// Read only Exit Codes, for this state
        /// </summary>
        public IReadOnlyDictionary<string ,int> ExitCodes
        { 
            get { return exitCodes; } 
        }

        /// <summary>
        /// Start the state logic with a reference to the state machine.
        /// This method is called when the state is instantiated by a Decision Node.
        /// </summary>
        public virtual void Begin(StateMachine StateMachine)
        {
            stateMachine = StateMachine;
        }

        public virtual void OnEnable() { }

        /// <summary>
        /// Evaluates the state logic.
        /// This method should be overridden in derived classes to implement specific state logic.
        /// </summary>
        public abstract int Evaluate();

        /// <summary>
        /// Called when the state is exited or the node that owns it is exited.
        /// </summary>
        public virtual void Exit() { }

        /// <summary>
        /// Pauses the state logic. Override in derived classes for custom behavior.
        /// </summary>
        public virtual void Pause()
        {
            Debug.Log( $"{GetType().Name} paused." );
        }

        /// <summary>
        /// Resumes the state logic. Override in derived classes for custom behavior.
        /// </summary>
        public virtual void Resume()
        {
            Debug.Log( $"{GetType().Name} resumed." );
        }
    }
}