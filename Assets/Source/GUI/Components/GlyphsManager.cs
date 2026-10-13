//  Cross-Device Glyphs Manager
//  By PenguinVEVO
//  Created October 2026
//
//  =============================[!]  AI ASSISTANCE DISCLAIMER  [!]=============================
//  This script was created with the help of ChatGPT. The areas assisted with AI are as follows:
//  • How to detect for input device changes with project-wide input actions:
//    https://share.google/aimode/kbDbzVPeAgnmNYEFK
//  • How to find the control scheme from an InputAction:
//    https://share.google/aimode/B45qbcHOgVUsvnVtw
//  ============================================================================================

using System.Collections.Generic;
using System.Linq;
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
		[SerializeField] private LogOptions logOptions;
		
		// ======================[#]  INTERNAL VARIABLES/REFERENCES  [#]======================

		private List<TrackableGlyph> trackedGlyphs = new();

		private InputDevice lastKnownDevice;
		private string lastKnownScheme;

		// ======================[#]  LIFECYCLE FUNCTIONS  [#]======================
		
		protected override void Awake() 
		{
			base.Awake();
			lastKnownDevice = InputSystem.devices[0];
			lastKnownScheme = lastKnownDevice.layout;
		}

		private void OnEnable()
			=> InputSystem.onActionChange += OnActionChange;

		private void OnDisable()
			=> InputSystem.onActionChange -= OnActionChange;
		
        // ======================[#]  GLYPHS MANAGER API  [#]======================

        public Sprite GetGlyph(string actionMapName, string actionName)
        {
	        // Turn the parameters into tangible stuff that the function can work with
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
	        
	        // Get all the supported devices of the current control scheme (to get the glyph via the scheme rather than specific device)
	        InputControlScheme currentScheme = InputSystem.actions.controlSchemes.FirstOrDefault(s => s.SupportsDevice(lastKnownDevice));
	        var supportedDeviceNames = new string[currentScheme.deviceRequirements.Count];
	        int iteration = 0;
	        supportedDeviceNames[iteration++] = lastKnownDevice.name;
	        foreach (InputControlScheme.DeviceRequirement requirement in currentScheme.deviceRequirements
		                 .Where(d =>!d.controlPath.Contains(lastKnownDevice.name)))
	        {
		        string originalDeviceName = requirement.controlPath;
		        string cleanDeviceName = originalDeviceName.Replace("<", "").Replace(">", "");
		        supportedDeviceNames[iteration++] = cleanDeviceName;
	        }
	        
	        // Go through all the controls until it lands on the one for the current control scheme
	        InputControl[] controls = targetAction.controls.ToArray();
	        foreach (InputControl control in controls)
	        {
		        if (!supportedDeviceNames.Contains(control.device.name)) continue;
		        foreach (var device in supportedDeviceNames)
		        {
			        // Find the glyph list for each device in this control scheme. This first bit weeds out any unsupported devices in the control scheme.
			        List<GlyphEntry> foundList = config.GetGlyphList(device);
			        if (foundList == null)
			        {
				        foreach (var layout in config.ControllerLayouts)
				        {
					        if (!InputSystem.IsFirstLayoutBasedOnSecond(lastKnownDevice.layout, layout)) continue;
					        foundList = config.GetGlyphList(layout);
					        break;
				        }
				        if (foundList == null) continue; // If it's still null, just end the search
			        }
			        
			        // This next bit tries to get the glyph for the current device and skips over if it's not found
			        // (for example, mouse was passed through but it's keyboard that has the control)
			        GlyphEntry foundEntry = foundList.Find(g => g.GlyphPath == control.path);
			        if (foundEntry == null) continue; // Likely not the right device in the control scheme
			        if (logOptions.ShowInfoLogs) 
				        Debug.Log($"[INFO] Returned glyph {foundEntry.GlyphSprite.name} for input action \"{actionName}\".");
			        return foundEntry.GlyphSprite;
		        }
	        }
	        
	        // "Oh well... guess that's that, then." (Just end the search here lol)
	        string controlSchemeList = InputSystem.devices.Aggregate("", (current, device) => current + $"\n• {device.layout}");
	        Debug.LogError($"[ERROR] Glyph could not be found at input action \"{actionName}\" within the following connected control schemes:" +
	                       $"\n{controlSchemeList}");
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
	        int bindingIndex = action.GetBindingIndexForControl(action.activeControl);
	        if (bindingIndex == -1 || action.bindings[bindingIndex].groups == lastKnownScheme) return;
	        lastKnownScheme = action.bindings[bindingIndex].groups;
	        
	        InputDevice newDevice = action.activeControl.device;
	        if (newDevice.name == lastKnownDevice.name) return;
	        lastKnownDevice = newDevice;
	        if (logOptions.ShowInfoLogs) Debug.Log($"[INFO] Detected new input device \"{newDevice.name}\".");
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