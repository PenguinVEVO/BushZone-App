using System;
using UnityEngine;


namespace FrameLabs.AI.Sensory
{

    public delegate float CalculateIntensity(float distance);

    public abstract class Sensory : ScriptableObject, ISensory
    {
        public float detectRange = 10f;
        public LayerMask layerMask;  

        public event Action<SensoryEvent> OnSensoryEvent;

        protected Transform agent; 

        public virtual void Initialize(Transform transform)
        {
            agent = transform; 
            Debug.Log($"{GetType().Name} initialized.");
        }
        private bool IsSelf( GameObject target )
        {
            // Check if the target is part of the same agent (the same root object)
            if( target == agent.gameObject ) return true;

            // Check if the target shares the same root transform as the agent
            if( target.transform.root == agent.root ) return true;

            // Check if the target is a child of the agent
            if( target.transform.IsChildOf( agent ) ) return true;

            return false;  // The target is not part of the same agent
        }

        public void Detect()
        {
            // Detect colliders within the detection range
            Collider[] colliders = Physics.OverlapSphere( agent.position, detectRange, layerMask );

            for( int i = 0; i < colliders.Length; i++ )
            {
                GameObject target = colliders[ i ].gameObject;

                // Ignore self-detection by checking if the target is a part of the same agent
                if( IsSelf( target ) ) continue;

                Debug.Log( $"[{agent.root.name}] Detected: <{target.name}>" );

                // Create a sensory event and invoke the event handler
                SensoryEvent sensoryEvent = CreateSensoryEvent( target );
                OnSensoryEvent?.Invoke( sensoryEvent );
            }
        }

        protected abstract SensoryEvent CreateSensoryEvent(GameObject target, CalculateIntensity function = null);
    }

}