using BZApp.GUI.Components.Configs;
using UnityEditor;
using UnityEngine;

namespace BZAppEditor.GUI
{
    public sealed class GlyphsManagerConfigEditor : Editor
    {
        private GlyphsManagerConfig config;

        private void Awake()
        {
            config = (GlyphsManagerConfig)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space(5);
            if (GUILayout.Button("Generate Control Entries")) return;
        }
    }
}
