using BZApp.GUI.Modules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BZApp.GUI.Testing
{
    public class SelectableCursorTester : MonoBehaviour
    {
        [SerializeField] private Selectable firstSelectable;

        private SelectableCursorManager selectableCursorManager;

        private void Start() => selectableCursorManager = GuiSystem.Instance.Get<SelectableCursorManager>();

        private void Update()
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame) 
                selectableCursorManager.SetFirstSelected(firstSelectable);
        }
    }
}
