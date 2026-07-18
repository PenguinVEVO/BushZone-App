using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Mitchel.Utilities
{
    public class TestingEventInvoker : MonoBehaviour
    {
        [SerializeField] private TestingEvent[] testingEvents;

        // Developer's note: must remember to enable and disable Unity's stupid InputActions
        // as it doesn't happen automatically.
        private void OnEnable()
        {
            for (int i = 0; i < testingEvents.Length; i++)
            {
                if (testingEvents[i].inputToActivate != null)
                    testingEvents[i].inputToActivate.Enable();
            }
        }
        
        private void OnDisable()
        {
            for (int i = 0; i < testingEvents.Length; i++)
            {
                if (testingEvents[i].inputToActivate != null)
                    testingEvents[i].inputToActivate.Disable();
            }
        }

        private void Update()
        {
            for (int i = 0; i < testingEvents.Length; i++)
            {
                if (testingEvents[i].inputToActivate == null)
                {
                    Debug.LogError($"[ERROR] {GetType()}: Testing Event at index {i} contains an empty InputAction and cannot be invoked! \n" +
                                   $"Ensure all Testing Events are correctly set up before hitting Play!");
                    return;
                }

                if (testingEvents[i].inputToActivate.WasPressedThisFrame())
                    testingEvents[i].invocationList.Invoke();
            }
        }
    }

    [System.Serializable]
    public struct TestingEvent
    {
        public InputAction inputToActivate;
        public UnityEvent invocationList;
    }
}