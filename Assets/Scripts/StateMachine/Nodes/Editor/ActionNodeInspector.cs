using UnityEngine;
using UnityEditor;
using FrameLabs.AI.Nodes;
using System;
using FrameLabs.AI.Extension;

namespace FrameLabs.Utilities.NodeEditor
{
    [CanEditMultipleObjects]
    [CustomEditor( typeof(ActionNode), true )]
    public class ActionNodeInspector : BaseNodeInspector
    {
        private string[] extensionDisplayNames;
        private string[] extensionTypeNames;
        private int selectedExtensionTypeIndex = 0;

        private void OnEnable()
        {
            extensionDisplayNames = Utility.Instance.GetAssembly<StateExtension>();
            extensionTypeNames = Utility.Instance.GetAssembly<StateExtension>( true );
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            ActionNode node = (ActionNode)target;

            EditorGUILayout.LabelField( "Logic Extension Script", EditorStyles.boldLabel );

            node.stateExtension = EditorGUILayout.ObjectField( node.stateExtension, typeof( StateExtension ), false ) as StateExtension;

            if( node.stateExtension == null )
            {
                selectedExtensionTypeIndex = EditorGUILayout.Popup( "Extension Type", selectedExtensionTypeIndex, extensionDisplayNames );

                if( GUILayout.Button("Create New Extension") )
                {
                    if( extensionTypeNames.Length == 0 )
                        return;

                    string selectedTypeName = extensionTypeNames[ selectedExtensionTypeIndex ];
                    Type selectedType = Type.GetType( selectedTypeName );

                    if( selectedType != null )
                    {
                        var action = CreateInstance( selectedType ) as StateExtension;
                        string path = EditorUtility.SaveFilePanelInProject("Save Extension", "NewExtension", "asset", "Please enter a file name to save the extension to");
                        if( path.Length > 0 )
                        {
                            AssetDatabase.CreateAsset( action, path );
                            AssetDatabase.SaveAssets();
                            node.stateExtension = action;
                        }
                    }
                    else
                    {
                        Debug.LogError("Selected extension type could not be found.");
                    }
                }
            }

            if( GUI.changed )
            {
                EditorUtility.SetDirty( target );
            }
        }
    }
}