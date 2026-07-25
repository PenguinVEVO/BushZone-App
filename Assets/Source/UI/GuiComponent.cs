//  GUI Component
//  By Mitchel Smith
//  November 2025

using UnityEngine;

namespace BZApp.GUI.Systems.Components
{
    public class GuiComponent : MonoBehaviour
    {
        /* ======================[#]  DEPENDENCIES  [#]====================== */

        protected GuiSystem guiSystem;
        
        /* ======================[#]  LIFECYCLE FUNCTIONS  [#]====================== */

        protected virtual void Awake()
        {
            guiSystem = GetComponent<GuiSystem>();
            guiSystem.Register(this); // Add this GUI Component to the GUI System's registry
        }
    }
}