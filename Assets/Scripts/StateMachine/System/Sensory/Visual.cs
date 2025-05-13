using FrameLabs.Utilities;
using System;
using UnityEngine;

namespace FrameLabs.AI.Sensory
{
    [CreateAssetMenu(fileName = "Visual Sensor", menuName = "FrameLabs/AI/Sensors/Visual")]
    public class Visual : Sensory
    {
        public float horizontalConeAngle = 45f;  // Horizontal FOV angle
        public float verticalConeAngle = 90f;  // Vertical FOV angle
        public float sphereCastRadius = 1.0f;  // Sphere cast radius for line of sight checks

        private void Awake()
        {
            // Initialize any necessary settings or default values
            Debug.Log("Visual Sensory initialized.");
        }

        private float DefaultIntensity(float distance)
        {
            float maxDetectRange = detectRange;

            float normalizedDistance = Mathf.Clamp01(1 - (distance / maxDetectRange));

            return normalizedDistance;
        }

        private bool IsTargetVisible(GameObject target, float distance)
        {
            // Check if the target is within the horizontal and vertical field of view (FOV) cone
            if (!PhysicsUtilities.Instance.IsInCone(agent.gameObject, target, horizontalConeAngle, verticalConeAngle, distance, layerMask))
            {
                Debug.Log($"[{agent.root.name}] Target {target.name} is outside the vision cone.");
                return false;
            }

            Debug.Log($"[{agent.root.name}] Target {target.name} is visible.");
            return true;
        }

        protected override SensoryEvent CreateSensoryEvent(GameObject target, CalculateIntensity function = null)
        {
            float distance = Vector3.Distance(agent.position, target.transform.position);

            if (IsTargetVisible(target, distance))
            {
                float intensity = function != null ? function(distance) : DefaultIntensity(distance);

                return new SensoryEvent
                {
                    type = SensoryType.Visual,
                    Source = target,
                    Intensity = intensity,
                    postion = target.transform.position,                  
                };
            }

            return null;
        }

    }
}