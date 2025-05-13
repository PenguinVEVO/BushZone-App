using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphWriter
    {
        private static readonly GraphWriter instance = new GraphWriter();
        public static GraphWriter Instance => instance;

        private GraphWriter() { }

        private bool ValidatePath( string fullPath )
        {
            if( !Directory.Exists( fullPath ) )
            {
                Debug.LogError( $"[GraphWriter] Path does not exist: {fullPath}" );
                return false;
            }
            return true;
        }

        public bool WriteGraph( NodeGraphData data, string path )
        {
            if( data == null )
            {
                Debug.LogError( "[GraphWriter] Cannot write graph: data is null." );
                return false;
            }

            if( string.IsNullOrEmpty( path ) )
            {
                Debug.LogError( "[GraphWriter] Cannot write graph: path is null or empty." );
                return false;
            }

            string dir = Path.GetDirectoryName( path );
            if( !ValidatePath( dir ) )
            {
                Debug.LogError( $"[GraphWriter] Invalid path: {path}" );
                return false;
            }

            string existingPath = AssetDatabase.GetAssetPath( data );
            if( !string.IsNullOrEmpty( existingPath ) && existingPath == path )
            {
                EditorUtility.SetDirty( data );
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                return true;
            }

            NodeGraphData existingAsset = AssetDatabase.LoadAssetAtPath<NodeGraphData>( path );
            if( existingAsset != null )
            {
                var copied = CopyAndSaveAsset( data, existingAsset, "NodeGraphData" );
                if( copied == null )
                {
                    Debug.LogError( "[GraphWriter] Copy operation failed." );
                    return false;
                }
            }
            else
            {
                try
                {
                    data.name = Path.GetFileNameWithoutExtension(path);
                    AssetDatabase.CreateAsset( data, path );
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                catch( Exception ex )
                {
                    Debug.LogError( $"[GraphWriter] Failed to create asset at path '{path}': {ex.Message}" );
                    return false;
                }
            }

            return true;
        }



        public bool SaveGraphForTab( string tabName, NodeGraphData data, string customPath = null )
        {
            string path = string.IsNullOrEmpty( customPath )
                ? $"Assets/Cache/{tabName}.asset"
                : customPath;

            return WriteGraph( data, path );
        }


        public bool TryGetAssetPath( NodeGraphData data, out string path )
        {
            path = AssetDatabase.GetAssetPath( data );
            return !string.IsNullOrEmpty( path );
        }

        public bool DeleteGraph( NodeGraphData data )
        {
            if( TryGetAssetPath( data, out var path ) )
            {
                return AssetDatabase.DeleteAsset( path );
            }
            return false;
        }

        public T CopyAndSaveAsset<T>( T sourceAsset, T targetAsset, string assetTypeName ) where T : ScriptableObject
        {
            if( sourceAsset == null )
            {
                Debug.LogError( $"[GraphWriter] Cannot copy asset: source {assetTypeName} is null." );
                return null;
            }

            if( targetAsset == null )
            {
                Debug.LogWarning( $"[GraphWriter] Creating new asset for {assetTypeName} since target is null." );
                targetAsset = ScriptableObject.CreateInstance<T>();
            }

            EditorUtility.CopySerialized( sourceAsset, targetAsset );

            return targetAsset;
        }

    }

}