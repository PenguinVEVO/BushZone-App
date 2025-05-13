using FrameLabs.AI.Nodes;
using System;
using UnityEngine;

namespace FrameLabs.AI.System
{
    [Serializable]
    public class NodeExecutionTracker : MonoBehaviour
    {
        public string NodeId;
        public Node Node;
        public int ExecutionCount;

        public bool CanExecute()
        {
            return ExecutionCount == 0;
        }

        public void IncrementExecutionCount()
        {
            ExecutionCount++;
        }
    }
}