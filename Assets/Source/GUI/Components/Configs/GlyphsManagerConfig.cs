using System;
using System.Collections.Generic;
using UnityEngine;

namespace BZApp.GUI.Components.Configs
{
    [CreateAssetMenu(fileName = "GlyphsManagerConfig", menuName = "UI/GUI Components/Configs/GlyphsManagerConfig")]
    public class GlyphsManagerConfig : ScriptableObject
    {
        // Glyph collections
        public List<GlyphEntry> KeyboardGlyphSprites = new();
        public List<GlyphEntry> MouseGlyphSprites = new();
        public List<GlyphEntry> GamepadGlyphSprites = new();
        public List<GlyphEntry> XboxGlyphSprites = new();
        public List<GlyphEntry> PlaystationGlyphSprites = new();
        public List<GlyphEntry> SwitchProGlyphSprites = new();
        public Sprite ErrorGlyphSprite;
        
        // Accessor properties
        public string[] ControllerLayouts => controllerLayouts;
        
        // Controller layout collections
        private readonly string[] controllerLayouts =
        {
            "Keyboard",
            "Mouse",
            "Gamepad",
            "XInputController",
            "DualShockGamepad",
            "SwitchProControllerHID"
        };
        
        public List<GlyphEntry> GetGlyphList(string layoutName) => layoutName switch
        {
            "Keyboard" => KeyboardGlyphSprites,
            "Mouse" => MouseGlyphSprites,
            "Gamepad" => GamepadGlyphSprites,
            "XInputController" => XboxGlyphSprites,
            "DualShockGamepad" => PlaystationGlyphSprites,
            "SwitchProControllerHID" => SwitchProGlyphSprites,
            _ => throw new ArgumentException($"Invalid controller layout: {layoutName}")
        };
    }
    
    [Serializable]
    public class GlyphEntry
    {
        public string GlyphPath;
        public string GlyphName;
        public Sprite GlyphSprite;
        
        public GlyphEntry(string glyphPath, string glyphName, Sprite glyphSprite)
        {
            GlyphPath = glyphPath;
            GlyphName = glyphName;
            GlyphSprite = glyphSprite;
        }
    }
}