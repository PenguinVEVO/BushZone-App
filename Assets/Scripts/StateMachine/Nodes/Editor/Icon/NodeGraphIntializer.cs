using FrameLabs.AI.Nodes;
using FrameLabs.Utilities;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.NodeEditor.Utilities
{

    [InitializeOnLoad]
    public static class NodeGraphIconInitializer
    {

        public static readonly string assetPath = "Assets";
       
        static NodeGraphIconInitializer()
        {
            ReapplyIcons();
        }

        public static void ReapplyIcons()
        {
            string[] graphGuids = AssetDatabase.FindAssets("t:NodeGraphData");
            foreach (string guid in graphGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject graphData = AssetDatabase.LoadAssetAtPath<NodeGraphData>(path);
                if (graphData != null)
                {
                    ApplyIcon(graphData);
                }
            }

            string[] nodeGuids = AssetDatabase.FindAssets("t:Node");
            foreach (string guid in nodeGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject node = AssetDatabase.LoadAssetAtPath<Node>(path);
                if (node != null)
                {
                    ApplyIcon(node);
                }
            }
        }

        public static Texture2D GetIconForObjectType(System.Type objType)
        {
            if (!Directory.Exists(assetPath))
            {
                Debug.LogWarning($"{assetPath} not found, using default icon");
                return null;
            }

            string iconFile = objType == typeof(StartNode) ? "StartNodeIcon.png"
                             : objType == typeof(ActionNode) ? "ActionNodeIcon.png"
                             : objType == typeof(NodeGraphData) ? "NodeGraphIcon.png"
                             : objType == typeof(ExitNode) ? "ExitNodeIcon.png"
                             : null;

            if (string.IsNullOrEmpty(iconFile)) return null;

            string file = Utility.Instance.FindFileInPath(assetPath, iconFile);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(file);
        }

        private static void ApplyIcon(ScriptableObject obj)
        {
            Texture2D icon = GetIconForObjectType(obj.GetType());
            if (icon != null)
            {
                EditorGUIUtility.SetIconForObject(obj, icon);
            }
        }
    }
}