using FrameLabs.AI.Sensory;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SensoryManager : MonoBehaviour
{
    public Transform AgentHead;

    // List of sensor templates (ScriptableObjects) to instantiate for each agent
    public List<Sensory> sensorTemplates = new List<Sensory>();

    // List to keep track of instantiated sensors for this agent
    private List<Sensory> sensors = new List<Sensory>();

    // Event that gets invoked when a sensory event is detected by any sensor
    public event Action<SensoryEvent> OnSensoryEventDetected;

    private void Start()
    {
        for (int i=0; i <sensorTemplates.Count; i++) 
        {
            Sensory sensorInstance = Instantiate(sensorTemplates[i]);
            sensorInstance.Initialize(AgentHead);
            RegisterSensor(sensorInstance);
        }
    }

    private void OnDestroy()
    {
        for (int i=0;i < sensorTemplates.Count;i++)
        {
            UnregisterSensor(sensorTemplates[i]);
        }
    }

    /// <summary>
    /// Registers a new sensor and subscribes to its sensory event.
    /// </summary>
    /// <param name="sensor">The sensor to register.</param>
    private void RegisterSensor(Sensory sensor)
    {
        if (sensor == null) return;

        sensors.Add(sensor); // Add the instantiated sensor to the list
        sensor.OnSensoryEvent += HandleSensoryEvent; // Subscribe to the sensor's event
    }

    /// <summary>
    /// Unregisters a sensor and unsubscribes from its sensory event.
    /// </summary>
    /// <param name="sensor">The sensor to unregister.</param>
    private void UnregisterSensor(Sensory sensor)
    {
        if (sensor == null || !sensors.Contains(sensor)) return;

        sensor.OnSensoryEvent -= HandleSensoryEvent; // Unsubscribe from the sensor's event
        sensors.Remove(sensor); // Remove the sensor from the list
    }

    private void HandleSensoryEvent(SensoryEvent sensoryEvent)
    {
        OnSensoryEventDetected?.Invoke(sensoryEvent); // Propagate the event
    }

    private void Update()
    {
        for (int i = 0; i < sensors.Count; i++)
        {
            sensors[i].Detect();
        }
    }
}

