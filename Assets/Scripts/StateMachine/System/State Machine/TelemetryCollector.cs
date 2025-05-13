#if UNITY_EDITOR

using FrameLabs.AI.Nodes;
using FrameLabs.AI.System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Collects telemetry data by subscribing to NodeExecutionManager events.
/// Only active in Editor builds.
/// </summary>
public class TelemetryCollector : MonoBehaviour
{
    private NodeExecutionManager executionManager;

    /// <summary>
    /// Hooks into a StateMachine's NodeExecutionManager to listen for execution events.
    /// </summary>
    public void Hook(StateMachine stateMachine)
    {
        if (stateMachine == null)
        {
            Debug.LogWarning("[TelemetryCollector] StateMachine is null. Telemetry disabled.");
            Destroy(this);
            return;
        }

        executionManager = stateMachine.nodeExecutionManager;

        if (executionManager != null)
        {
            executionManager.OnNodeExecution += OnNodeExecuted;
        }
        else
        {
            Debug.LogWarning("[TelemetryCollector] NodeExecutionManager is null. Telemetry disabled.");
            Destroy(this);
        }
    }

    private void OnDestroy()
    {
        if (executionManager != null)
        {
            executionManager.OnNodeExecution -= OnNodeExecuted;
        }
    }

    private void OnNodeExecuted(Node node)
    {
        if (node != null)
        {
            TelemetryManager.Instance.RecordExecution(node);
        }
    }

    /// <summary>
    /// Retrieves telemetry data for a specific node.
    /// </summary>
    public NodeTelemetry GetTelemetry(Node node)
    {
        return TelemetryManager.Instance.GetTelemetry(node);
    }

    /// <summary>
    /// Retrieves telemetry data for all nodes.
    /// </summary>
    public IEnumerable<NodeTelemetry> GetAllTelemetry()
    {
        return TelemetryManager.Instance.GetAllTelemetry();
    }

    /// <summary>
    /// Resets all collected telemetry data.
    /// </summary>
    public void ResetTelemetry()
    {
        TelemetryManager.Instance.ResetTelemetry();
    }
}

#endif
