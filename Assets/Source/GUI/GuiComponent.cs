//  Base GUI Component
//  By PenguinVEVO
//  Created November 2025

using UnityEngine;

namespace BZApp.GUI.Components
{
    [RequireComponent(typeof(GuiSystem))]
    public abstract class GuiComponent : MonoBehaviour
    {
        protected GuiSystem guiSystem;

        protected virtual void Awake()
        {
            guiSystem = GetComponent<GuiSystem>();
            guiSystem.Register(this); // Add this GUI Component to the GUI System's registry
        }
    }
}