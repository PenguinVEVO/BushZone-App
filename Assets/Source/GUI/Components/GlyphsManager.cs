//  Cross-Device Glyphs Manager
//  By PenguinVEVO
//  Created October 2026
//
//  =============================[!]  AI ASSISTANCE DISCLAIMER  [!]=============================
//  This script was created with the help of ChatGPT. The areas assisted with AI are as follows:
//  • How to detect for input device changes with project-wide input actions:
//    https://share.google/aimode/kbDbzVPeAgnmNYEFK
//  ============================================================================================

using System.Collections.Generic;
using BZApp.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BZApp.GUI.Components
{
    [AddComponentMenu("UI/GUI Components/GlyphsManager")]
	public sealed class GlyphsManager : GuiComponent
	{
        // ======================[#]  CONFIGURATION  [#]======================
		
		[SerializeField] private GlyphsManagerConfig config;
		[SerializeField] private Image debugImageToTrack;
		[SerializeField] private LogOptions logOptions;
		
		// ======================[#]  INTERNAL VARIABLES/REFERENCES  [#]======================

		private List<TrackableGlyph> trackedGlyphs = new();

		private string lastKnownDevice;

		// ======================[#]  LIFECYCLE FUNCTIONS  [#]======================
		
		protected override void Awake() 
		{
			base.Awake();
			debugImageToTrack.sprite = GetGlyph("Menus", "Confirm");
			StartTrackingGlyph(new TrackableGlyph(debugImageToTrack, "Menus", "Confirm"));
			
			lastKnownDevice = InputSystem.devices[0].name;
		}

		private void OnEnable()
			=> InputSystem.onActionChange += OnActionChange;

		private void OnDisable()
			=> InputSystem.onActionChange -= OnActionChange;
		
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
	        
	        InputControl[] controls = targetAction.controls.ToArray();
	        foreach (InputControl control in controls)
	        {
		        foreach (InputDevice device in InputSystem.devices)
		        {
			        Debug.Log($"Balls: {control.device} / {device}");
			        if (control.device != device) continue;
			        
			        List<GlyphEntry> foundList = config.GetGlyphList(device.name);
			        if (foundList == null)
				        foreach (var platform in config.ControllerLayouts)
				        {
					        if (!InputSystem.IsFirstLayoutBasedOnSecond(device.layout, platform)) continue;
					        foundList = config.GetGlyphList(platform);
					        break;
				        }
			        if (foundList == null) break; // If it's still null, just end the search
			        
			        // Note: only the first control per device will be returned. This acts as the "primary" control.
			        Sprite foundSprite = foundList.Find(g => g.GlyphPath == control.path).GlyphSprite;
			        if (logOptions.ShowInfoLogs) 
				        Debug.Log($"[INFO] Returned glyph {foundSprite.name} for input action \"{actionName}\".");
			        return foundSprite;
		        }
	        }

	        Debug.LogError($"[ERROR] Glyph could not be found at input action \"{actionName}\" within the connected control schemes.");
	        return config.ErrorGlyphSprite;
        }

        public void StartTrackingGlyph(TrackableGlyph glyphToTrack)
        {
	        if (trackedGlyphs.Contains(glyphToTrack))
	        {
		        if (logOptions.ShowWarningLogs)
			        Debug.LogWarning($"[WARNING] Glyph {glyphToTrack.ImageToTrack.name} is already being tracked.");
		        return;
	        }
	        trackedGlyphs.Add(glyphToTrack);
	        if (!logOptions.ShowInfoLogs) return;
	        Debug.Log($"[INFO] Started tracking glyph {glyphToTrack.ImageToTrack.name} with action {glyphToTrack.ActionName}.");
        }

        public void StopTrackingGlyph(TrackableGlyph glyphToRemove)
        {
	        if (!trackedGlyphs.Contains(glyphToRemove))
	        {
		        if (logOptions.ShowWarningLogs)
			        Debug.LogWarning($"[WARNING] Glyph {glyphToRemove.ImageToTrack.name} is not currently being tracked.");
		        return;
	        }
	        trackedGlyphs.Remove(glyphToRemove);
	        if (!logOptions.ShowInfoLogs) return;
	        Debug.Log($"[INFO] Stopped tracking glyph {glyphToRemove.ImageToTrack.name}.");
        }
        
        // ======================[#]  INTERNAL FUNCTIONS: INPUT SYSTEM HANDLING  [#]======================

        private void OnActionChange(object obj, InputActionChange change)
        {
	        if (change != InputActionChange.ActionPerformed) return;
	        InputAction action = (InputAction)obj;
	        InputDevice newDevice = action.activeControl.device;
	        if (newDevice.name == lastKnownDevice) return;
	        lastKnownDevice = newDevice.name;
	        UpdateTrackedGlyphs();
        }

        private void UpdateTrackedGlyphs()
        {
	        int iteration = 0;
	        foreach (var glyph in trackedGlyphs)
	        {
		        glyph.ImageToTrack.sprite = GetGlyph(glyph.ActionMapName, glyph.ActionName);
		        iteration++;
	        }
	        if (!logOptions.ShowInfoLogs) return;
	        Debug.Log($"[INFO] Updated {iteration} tracked glyphs.");
        }
	}
}