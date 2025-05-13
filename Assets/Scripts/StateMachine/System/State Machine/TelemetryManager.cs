using FrameLabs.AI.Nodes;
using FrameLabs.AI.System;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Manages telemetry data collection for all nodes during execution.
/// </summary>
public sealed class TelemetryManager
{
    private static readonly TelemetryManager instance = new();
    public static TelemetryManager Instance => instance;
    private TelemetryManager() { } 

    private readonly Dictionary<Node, NodeTelemetry> nodeTelemetryMap = new();

    /// <summary>
    /// Updates or creates telemetry data for a node after execution.
    /// </summary>
    public void RecordExecution(Node node)
    {
        if (!nodeTelemetryMap.TryGetValue(node, out NodeTelemetry telemetry))
        {
            telemetry = new NodeTelemetry(node);
            nodeTelemetryMap[node] = telemetry;
        }

        telemetry.executionCount++;
        telemetry.hasExecuted = true;
        telemetry.currentState = node.State;
        telemetry.lastExecutionTimestamp = Time.time;

        switch (node.State)
        {
            case NodeState.Running:
                if (!telemetry.isRunning)
                {
                    telemetry.runningStartTime = Time.time;
                    telemetry.isRunning = true;
                }
                break;

            case NodeState.Success:
            case NodeState.Failure:
            case NodeState.EndBranch:
                if (telemetry.isRunning)
                {
                    telemetry.totalExecutionTime += (Time.time - telemetry.runningStartTime);
                    telemetry.isRunning = false;
                }
                break;
        }
    }

    /// <summary>
    /// Retrieves telemetry data for a specific node.
    /// Returns null if not found.
    /// </summary>
    public NodeTelemetry GetTelemetry(Node node)
    {
        nodeTelemetryMap.TryGetValue(node, out NodeTelemetry telemetry);
        return telemetry;
    }

    /// <summary>
    /// Retrieves all telemetry data for all nodes.
    /// </summary>
    public IEnumerable<NodeTelemetry> GetAllTelemetry()
    {
        return nodeTelemetryMap.Values;
    }

    /// <summary>
    /// Clears all collected telemetry data.
    /// </summary>
    public void ResetTelemetry()
    {
        nodeTelemetryMap.Clear();
    }
}
