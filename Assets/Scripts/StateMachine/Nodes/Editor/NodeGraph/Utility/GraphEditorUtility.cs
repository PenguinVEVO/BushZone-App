using FrameLabs.Utilities;
using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*
 * Name: Utility.cs
 * Date: 2/07/2024
 * Modified: 29/04/2025
 */
namespace FrameLabs.NodeEditor.Utilities
{
    public sealed class GraphEditorUtility
    {

        public GraphEditorUtility() { }

        private static readonly Lazy<GraphEditorUtility> instance = new();


        public static GraphEditorUtility Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Generates a ScriptableObject of the specified type and saves it as an asset.
        /// </summary>
        /// <param name="className">The name of the class for the ScriptableObject.</param>
        /// <param name="namespaceName">The namespace of the class, if any.</param>
        public void GenerateScriptableObject<T>( string outputPath, string className, string namespaceName = "" )
        {
            if( string.IsNullOrEmpty( outputPath ) )
            {
                Debug.LogError( "Destination path not specified" );
                return;
            }

            string scriptableObjectTypeName = string.IsNullOrEmpty( namespaceName ) ? className : $"{namespaceName}.{className}";

            string[] qualifiedNames = Utility.Instance.GetAssembly<T>( true );
            string qualifiedName = qualifiedNames.FirstOrDefault( qn => qn.Contains( scriptableObjectTypeName ) );

            if( string.IsNullOrEmpty( qualifiedName ) )
            {
                Debug.LogError( $"Failed to find qualified name for type: {scriptableObjectTypeName}" );
                return;
            }

            Type scriptableObjectType = Type.GetType( scriptableObjectTypeName );

            if( scriptableObjectType == null )
            {
                Debug.LogError( $"Failed to find type: {scriptableObjectTypeName}" );
                return;
            }

            ScriptableObject scriptableObjectInstance = ScriptableObject.CreateInstance( scriptableObjectType );

            if( scriptableObjectInstance == null )
            {
                Debug.LogError( $"Failed to create instance of: {scriptableObjectTypeName}" );
                return;
            }

            string assetPath = $"{outputPath}{className}.asset";
            AssetDatabase.CreateAsset( scriptableObjectInstance, assetPath );
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = scriptableObjectInstance;

            Debug.Log( $"ScriptableObject created at: {assetPath}" );
        }

        /// <summary>
        /// Attempts to resolve the first class defined in the specified script file.
        /// </summary>
        /// <param name="scriptAssetPath">Asset-relative path to a .cs file"</param>
        /// <returns>The Type of the class defined in the file, or null if not found.</returns>
        public Type GetTypeFromScriptPath( string scriptAssetPath )
        {
#if UNITY_EDITOR
            if( string.IsNullOrEmpty( scriptAssetPath ) || !scriptAssetPath.EndsWith( ".cs" ) )
                return null;

            MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>( scriptAssetPath );
            return script?.GetClass();
#else
            Debug.LogWarning("GetTypeFromScriptPath is only available in the Unity Editor.");
            return null;
#endif
        }

        /// <summary>
        /// Generates a ScriptableObject script from a given class type and writes it to the specified output path.
        /// </summary>
        /// <param name="classType">The class type to base the ScriptableObject on.</param>
        /// <param name="outputPath">The path where the generated script should be saved.</param>
        public void GenerateScriptableObject( Type classType, string outputPath )
        {
            // generate the script from the scriptableObject
            string script = $@"

                using UnityEngine;

                [CreateAssetMenu(fileName = ""New{classType.Name}"", menuName = ""ScriptableObjects/{classType.Name}"")]
                public class {classType.Name}ScriptableObject : ScriptableObject
                {{
                    
                    {GenerateFieldsFromType( classType )}

                }}";

            // write the generated script to the specified path
            string scriptPath = $"{outputPath}/{classType.Name}.cs";
            System.IO.File.WriteAllText( scriptPath, script );

            // refresh asset database
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Computes a stable, IL2CPP-compatible hash of the serialized graph data.
        /// Uses Unity's Hash128 to ensure compatibility across mobile and desktop builds.
        /// </summary>
        public int ComputeHash(NodeGraphData data)
        {
            string json = JsonUtility.ToJson(data);
            Hash128 hash = Hash128.Compute( json );

            return hash.GetHashCode();            
        }

        /// <summary>
        /// Generates field declarations based on the public instance fields of the provided class type.
        /// </summary>
        /// <param name="classType">The class type to generate fields from.</param>
        /// <returns>A string containing the field declarations.</returns>
        public string GenerateFieldsFromType( Type classType )
        {
            // Retrieve fields from class type
            var fields = classType.GetFields( BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic );

            string fieldDeclarations = string.Empty;

            // Generate field declarations
            foreach( var field in fields )
            {
                fieldDeclarations = $"public {field.FieldType.Name} {field.Name}";
            }

            return fieldDeclarations;
        }


    }
}