using FrameLabs.AI.Nodes;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.NodeEditor.Utilities
{

    public class NodeGraphPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets( string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths )
        {
            foreach (string path in importedAssets)
            {
                if (!path.EndsWith(".asset"))
                    continue;

                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (Object obj in assets)
                {
                    if (obj is ScriptableObject so)
                    {
                        ApplyIcon(so);
                    }
                }
            }
        }

        private static void ApplyIcon( ScriptableObject obj )
        {
            Texture2D icon = NodeGraphIconInitializer.GetIconForObjectType( obj.GetType() );

            if( icon != null )
            {
                EditorGUIUtility.SetIconForObject( obj, icon );
                EditorUtility.SetDirty(obj);
            }
        }
    }


}