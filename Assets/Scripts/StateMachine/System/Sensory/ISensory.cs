using System;
using UnityEngine;

namespace FrameLabs.AI.Sensory
{
    public interface ISensory
    {
        void Initialize(Transform transform);
        void Detect();

        event Action<SensoryEvent> OnSensoryEvent; 
    }
}