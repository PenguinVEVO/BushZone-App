using FrameLabs.AI.Nodes;

namespace FrameLabs.Utilities.NodeEditor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor( typeof( Node ), true )]
    public class BaseNodeInspector : Editor
    {
        protected bool showDefaultListView = true;
        protected bool showChildNodes = true;

        private void DrawDefaultList(Node node)
        {
            EditorGUILayout.BeginVertical("box");

            showChildNodes = EditorGUILayout.Foldout(showChildNodes, "Elements", true, EditorStyles.foldout);

            if (showChildNodes)
            {
                if (node.childNodes != null && node.childNodes.Count > 0)
                {
                    EditorGUI.indentLevel++;
                    for (int i = 0; i < node.childNodes.Count; i++)
                    {
                        EditorGUILayout.BeginHorizontal();

                        // Display the child node field
                        node.childNodes[i] = (Node)EditorGUILayout.ObjectField($"Child Node {i + 1}", node.childNodes[i], typeof(Node), true);

                        // Add remove button to remove the node
                        if (GUILayout.Button("-", GUILayout.Width(60)))
                        {
                            node.childNodes.RemoveAt(i);
                            i--; // Adjust index to account for removed item
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                else
                {
                    EditorGUILayout.HelpBox("No child nodes assigned.", MessageType.Info);
                }

                EditorGUILayout.Space();

                // Add button to add a new child node
                if (GUILayout.Button("Add Child Node"))
                {
                    node.childNodes.Add(null); // Add an empty slot for a new child node
                }
            }

            EditorGUILayout.EndVertical();
        }

        public override void OnInspectorGUI()
        {
            Node node = (Node)target;

            EditorGUILayout.LabelField( "Node Properties", EditorStyles.boldLabel );

            // Display the Execute Once toggle
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField( "Execute Once", GUILayout.Width( 120 ) );
            node.executeOnce = EditorGUILayout.Toggle( node.executeOnce, GUILayout.Width( 15 ) );
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            // Only show the default list view if the flag is true
            if( showDefaultListView)
            {
                DrawDefaultList(node);           
            }

            // Apply any changes made in the inspector
            if ( GUI.changed )
            {
                EditorUtility.SetDirty( node );
            }
        }
    }

}