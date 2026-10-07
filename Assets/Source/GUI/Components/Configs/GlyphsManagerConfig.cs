using UnityEngine;

namespace BZApp.GUI.Components.Configs
{
    [CreateAssetMenu(fileName = "GlyphsManagerConfig", menuName = "UI/GUI Components/Configs/GlyphsManagerConfig")]
    public class GlyphsManagerConfig : ScriptableObject
    {
        [Header("Glyph Dictionaries")]
        public Sprite[] KeyboardGlyphSprites;
        public Sprite[] MouseGlyphSprites;
        public Sprite[] GamepadGlyphSprites;
        public Sprite[] XboxGlyphSprites;
        public Sprite[] PlaystationGlyphSprites;
        public Sprite[] SwitchProGlyphSprites;
        public Sprite ErrorGlyphSprite;
    }
}