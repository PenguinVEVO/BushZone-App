using FrameLabs.AI.Nodes;

namespace FrameLabs.AI.System
{
    public class NodeTelemetry
    {
        public Node node;
        public NodeState currentState;
        public int executionCount;
        public bool hasExecuted;
        public float runningStartTime;
        public bool isRunning;
        public float lastExecutionTimestamp;
        public float totalExecutionTime;

        public NodeTelemetry(Node targetNode)
        {
            node = targetNode;
            currentState = NodeState.Uninitialized;
            executionCount = 0;
            hasExecuted = false;
            lastExecutionTimestamp = 0f;
            totalExecutionTime = 0f;
            runningStartTime = 0f;
            isRunning = false;
        }
    }
}