using System.Linq;
using System.Text.RegularExpressions;
using BZApp.GUI.Components.Configs;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BZAppEditor.GUI
{
    [CustomEditor(typeof(GlyphsManagerConfig))]
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
            if (GUILayout.Button("Generate Control Entries"))
                AutofillLists();
            if (GUILayout.Button("Clear Control Entries"))
                ClearLists();
        }

        private void AutofillLists()
        {
            foreach (string layoutName in config.ControllerLayouts)
            {
                InputDevice controller = InputSystem.AddDevice(layoutName);
                var targetList = config.GetGlyphList(layoutName);
                targetList.AddRange(from t in controller.allControls 
                                    where t != null 
                                    select new GlyphEntry(Regex.Replace(t.path, @"\d", ""), 
                                        t.name, null));
            }
        }

        private void ClearLists()
        {
            foreach (string layoutName in config.ControllerLayouts)
                config.GetGlyphList(layoutName).Clear();
        }
    }
}
