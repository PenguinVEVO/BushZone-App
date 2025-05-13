using FrameLabs.AI.Nodes;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.NodeEditor.Utilities
{
    [InitializeOnLoad]
    [CustomEditor( typeof( ActionNode ) )]
    public class NodeEditPreview : Editor
    {
        private Texture2D nodeIcon;

        static NodeEditPreview()
        {
            NodeGraphIconInitializer.ReapplyIcons();
        }

        private void OnEnable()
        {
            nodeIcon = GetIconForNodeType( target.GetType() );
        }

        public override Texture2D RenderStaticPreview( string assetPath, Object[] subAssets, int width, int height )
        {
            if( nodeIcon != null )
            {
                Texture2D resizedIcon = new Texture2D( width, height );
                EditorUtility.CopySerialized( nodeIcon, resizedIcon );

                return resizedIcon;
            }

            return base.RenderStaticPreview( assetPath, subAssets, width, height );
        }

        private Texture2D GetIconForNodeType( System.Type nodeType )
        {
            if( nodeType == typeof( StartNode ) )
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>( $"{NodeGraphIconInitializer.assetPath}/StartNodeIcon.png" );
            }
            else if( nodeType == typeof( ActionNode ) )
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>($"{NodeGraphIconInitializer.assetPath}/ActionNodeIcon.png");
            }
            else if ( nodeType == typeof( ExitNode ) ) 
            {
                return AssetDatabase.LoadAssetAtPath<Texture2D>($"{NodeGraphIconInitializer.assetPath}/ExitNodeIcon.png");
            }

            return null;
        }
    }

}