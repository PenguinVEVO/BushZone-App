using UnityEditor;
using UnityEngine;


namespace FrameLabs.NodeEditor.Utilities
{
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    [CustomEditor( typeof( NodeGraphData ) )]
    public class NodeGraphEditPreview : Editor
    {
        private Texture2D graphIcon;

        private void OnEnable()
        {
            graphIcon = NodeGraphIconInitializer.GetIconForObjectType( typeof( NodeGraphData ) );
        }

        public override Texture2D RenderStaticPreview( string assetPath, Object[] subAssets, int width, int height )
        {
            if( graphIcon != null )
            {
                // Resize the icon for static preview
                Texture2D resizedIcon = new Texture2D( width, height );
                EditorUtility.CopySerialized( graphIcon, resizedIcon );

                return resizedIcon;
            }

            return base.RenderStaticPreview( assetPath, subAssets, width, height );
        }
    }
}