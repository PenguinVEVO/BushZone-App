using System;
using System.Collections.Generic;
using System.IO;
using FrameLabs.Utilities;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.AI.Blackboard
{
    public class ClassGenerator
    {
        public void WriteUsings( StreamWriter writer )
        {
            writer.WriteLine( "using System.Collections.Generic;" );
            writer.WriteLine( "using FrameLabs.AI.Blackboard;" );
            writer.WriteLine( "using UnityEditor;" );
            writer.WriteLine( "using UnityEngine;" );
            writer.WriteLine();
        }

        public void WriteNamespaceStart( StreamWriter writer, string namespaceName )
        {
            if( !string.IsNullOrEmpty( namespaceName ) )
            {
                writer.WriteLine( $"namespace {namespaceName}" );
                writer.WriteLine( "{" );
            }
        }

        public void WriteNamespaceEnd( StreamWriter writer, string namespaceName )
        {
            if( !string.IsNullOrEmpty( namespaceName ) )
            {
                writer.WriteLine( "}" );
            }
        }

        public void WriteClassStart( StreamWriter writer, string className, string namespaceName )
        {
            if( !string.IsNullOrEmpty( namespaceName ) )
            {
                writer.WriteLine( $"\tpublic class {className} : BlackboardData" );
                writer.WriteLine( "\t{" );
            }
            else
            {
                writer.WriteLine( $"public class {className} : BlackboardData" );
                writer.WriteLine( "{" );
            }
        }

        public void WriteClassEnd( StreamWriter writer, string namespaceName )
        {
            if( !string.IsNullOrEmpty( namespaceName ) )
            {
                writer.WriteLine( "\t}" );
            }
            else
            {
                writer.WriteLine( "}" );
            }
        }

        public void WriteOnEnable( StreamWriter writer, List<BlackboardEntry> entries )
        {
            writer.WriteLine( "\t\tvoid OnEnable()" );
            writer.WriteLine( "\t\t{" );

            foreach( var entry in entries )
            {
                string fieldName = entry.Name;

                if( entry.IsSerialized )
                    fieldName = Utility.Instance.CharacterToLower( entry.Name, 0 ) + entry.Name.Substring( 1 );

                if( entry.Type == DataType.List )
                {
                    writer.WriteLine( "#if UNITY_EDITOR" );
                    writer.WriteLine( $"\t\t\t{fieldName} = new List<Component>();" );

                    var listWrapper = JsonUtility.FromJson<Listwrapper<string>>( entry.Value );
                    foreach( var path in listWrapper.list )
                    {
                        string[] componentData = path.Split( '|' );
                        string componentPath = componentData[ 0 ];
                        string componentType = componentData[ 1 ].Split( ',' )[ 0 ];

                        writer.WriteLine( $"\t\t\t{fieldName}.Add(AssetDatabase.LoadAssetAtPath<GameObject>(\"{componentPath}\").GetComponent<{componentType}>());" );
                    }
                    writer.WriteLine( "#endif" );
                }
                else if( entry.Type == DataType.GameObject )
                {
                    writer.WriteLine( "#if UNITY_EDITOR" );
                    writer.WriteLine( $"\t\t\t{fieldName} = AssetDatabase.LoadAssetAtPath<GameObject>(\"{entry.Value}\");" );
                    writer.WriteLine( "#endif" );
                }
            }

            writer.WriteLine( "\t\t}" );
        }

        private string MakeAutoProperty(BlackboardEntry entry)
        {
            string propertyType = entry.Type.ToString().ToLower();

            if (entry.Type == DataType.GameObject)
            {
                propertyType = "GameObject";
            }
            else if (entry.Type == DataType.List)
            {
                propertyType = "List<Component>";
            }
            else if (entry.Type == DataType.Vector2)
            {
                propertyType = "Vector2";
            }
            else if (entry.Type == DataType.Vector3)
            {
                propertyType = "Vector3";
            }
            else if (entry.Type == DataType.Vector4)
            {
                propertyType = "Vector4";
            }
            else if (entry.Type == DataType.Color)
            {
                propertyType = "Color";
            }

            string propertyName = Utility.Instance.CharacterToUppercase(entry.Name, 0) + entry.Name.Substring(1);

            string value = string.Empty;
            if (!string.IsNullOrEmpty(entry.Value))
            {
                if (entry.Type == DataType.GameObject || entry.Type == DataType.List)
                {
                    value = " = new List<Component>();";
                }
                else if (entry.Type == DataType.Color)
                {
                    var color = entry.Value.Split(',');
                    if (color.Length == 4)
                    {
                        value = $" = new Color({color[0].Trim()}f, {color[1].Trim()}f, {color[2].Trim()}f, {color[3].Trim()}f);";
                    }
                }
                else if (entry.Type == DataType.Vector2 || entry.Type == DataType.Vector3 || entry.Type == DataType.Vector4)
                {
                    var vecValues = entry.Value.Split(',');
                    if (vecValues.Length == 2 && entry.Type == DataType.Vector2)
                    {
                        value = $" = new Vector2({vecValues[0].Trim()}f, {vecValues[1].Trim()}f);";
                    }
                    else if (vecValues.Length == 3 && entry.Type == DataType.Vector3)
                    {
                        value = $" = new Vector3({vecValues[0].Trim()}f, {vecValues[1].Trim()}f, {vecValues[2].Trim()}f);";
                    }
                    else if (vecValues.Length == 4 && entry.Type == DataType.Vector4)
                    {
                        value = $" = new Vector4({vecValues[0].Trim()}f, {vecValues[1].Trim()}f, {vecValues[2].Trim()}f, {vecValues[3].Trim()}f);";
                    }
                }
                else
                {
                    value = $" = {entry.Value};";
                }
            }

            return $"\t\tpublic {propertyType} {propertyName} {{ get; set; }}{value}{Environment.NewLine}";
        }


        public void WriteEntry( StreamWriter writer, BlackboardEntry entry )
        {
            if( entry.Scope == FieldScope.Public )
            {
                writer.WriteLine( MakeAutoProperty( entry ) );
            }
            else
            {
                string type = entry.Type == DataType.List ? "List<Component>" :
                              entry.Type == DataType.GameObject ? "GameObject"
                              : entry.Type.ToString().ToLower();

                string fieldName = Utility.Instance.CharacterToLower( entry.Name, 0 ) + entry.Name.Substring( 1 );
                string propertyName = Utility.Instance.CharacterToUppercase( entry.Name, 0 ) + entry.Name.Substring( 1 );
                string serializedField = entry.IsSerialized ? "[SerializeField] " : string.Empty;

                // Handle special case for GameObject and List<Component> types
                string defaultValue = string.Empty;
                if( entry.Type == DataType.GameObject || entry.Type == DataType.List )
                {
                    defaultValue = "null"; // GameObject and List<Component> should be initialized in OnEnable
                }
                else if( !string.IsNullOrEmpty( entry.Value ) )
                {
                    defaultValue = entry.Value;
                }

                // Create private field
                writer.WriteLine( $"\t\t{serializedField}private {type} {fieldName} = {defaultValue};" );

                // Create public getter
                if( entry.IsSerialized )
                {
                    writer.WriteLine( $"\t\tpublic {type} {propertyName} => {fieldName};" );
                }
                else
                {
                    writer.WriteLine( MakeAutoProperty( entry ) );
                }
            }
        }

        public void WriteCloneMethod( StreamWriter writer, string className, List<BlackboardEntry> entries )
        {
            writer.WriteLine( $"\t\tpublic override BlackboardData Clone()" );
            writer.WriteLine( "\t\t{" );
            writer.WriteLine( $"\t\t\tvar clone = CreateInstance<{className}>();" );

            foreach( var entry in entries )
            {
                string fieldName = entry.Name;

                switch( entry.Type )
                {
                    case DataType.Int:
                    case DataType.Float:
                    case DataType.String:
                    case DataType.Bool:
                        // Directly assign value types and strings
                        writer.WriteLine( $"\t\t\tclone.{fieldName} = {fieldName};" );
                        break;

                    case DataType.Vector2:
                        writer.WriteLine( $"\t\t\tclone.{fieldName} = new Vector2({fieldName}.x, {fieldName}.y);" );
                        break;

                    case DataType.Vector3:
                        writer.WriteLine( $"\t\t\tclone.{fieldName} = new Vector3({fieldName}.x, {fieldName}.y, {fieldName}.z);" );
                        break;

                    case DataType.Vector4:
                        writer.WriteLine( $"\t\t\tclone.{fieldName} = new Vector4({fieldName}.x, {fieldName}.y, {fieldName}.z, {fieldName}.w);" );
                        break;

                    case DataType.Color:
                        writer.WriteLine( $"\t\t\tclone.{fieldName} = new Color({fieldName}.r, {fieldName}.g, {fieldName}.b, {fieldName}.a);" );
                        break;

                    case DataType.List:
                            writer.WriteLine( $"\t\t\tclone.{fieldName} = new List<Component>();" );
                            writer.WriteLine( $"\t\t\tif ({fieldName} != null)" );
                            writer.WriteLine( $"\t\t\t{{" );
                            writer.WriteLine( $"\t\t\t\tforeach (var item in {fieldName})" );
                            writer.WriteLine( $"\t\t\t\t{{" );
                            writer.WriteLine( $"\t\t\t\t\tclone.{fieldName}.Add(item != null ? Instantiate(item) : null);" );
                            writer.WriteLine( $"\t\t\t\t}}" );
                            writer.WriteLine( $"\t\t\t}}" );
                        break;

                    case DataType.GameObject:
                        writer.WriteLine( $"\t\t\tclone.{fieldName} = {fieldName} != null ? Instantiate({fieldName}) : null;" );
                        break;

                    default:
                        writer.WriteLine( $"\t\t\tclone.{fieldName} = {fieldName};" );
                        break;
                }
            }

            writer.WriteLine( "\t\t\treturn clone;" );
            writer.WriteLine( "\t\t}" );
        }


        public void GenerateScript( string path, BlackboardDataEntry blackboardData )
        {
            using( StreamWriter writer = new StreamWriter( path ) )
            {
                WriteUsings( writer );
                WriteNamespaceStart( writer, blackboardData.Namespace );
                writer.WriteLine();

                // Write the class
                WriteClassStart( writer, blackboardData.Classname, blackboardData.Namespace );
                foreach( var entry in blackboardData.Entries )
                {
                    WriteEntry( writer, entry );
                }

                WriteOnEnable( writer, blackboardData.Entries );
                WriteCloneMethod( writer, blackboardData.Classname, blackboardData.Entries );

                WriteClassEnd( writer, blackboardData.Namespace );
                WriteNamespaceEnd( writer, blackboardData.Namespace );
            }

            AssetDatabase.Refresh();
        }
    }

}
