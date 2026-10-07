//  Cross-device Glyphs Manager
//  By PenguinVEVO
//  Created October 2026

using BZApp.GUI.Components.Configs;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace BZApp.GUI.Components
{
    [AddComponentMenu("UI/GUI Components/GlyphsManager")]
	public class GlyphsManager : GuiComponent
	{
        // ======================[#]  CONFIGURATION  [#]======================
		
		[SerializeField] private GlyphsManagerConfig config;
		
        // ======================[#]  LIFECYCLE FUNCTIONS  [#]======================
		
		protected override void Awake() 
		{
			base.Awake();
			
		}
		
		private void Start()
		{
			ReadOnlyArray<InputBinding> bindings = GetBindingsForAction("Menus", "Confirm");
			ReadOnlyArray<InputControl> controls = GetControlsForAction("Menus", "Confirm");
			string bindingsString = "Bindings for confirm:";
			foreach (var b in bindings)
				bindingsString += $"\n- {b.name} | {b.path}";
			Debug.Log(bindingsString);
			string controlsString = "Controls for confirm:";
			foreach (var c in controls)
				controlsString += $"\n- {c.name} | {c.path}";
			Debug.Log(controlsString);
		}
		
        // ======================[#]  GLYPHS MANAGER API  [#]======================

        public Sprite GetGlyph(string actionMapName, string actionName)
        {
	        InputActionMap targetActionMap = InputSystem.actions.FindActionMap(actionMapName);
	        if (targetActionMap == null)
	        {
		        Debug.LogError($"[ERROR] Could not find action map {actionMapName}!"); 
		        return config.ErrorGlyphSprite;
	        }
	        InputAction targetAction = targetActionMap.FindAction(actionName);
	        if (targetAction == null)
	        {
		        Debug.LogError($"[ERROR] Could not find action {actionName} in action map {actionMapName}!");
		        return config.ErrorGlyphSprite;
	        }
	        return null;
        }

        private ReadOnlyArray<InputBinding> GetBindingsForAction(string actionMapName, string actionName)
        {
	        InputActionMap targetActionMap = InputSystem.actions.FindActionMap(actionMapName);
	        InputAction targetAction = targetActionMap.FindAction(actionName);
	        return targetAction.bindings;
        }
        
        private ReadOnlyArray<InputControl> GetControlsForAction(string actionMapName, string actionName)
        {
	        InputActionMap targetActionMap = InputSystem.actions.FindActionMap(actionMapName);
	        InputAction targetAction = targetActionMap.FindAction(actionName);
	        return targetAction.controls;
        }
	}
}