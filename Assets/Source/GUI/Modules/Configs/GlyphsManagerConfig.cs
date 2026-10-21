//  Config for Cross-Device Glyphs Manager
//  By PenguinVEVO
//  Created October 2026

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BZApp.GUI.Modules
{
    [CreateAssetMenu(fileName = "GlyphsManagerConfig", menuName = "UI/GUI Components/Configs/GlyphsManagerConfig")]
    public class GlyphsManagerConfig : ScriptableObject
    {
        public List<GlyphEntry> KeyboardGlyphSprites = new();
        public List<GlyphEntry> MouseGlyphSprites = new();
        public List<GlyphEntry> GamepadGlyphSprites = new();
        public List<GlyphEntry> XboxGlyphSprites = new();
        public List<GlyphEntry> PlaystationGlyphSprites = new();
        public List<GlyphEntry> SwitchProGlyphSprites = new();
        public Sprite ErrorGlyphSprite;
        
        public string[] ControllerLayouts { get; } =
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
            _ => null
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