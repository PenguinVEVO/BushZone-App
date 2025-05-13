using UnityEngine;

namespace FrameLabs.AI.Sensory
{
    public enum SensoryType
    {
        Audio,
        Visual
    }

    public class SensoryEvent
    {
        public SensoryType type;
        public GameObject Source;
        public float Intensity;
        public Vector3 postion;
        public float Timestamp;

        public SensoryEvent()
        {
            Timestamp = Time.time;
        }
    }
}