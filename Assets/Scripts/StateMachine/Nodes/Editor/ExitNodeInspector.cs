using FrameLabs.AI.Nodes;
using UnityEditor;
using UnityEngine;


namespace FrameLabs.Utilities.NodeEditor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(ExitNode), true)]
    public class ExitNodeInspector : BaseNodeInspector
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ExitNode node = (ExitNode)target;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Exit Code", GUILayout.Width(120));
            EditorGUILayout.LabelField(new GUIContent(node.ExitCode.ToString()), GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Reset Tree", GUILayout.Width(120));
            node.RestartTree = EditorGUILayout.Toggle(node.RestartTree, GUILayout.Width(15));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }
}