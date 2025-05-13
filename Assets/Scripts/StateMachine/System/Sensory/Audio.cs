using FrameLabs.AI.Sensory;
using FrameLabs.AI.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.AI.Sensory
{
    [CreateAssetMenu(fileName = "Audio Sensor", menuName = "FrameLabs/AI/Sensors/Audio")]
    public class Audio : Sensory
    {
        private float DefaultIntensity(float distance)
        {
            float maxDetectRange = detectRange;  
            float normalizedDistance = Mathf.Clamp01(1 - (distance / maxDetectRange));

            return normalizedDistance;
        }

        protected override SensoryEvent CreateSensoryEvent(GameObject target, CalculateIntensity function = null)
        {
            float distance = Vector3.Distance(agent.position, target.transform.position);

            float intensity = function != null ? function(distance) : DefaultIntensity(distance);

            return new SensoryEvent
            {
                type = SensoryType.Audio,
                Source = target,
                Intensity = intensity
            };
        }
    }
}