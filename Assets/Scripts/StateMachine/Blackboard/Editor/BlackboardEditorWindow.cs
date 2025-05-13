using FrameLabs.Utilities;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.IO;
using FrameLabs.NodeEditor.Utilities;

namespace FrameLabs.AI.Blackboard
{
    public class BlackboardEditorWindow : EditorWindow
    {
        private EditorHelper helper = EditorHelper.Instance;
        private ClassGenerator classGenerator = new ClassGenerator();

        private BlackboardDataEntry blackboardData;
        private SerializedObject serializedBlackboardData;

        private SerializedProperty mainEntriesProperty;
        private SerializedProperty namespaceProperty;
        private SerializedProperty classNameProperty;

        private string newEntryName = "";
        private DataType newEntryType = DataType.Int;
        private FieldScope newScope = FieldScope.Public;
        private bool newIsSerialized = false;
        private string newEntryValue = "";
        private GameObject fieldGameObject = null;
        private GameObject listTypeselectedObject = null;
        private int selectedComponentIndex = 0;
        private string[] componentTypeOptions = new string[ 0 ];
        private List<Component> serializableComponents = new List<Component>();

        private bool showFieldNameError;
        private string errorMessage;
        private bool showClassNameError;
        private string classNameErrorMessage;

        private string assetName = "BlackboardData";
        private string assetPath = "Assets/";

        private Vector2 scrollPos;

        [MenuItem( "Window/FrameLabs/Blackboard Editor" )]
        public static void ShowWindow()
        {
            GetWindow<BlackboardEditorWindow>( "Blackboard Editor" );
        }

        private bool LoadBlackboardData( string path )
        {
            blackboardData = AssetDatabase.LoadAssetAtPath<BlackboardDataEntry>( path );
            return blackboardData != null;
        }

        private void CreateBlackboardData( string path )
        {
            blackboardData = CreateInstance<BlackboardDataEntry>();
            AssetDatabase.CreateAsset( blackboardData, path );
            AssetDatabase.SaveAssets();
        }

        private void SerializeBlackboard()
        {
            if( blackboardData != null )
            {
                serializedBlackboardData = new SerializedObject( blackboardData );
                var properties = new Dictionary<string, SerializedProperty>
            {
                { "Entries", serializedBlackboardData.FindProperty("Entries") },
                { "Namespace", serializedBlackboardData.FindProperty("Namespace") },
                { "Classname", serializedBlackboardData.FindProperty("Classname") }
            };

                mainEntriesProperty = properties[ "Entries" ];
                namespaceProperty = properties[ "Namespace" ];
                classNameProperty = properties[ "Classname" ];
            }
        }

        private void OnGUI()
        {
            DrawErrorMessages();

            if( !LoadBlackboardData( $"{assetPath}/{assetName}.asset" ) )
            {
                DrawCreateBlackboardSection();
                return;
            }
            else
            {
                SerializeBlackboard();
            }

            serializedBlackboardData.Update();

            DrawNamespaceAndClassName();
            DrawEntriesList();

            DrawAddNewEntrySection();
            DrawGenerateButtons();
            DrawLoadScriptButton();
            serializedBlackboardData.ApplyModifiedProperties();
        }

        private void DrawErrorMessages()
        {
            helper.DrawErrorMessage( showFieldNameError, errorMessage );
            helper.DrawErrorMessage( showClassNameError, classNameErrorMessage );
        }

        private void DrawCreateBlackboardSection()
        {
            EditorGUILayout.HelpBox( "BlackboardData asset not found or created. Please create a new asset.", MessageType.Warning );

            helper.DrawSection( "Create New Blackboard", () =>
            {
                if( helper.DrawButton( "Create New Blackboard" ) )
                {
                    CreateBlackboardData( $"{assetPath}/{assetName}.asset" );
                }
            } );
        }

        private void DrawNamespaceAndClassName()
        {
            helper.DrawSection( "Namespace", () => helper.DrawPropertyField( namespaceProperty, "Name" ) );
            helper.DrawSection( "Class Name", () => helper.DrawPropertyField( classNameProperty, "Name" ) );
        }

        private void DrawEntriesList()
        {
            scrollPos = EditorGUILayout.BeginScrollView( scrollPos, GUILayout.Height( position.height - 325 ) );
            EditorGUILayout.LabelField( "BlackBoard Data", EditorStyles.boldLabel );

            helper.DrawList( blackboardData.Entries, DrawEntry, RemoveEntry );

            EditorGUILayout.EndScrollView();
        }

        private void DrawAddNewEntrySection()
        {
            EditorGUILayout.BeginVertical( "box" );
            helper.DrawSection( "Add New Entry", () =>
            {
                DrawNewEntryFields();
                if( helper.DrawButton( "Add Entry" ) )
                {
                    if( string.IsNullOrEmpty( newEntryName ) )
                    {
                        showFieldNameError = true;
                        errorMessage = "Field Name property cannot be empty or null";
                    }
                    else
                    {
                        showFieldNameError = false;
                        AddNewEntry();
                    }
                }
            } );
            EditorGUILayout.EndVertical();
        }

        private void DrawNewEntryFields()
        {
            newScope = (FieldScope)helper.DrawEnumPopup( "Scope", newScope );

            if( newScope == FieldScope.Private )
                newIsSerialized = helper.DrawToggleField( "Serialized Field", newIsSerialized );
            else
                newIsSerialized = false;

            newEntryType = (DataType)helper.DrawEnumPopup( "Type", newEntryType );
            newEntryName = helper.DrawTextField( "Name", newEntryName );

            if( newEntryType == DataType.List )
            {
                DrawListTypeFields();
            }
            else if( newEntryType == DataType.GameObject )
            {
                fieldGameObject = helper.DrawObjectField( "GameObject", fieldGameObject );
            }
            else if (newEntryType == DataType.Vector2)
            {
                Vector2 vec2 = Vector2.zero;
                if (!string.IsNullOrEmpty(newEntryValue))
                {
                    string[] parts = newEntryValue.Split(',');
                    if (parts.Length == 2 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y))
                    {
                        vec2 = new Vector2(x, y);
                    }
                }

                vec2 = EditorHelper.Instance.DrawVector2Field("Vector2", vec2);
                newEntryValue = $"{vec2.x},{vec2.y}";
            }
            else if (newEntryType == DataType.Vector3)
            {
                Vector3 vec3 = Vector3.zero;
                if (!string.IsNullOrEmpty(newEntryValue))
                {
                    string[] parts = newEntryValue.Split(',');
                    if (parts.Length == 3 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y) &&
                        float.TryParse(parts[2], out float z))
                    {
                        vec3 = new Vector3(x, y, z);
                    }
                }

                vec3 = EditorHelper.Instance.DrawVector3Field("Vector3", vec3);
                newEntryValue = $"{vec3.x},{vec3.y},{vec3.z}";
            }
            else if (newEntryType == DataType.Vector4)
            {
                Vector4 vec4 = Vector4.zero;
                if (!string.IsNullOrEmpty(newEntryValue))
                {
                    string[] parts = newEntryValue.Split(',');
                    if (parts.Length == 4 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y) &&
                        float.TryParse(parts[2], out float z) &&
                        float.TryParse(parts[3], out float w))
                    {
                        vec4 = new Vector4(x, y, z, w);
                    }
                }

                vec4 = EditorHelper.Instance.DrawVector4Field("Vector4", vec4);
                newEntryValue = $"{vec4.x},{vec4.y},{vec4.z},{vec4.w}";
            }
            else if (newEntryType == DataType.Color)
            {
                Color color = Color.white;
                if (!string.IsNullOrEmpty(newEntryValue))
                {
                    string[] parts = newEntryValue.Split(',');
                    if (parts.Length == 4 &&
                        float.TryParse(parts[0], out float r) &&
                        float.TryParse(parts[1], out float g) &&
                        float.TryParse(parts[2], out float b) &&
                        float.TryParse(parts[3], out float a))
                    {
                        color = new Color(r, g, b, a);
                    }
                }

                color = EditorHelper.Instance.DrawColorField("Color", color);
                newEntryValue = $"{color.r},{color.g},{color.b},{color.a}";
            }
            else
            {
                newEntryValue = helper.DrawTextField( "Value", newEntryValue );
            }
        }

        private void DrawListTypeFields()
        {
            listTypeselectedObject = helper.DrawObjectField( "GameObject", listTypeselectedObject );

            if( listTypeselectedObject != null )
            {
                serializableComponents = Utility.Instance.GetSerializableComponents( listTypeselectedObject );
                componentTypeOptions = serializableComponents.Select( c => c.GetType().Name ).ToArray();
                selectedComponentIndex = 0;

                selectedComponentIndex = EditorGUILayout.Popup( "Component Type", selectedComponentIndex, componentTypeOptions );

                var selectedComponent = serializableComponents[ selectedComponentIndex ];
                var fields = selectedComponent.GetType().GetFields( BindingFlags.Public | BindingFlags.Instance );
                foreach( var field in fields )
                {
                    object fieldValue = field.GetValue( selectedComponent );
                    DrawField( field, fieldValue, selectedComponent );
                }
            }
            else
            {
                componentTypeOptions = new string[ 0 ];
            }
        }

        private void DrawGenerateButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if( helper.DrawButton( "Generate Script" ) )
            {
                GenerateScript();
            }

            if( helper.DrawButton( "Generate Object" ) )
            {
                if( string.IsNullOrEmpty( classNameProperty.stringValue ) )
                {
                    showClassNameError = true;
                    classNameErrorMessage = "Class Name property cannot be empty or null";
                }
                else
                {
                    showClassNameError = false;
                    GenerateScriptableObject();
                }
            }
            EditorGUILayout.EndHorizontal();
        }


        private void DrawLoadScriptButton()
        {
            EditorGUILayout.BeginHorizontal();

            if( helper.DrawButton( "Load Script" ) )
            {
                string path = EditorUtility.OpenFilePanel( "Load Blackboard Script", "", "cs" );
                if( !string.IsNullOrEmpty( path ) )
                {
                    LoadScript( path );
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

        }

        private void GenerateScriptableObject()
        {
            if( string.IsNullOrEmpty( blackboardData.Classname ) )
                return;

            GraphEditorUtility.Instance.GenerateScriptableObject<BlackboardData>(
                $"{assetPath}",
                blackboardData.Classname,
                blackboardData.Namespace );
        }

        private void AddNewEntry()
        {
            serializedBlackboardData.Update();
            mainEntriesProperty.arraySize++;

            var newEntry = mainEntriesProperty.GetArrayElementAtIndex( mainEntriesProperty.arraySize - 1 );

            if( string.IsNullOrEmpty( newEntryName ) )
                return;

            newEntry.FindPropertyRelative( "Name" ).stringValue = newEntryName;
            newEntry.FindPropertyRelative( "Type" ).enumValueIndex = (int)newEntryType;
            newEntry.FindPropertyRelative( "Scope" ).enumValueIndex = (int)newScope;
            newEntry.FindPropertyRelative( "IsSerialized" ).boolValue = newIsSerialized;

            AssignEntryValue( newEntry, newEntryType );

            serializedBlackboardData.ApplyModifiedProperties();
            EditorUtility.SetDirty( blackboardData );
            AssetDatabase.SaveAssets();
        }

        private void AssignEntryValue( SerializedProperty newEntry, DataType entryType )
        {
            switch( entryType )
            {
                case DataType.Int:
                    if( int.TryParse( newEntryValue, out int intValue ) )
                    {
                        newEntry.FindPropertyRelative( "Value" ).stringValue = intValue.ToString();
                    }
                    break;
                case DataType.Float:
                    string sanitizedFloatValue = newEntryValue.Replace( "f", "" ).Replace( "F", "" );
                    if( float.TryParse( sanitizedFloatValue, out float floatValue ) )
                    {
                        newEntry.FindPropertyRelative( "Value" ).stringValue = floatValue.ToString();
                    }
                    break;
                case DataType.String:
                    newEntry.FindPropertyRelative( "Value" ).stringValue = newEntryValue;
                    break;
                case DataType.Bool:
                    if( bool.TryParse( newEntryValue, out bool boolValue ) )
                    {
                        newEntry.FindPropertyRelative( "Value" ).stringValue = boolValue.ToString().ToLower();
                    }
                    break;
                case DataType.GameObject:
                    if( fieldGameObject != null )
                    {
                        string assetPath = AssetDatabase.GetAssetPath( fieldGameObject );

                        if( !string.IsNullOrEmpty( assetPath ) )
                        {
                            newEntry.FindPropertyRelative( "Value" ).stringValue = assetPath;
                        }
                        else
                        {
                            newEntry.FindPropertyRelative( "Value" ).stringValue = string.Empty;
                        }
                    }
                    break;
                case DataType.List:
                    HandleListTypeEntry( newEntry );
                    break;
                case DataType.Vector2:
                    AssignVectorValue<Vector2>(newEntry, 2);
                    break;
                case DataType.Vector3:
                    AssignVectorValue<Vector3>(newEntry, 3);
                    break;
                case DataType.Vector4:
                    AssignVectorValue<Vector3>(newEntry, 4);
                    break;

                case DataType.Color:
                    string[] colorParts = newEntryValue.Split(',');
                    if (colorParts.Length == 4 &&
                        float.TryParse(colorParts[0], out float r) &&
                        float.TryParse(colorParts[1], out float g) &&
                        float.TryParse(colorParts[2], out float b) &&
                        float.TryParse(colorParts[3], out float a))
                    {
                        Color color = new Color(r, g, b, a);
                        newEntry.FindPropertyRelative("Value").stringValue = $"{color.r},{color.g},{color.b},{color.a}";
                    }
                    break;
            }
        }

        private void AssignVectorValue<T>(SerializedProperty newEntry, int dimensions) where T : struct
        {
            // Remove parentheses and spaces
            string sanitizedVectorValue = newEntryValue.Replace("(", "").Replace(")", "").Trim();
            string[] splitValues = sanitizedVectorValue.Split(',');

            // Ensure we have the correct number of components for the vector type
            if (splitValues.Length == dimensions)
            {
                float[] parsedValues = new float[dimensions];
                bool allParsed = true;

                for (int i = 0; i < dimensions; i++)
                {
                    if (!float.TryParse(splitValues[i].Trim(), out parsedValues[i]))
                    {
                        allParsed = false;
                        break;
                    }
                }

                if (allParsed)
                {
                    // Construct the appropriate vector and assign it
                    string vectorString = string.Join(",", parsedValues);
                    newEntry.FindPropertyRelative("Value").stringValue = vectorString;
                }
            }
        }


        private void HandleListTypeEntry( SerializedProperty newEntry )
        {
            if( listTypeselectedObject != null )
            {
                Component selectedComponent = serializableComponents[ selectedComponentIndex ];
                if( selectedComponent != null )
                {
                    // Serialize the component reference
                    string componentPath = AssetDatabase.GetAssetPath( selectedComponent );
                    string componentType = selectedComponent.GetType().AssemblyQualifiedName;

                    if( !string.IsNullOrEmpty( componentPath ) && !string.IsNullOrEmpty( componentType ) )
                    {
                        var componentListWrapper = new Listwrapper<string>
                        {
                            list = new List<string> { $"{componentPath}|{componentType}" }
                        };

                        string json = JsonUtility.ToJson( componentListWrapper );
                        newEntry.FindPropertyRelative( "Value" ).stringValue = json;
                    }
                }
            }
            else
            {
                newEntry.FindPropertyRelative( "Value" ).stringValue = JsonUtility.ToJson( new Listwrapper<string>() );
            }
        }



        private void DrawEntry( BlackboardEntry entry, int index )
        {
            var helper = EditorHelper.Instance;
            var entryProperty = mainEntriesProperty.GetArrayElementAtIndex( index );

            if( entryProperty == null )
            {
                EditorGUILayout.HelpBox( "Entry property is null.", MessageType.Error );
                return;
            }

            var isSerializedProperty = entryProperty.FindPropertyRelative( "IsSerialized" );
            var scopeProperty = entryProperty.FindPropertyRelative( "Scope" );
            var typeProperty = entryProperty.FindPropertyRelative( "Type" );
            var nameProperty = entryProperty.FindPropertyRelative( "Name" );
            var valueProperty = entryProperty.FindPropertyRelative( "Value" );

            helper.DrawPropertyField( scopeProperty, "Scope" );
            if( (FieldScope)scopeProperty.enumValueIndex == FieldScope.Private )
            {
                helper.DrawPropertyField( isSerializedProperty, "Is Serialized" );
            }

            helper.DrawPropertyField( typeProperty, "Type" );
            helper.DrawPropertyField( nameProperty, "Name" );

            DrawEntryValueField( (DataType)typeProperty.enumValueIndex, valueProperty, index );
        }

        private void DrawEntryValueField( DataType type, SerializedProperty valueProperty, int index )
        {
            switch( type )
            {
                case DataType.Int:
                    helper.DrawEntryValueField<int>( valueProperty, "Value" );
                    break;
                case DataType.Float:
                    helper.DrawEntryValueField<float>( valueProperty, "Value" );
                    break;
                case DataType.String:
                    helper.DrawEntryValueField<string>( valueProperty, "Value" );
                    break;
                case DataType.Bool:
                    helper.DrawEntryValueField<bool>( valueProperty, "Value" );
                    break;
                case DataType.GameObject:
                    helper.DrawEntryValueField<GameObject>( valueProperty, "Value" );
                    break;
                case DataType.List:
                    DrawListValueField( valueProperty, index );
                    break;

                case DataType.Vector2:
                    helper.DrawEntryValueField<Vector2>( valueProperty, "Value" );
                    break;

                case DataType.Vector3:
                    helper.DrawEntryValueField<Vector3>(valueProperty, "Value");
                    break;

                case DataType.Vector4:
                    helper.DrawEntryValueField<Vector4>(valueProperty, "Value");
                    break;

                case DataType.Color:
                    helper.DrawEntryValueField<Color>(valueProperty, "Value");
                    break;

            }
        }

        private void DrawListValueField( SerializedProperty valueProperty, int index )
        {
            if( !string.IsNullOrEmpty( valueProperty.stringValue ) )
            {
                var listWrapper = JsonUtility.FromJson<Listwrapper<string>>( valueProperty.stringValue );
                var list = listWrapper?.list ?? new List<string>();

                EditorGUILayout.BeginVertical( "box" );

                for( int i = 0; i < list.Count; i++ )
                {
                    DrawListElement( list, i, index );
                }

                if( GUILayout.Button( "+" ) )
                {
                    AddNewListElement( list );
                }

                EditorGUILayout.EndVertical();

                valueProperty.stringValue = JsonUtility.ToJson( new Listwrapper<string> { list = list } );
            }
        }

        private void DrawListElement( List<string> list, int i, int index )
        {
            EditorGUILayout.BeginHorizontal();

            bool foldout = EditorPrefs.GetBool( $"ComponentFoldout_{index}_{i}", false );
            foldout = EditorGUILayout.Foldout( foldout, $"Element {i}", true );
            EditorPrefs.SetBool( $"ComponentFoldout_{index}_{i}", foldout );

            // Deserialize the component reference
            string[] componentData = list[ i ].Split( '|' );
            string componentPath = componentData[ 0 ];
            string componentType = componentData[ 1 ];

            Component component = AssetDatabase.LoadAssetAtPath<GameObject>( componentPath )?.GetComponent( Type.GetType( componentType ) );

            Component newComponent = (Component)EditorGUILayout.ObjectField( component, typeof( Component ), true );
            if( newComponent != null && newComponent != component )
            {
                string newComponentPath = AssetDatabase.GetAssetPath( newComponent.gameObject );
                string newComponentType = newComponent.GetType().AssemblyQualifiedName;

                if( !string.IsNullOrEmpty( newComponentPath ) && !string.IsNullOrEmpty( newComponentType ) )
                {
                    list[ i ] = $"{newComponentPath}|{newComponentType}";
                }
            }

            if( GUILayout.Button( "-" ) )
            {
                list.RemoveAt( i );
            }

            EditorGUILayout.EndHorizontal();

            if( foldout && newComponent != null )
            {
                EditorGUI.indentLevel++;
                SerializedObject serializedObject = new SerializedObject( newComponent );
                serializedObject.Update();
                SerializedProperty property = serializedObject.GetIterator();
                property.NextVisible( true );

                while( property.NextVisible( false ) )
                {
                    EditorGUILayout.PropertyField( property, true );
                }
                serializedObject.ApplyModifiedProperties();
                EditorGUI.indentLevel--;
            }
        }

        private void AddNewListElement( List<string> list )
        {
            if( list.Count > 0 )
            {
                // Retrieve the last entry in the list
                string lastEntry = list[ list.Count - 1 ];
                string[] componentData = lastEntry.Split( '|' );
                string lastGameObjectPath = componentData[ 0 ];
                string componentType = componentData[ 1 ];

                // Load the GameObject from the last entry
                GameObject lastGameObject = AssetDatabase.LoadAssetAtPath<GameObject>( lastGameObjectPath );
                if( lastGameObject != null )
                {
                    // Determine the base name (without the suffix)
                    string baseName = lastGameObject.name;
                    int underscoreIndex = baseName.LastIndexOf( '_' );
                    if( underscoreIndex >= 0 )
                    {
                        baseName = baseName.Substring( 0, underscoreIndex );
                    }

                    // Instantiate a copy of the last GameObject
                    GameObject newGameObject = Instantiate( lastGameObject );

                    // Set the new name with an incremented index
                    newGameObject.name = $"{baseName}_{list.Count + 1}";

                    // Optionally, save the new instance as a prefab
                    string prefabPath = $"Assets/{newGameObject.name}.prefab";
                    PrefabUtility.SaveAsPrefabAsset( newGameObject, prefabPath );


                    if( !string.IsNullOrEmpty( prefabPath ) && !string.IsNullOrEmpty( componentType ) )
                    {
                        // Add the combined reference of the new instance's GameObject path and its type
                        list.Add( $"{prefabPath}|{componentType}" );
                    }

                    // Destroy the new instance after saving it as a prefab
                    DestroyImmediate( newGameObject );
                }
            }
            else if( listTypeselectedObject != null )
            {
                // If the list is empty, add the first instance based on the selected object
                Component selectedComponent = serializableComponents[ selectedComponentIndex ];
                if( selectedComponent != null )
                {
                    GameObject newGameObject = Instantiate( listTypeselectedObject );
                    newGameObject.name = $"{listTypeselectedObject.name}_1";

                    string prefabPath = $"Assets/{newGameObject.name}.prefab";
                    PrefabUtility.SaveAsPrefabAsset( newGameObject, prefabPath );

                    if( !string.IsNullOrEmpty( prefabPath ) )
                    {
                        string componentType = selectedComponent.GetType().AssemblyQualifiedName;
                        list.Add( $"{prefabPath}|{componentType}" );
                    }

                    DestroyImmediate( newGameObject );
                }
            }
        }

        private void DrawField( FieldInfo field, object value, object component )
        {
            if( value == null )
            {
                EditorGUILayout.LabelField( field.Name, "null" );
                return;
            }

            Type fieldType = field.FieldType;

            if( fieldType == typeof( int ) )
            {
                int intValue = (int)value;
                intValue = EditorGUILayout.IntField( field.Name, intValue );
                field.SetValue( component, intValue );
            }
            else if( fieldType == typeof( float ) )
            {
                float floatValue = (float)value;
                floatValue = EditorGUILayout.FloatField( field.Name, floatValue );
                field.SetValue( component, floatValue );
            }
            else if( fieldType == typeof( string ) )
            {
                string stringValue = (string)value;
                stringValue = EditorGUILayout.TextField( field.Name, stringValue );
                field.SetValue( component, stringValue );
            }
            else if( fieldType == typeof( bool ) )
            {
                bool boolValue = (bool)value;
                boolValue = EditorGUILayout.Toggle( field.Name, boolValue );
                field.SetValue( component, boolValue );
            }
            else if( fieldType.IsEnum )
            {
                Enum enumValue = (Enum)value;
                enumValue = EditorGUILayout.EnumPopup( field.Name, enumValue );
                field.SetValue( component, enumValue );
            }
            else if( fieldType == typeof( Vector2 ) )
            {
                Vector2 vector2Value = (Vector2)value;
                vector2Value = EditorGUILayout.Vector2Field( field.Name, vector2Value );
                field.SetValue( component, vector2Value );
            }
            else if( fieldType == typeof( Vector3 ) )
            {
                Vector3 vector3Value = (Vector3)value;
                vector3Value = EditorGUILayout.Vector3Field( field.Name, vector3Value );
                field.SetValue( component, vector3Value );
            }
            else if( fieldType == typeof( Vector4 ) )
            {
                Vector4 vector4Value = (Vector4)value;
                vector4Value = EditorGUILayout.Vector4Field( field.Name, vector4Value );
                field.SetValue( component, vector4Value );
            }
            else if( typeof( IList ).IsAssignableFrom( fieldType ) )
            {
                DrawListField( field, value, component );
            }
            else if( typeof( UnityEngine.Object ).IsAssignableFrom( fieldType ) )
            {
                UnityEngine.Object objValue = (UnityEngine.Object)value;
                objValue = EditorGUILayout.ObjectField( field.Name, objValue, fieldType, true );
                field.SetValue( component, objValue );
            }
            else
            {
                EditorGUILayout.LabelField( field.Name, "Unsupported field type" );
            }
        }

        private void DrawListField( FieldInfo field, object value, object component )
        {
            IList list = (IList)value;
            EditorGUILayout.LabelField( field.Name );
            EditorGUI.indentLevel++;
            for( int i = 0; i < list.Count; i++ )
            {
                object element = list[ i ];
                EditorGUILayout.BeginHorizontal();
                DrawField( element.GetType().GetField( "Value" ), element, component );
                if( GUILayout.Button( "Remove" ) )
                {
                    list.RemoveAt( i );
                }
                EditorGUILayout.EndHorizontal();
            }
            if( GUILayout.Button( "Add" ) )
            {
                list.Add( Activator.CreateInstance( field.FieldType.GetGenericArguments()[ 0 ] ) );
            }
            EditorGUI.indentLevel--;
            field.SetValue( component, list );
        }

        private void RemoveEntry( int index )
        {
            mainEntriesProperty.DeleteArrayElementAtIndex( index );
            serializedBlackboardData.ApplyModifiedProperties();
            EditorUtility.SetDirty( blackboardData );
            AssetDatabase.SaveAssets();
        }

        private void GenerateScript()
        {
            if( string.IsNullOrEmpty( blackboardData.Classname ) )
                return;

            string path = EditorUtility.SaveFilePanel( "Save Blackboard Script", "", $"{blackboardData.Classname}.cs", "cs" );

            if( string.IsNullOrEmpty( path ) )
                return;

            classGenerator.GenerateScript( path, blackboardData );
        }


        private void LoadScript( string path )
        {
            string scriptContent = File.ReadAllText( path );
            string classNamePattern = @"public\s+class\s+(\w+)";
            string namespacePattern = @"namespace\s+([\w\.]+)";
            string fieldPattern = @"(\[SerializeField\]\s*)?(public|private|protected)\s+(int|float|string|bool|GameObject|List<\w+>)\s+(\w+)\s*(?:=\s*([^;]+))?;";
            string autoPropertyPattern = @"(?<scope>public|private|protected|internal)\s+(?<type>\w+)\s+(?<name>\w+)\s*\{\s*(?<getter>get\s*;)\s*(?<setter>(private|protected|internal|public)?\s*set\s*;)\s*\}\s*(=\s*(?<defaultValue>[^;]+);)?";
            string accessorPropertyPattern = @"public\s+\w+\s+(\w+)\s*=>\s*\w+\s*;";

            Match classNameMatch = Regex.Match( scriptContent, classNamePattern );
            Match namespaceMatch = Regex.Match( scriptContent, namespacePattern );

            if( classNameMatch.Success )
            {
                classNameProperty.stringValue = classNameMatch.Groups[ 1 ].Value;
            }

            if( namespaceMatch.Success )
            {
                namespaceProperty.stringValue = namespaceMatch.Groups[ 1 ].Value;
            }

            ClearEntries();

            // First, ignore accessor properties
            HashSet<string> ignoredProperties = new HashSet<string>();
            foreach( Match match in Regex.Matches( scriptContent, accessorPropertyPattern ) )
            {
                string name = match.Groups[ 1 ].Value;
                ignoredProperties.Add( name );
            }

            // Match fields
            foreach( Match match in Regex.Matches( scriptContent, fieldPattern ) )
            {
                string serializeField = match.Groups[ 1 ].Value;
                string scope = match.Groups[ 2 ].Value;
                string type = match.Groups[ 3 ].Value;
                string name = match.Groups[ 4 ].Value;
                string defaultValue = match.Groups[ 5 ].Success ? match.Groups[ 5 ].Value.Trim() : string.Empty;

                if( !ignoredProperties.Contains( name ) )
                {
                    AddEntryFromScript( serializeField, scope, type, name, defaultValue );
                }
            }

            // Match auto-properties with default values
            foreach( Match match in Regex.Matches( scriptContent, autoPropertyPattern ) )
            {
                string scope = match.Groups[ "scope" ].Value;
                string type = match.Groups[ "type" ].Value;
                string name = match.Groups[ "name" ].Value;

                string defaultValue = match.Groups[ "defaultValue" ].Success ? 
                    match.Groups[ "defaultValue" ].Value.Trim() : string.Empty;

                if( !ignoredProperties.Contains( name ) )
                {
                    AddEntryFromScript( string.Empty, scope, type, name, defaultValue );
                }
            }

            serializedBlackboardData.ApplyModifiedProperties();
            Repaint(); // Ensure the UI is updated
        }

        private void ClearEntries()
        {
            if( mainEntriesProperty != null )
            {
                while( mainEntriesProperty.arraySize > 0 )
                {
                    mainEntriesProperty.DeleteArrayElementAtIndex( mainEntriesProperty.arraySize - 1 );
                }

                serializedBlackboardData.ApplyModifiedProperties();
                EditorUtility.SetDirty( blackboardData );
                AssetDatabase.SaveAssets();
            }
        }

        private void AddEntryFromScript(string serializeField, string scope, string type, string name, string defaultValue)
        {
            serializedBlackboardData.Update();
            mainEntriesProperty.arraySize++;
            var newEntry = mainEntriesProperty.GetArrayElementAtIndex(mainEntriesProperty.arraySize - 1);

            newEntry.FindPropertyRelative("Name").stringValue = name;
            newEntry.FindPropertyRelative("Scope").enumValueIndex = scope == "public" ? (int)FieldScope.Public : (int)FieldScope.Private;
            newEntry.FindPropertyRelative("IsSerialized").boolValue = !string.IsNullOrEmpty(serializeField);

            switch (type)
            {
                case "int":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.Int;
                    int intValue = int.TryParse(defaultValue, out int parsedInt) ? parsedInt : 0;
                    newEntry.FindPropertyRelative("Value").stringValue = EditorHelper.Instance.DrawIntField("Value", intValue).ToString();
                    break;

                case "float":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.Float;
                    float floatValue = float.TryParse(defaultValue.Replace("f", "").Replace("F", ""), out float parsedFloat) ? parsedFloat : 0f;
                    newEntry.FindPropertyRelative("Value").stringValue = EditorHelper.Instance.DrawFloatField("Value", floatValue).ToString();
                    break;

                case "string":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.String;
                    string stringValue = EditorHelper.Instance.DrawTextField("Value", defaultValue?.Trim('"') ?? string.Empty);
                    newEntry.FindPropertyRelative("Value").stringValue = stringValue;
                    break;

                case "bool":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.Bool;
                    bool boolValue = bool.TryParse(defaultValue, out bool parsedBool) && parsedBool;
                    newEntry.FindPropertyRelative("Value").stringValue = EditorHelper.Instance.DrawToggleField("Value", boolValue).ToString().ToLower();
                    break;

                case "GameObject":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.GameObject;
                    GameObject gameObjectValue = AssetDatabase.LoadAssetAtPath<GameObject>(defaultValue);
                    gameObjectValue = EditorHelper.Instance.DrawObjectField("Value", gameObjectValue);
                    newEntry.FindPropertyRelative("Value").stringValue = AssetDatabase.GetAssetPath(gameObjectValue);
                    break;

                case "Vector2":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.Vector2;
                    Vector2 vec2 = ParseVector2(defaultValue, Vector2.zero);
                    vec2 = EditorHelper.Instance.DrawVector2Field("Value", vec2);
                    newEntry.FindPropertyRelative("Value").stringValue = $"{vec2.x},{vec2.y}";
                    break;

                case "Vector3":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.Vector3;
                    Vector3 vec3 = ParseVector3(defaultValue, Vector3.zero);
                    vec3 = EditorHelper.Instance.DrawVector3Field("Value", vec3);
                    newEntry.FindPropertyRelative("Value").stringValue = $"{vec3.x},{vec3.y},{vec3.z}";
                    break;

                case "Vector4":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.Vector4;
                    Vector4 vec4 = ParseVector4(defaultValue, Vector4.zero);
                    vec4 = EditorHelper.Instance.DrawVector4Field("Value", vec4);
                    newEntry.FindPropertyRelative("Value").stringValue = $"{vec4.x},{vec4.y},{vec4.z},{vec4.w}";
                    break;

                case "Color":
                    newEntry.FindPropertyRelative("Type").enumValueIndex = (int)DataType.Color;
                    Color color = ParseColor(defaultValue, Color.white);
                    color = EditorHelper.Instance.DrawColorField("Value", color);
                    newEntry.FindPropertyRelative("Value").stringValue = $"{color.r},{color.g},{color.b},{color.a}";
                    break;

                default:
                    Debug.LogWarning($"Unsupported data type: {type}");
                    break;
            }

            serializedBlackboardData.ApplyModifiedProperties();
            EditorUtility.SetDirty(blackboardData);
            AssetDatabase.SaveAssets();
            Repaint(); // Ensure the UI is updated
        }

        private Vector2 ParseVector2(string value, Vector2 defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;

            // Remove "new Vector2(" and ")" from the string
            value = value.Replace("new Vector2(", "").Replace(")", "");

            string[] parts = value.Split(',');

            float x, y;

            float.TryParse(parts[0].Replace('f', ' ').Trim(), out x);
            float.TryParse(parts[1].Replace('f', ' ').Trim(), out y);

            return new Vector2(x, y);
        }

        private Vector3 ParseVector3(string value, Vector3 defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;

            // Remove "new Vector3(" and ")" from the string
            value = value.Replace("new Vector3(", "").Replace(")", "");

            string[] parts = value.Split(',');

            float x, y, z;

            float.TryParse(parts[0].Replace('f', ' ').Trim(), out x);
            float.TryParse(parts[1].Replace('f', ' ').Trim(), out y);
            float.TryParse(parts[2].Replace('f', ' ').Trim(), out z);

            return new Vector3(x, y, z);
        }

        private Vector4 ParseVector4(string value, Vector4 defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;

            // Remove "new Vector4(" and ")" from the string
            value = value.Replace("new Vector4(", "").Replace(")", "");

            string[] parts = value.Split(',');

            float x, y, z, w;

            float.TryParse(parts[0].Replace('f', ' ').Trim(), out x);
            float.TryParse(parts[1].Replace('f', ' ').Trim(), out y);
            float.TryParse(parts[2].Replace('f', ' ').Trim(), out z);
            float.TryParse(parts[3].Replace('f', ' ').Trim(), out w);

            return new Vector4(x, y, z, w);
        }

        private Color ParseColor(string value, Color defaultValue)
        {
            if (string.IsNullOrEmpty(value)) return defaultValue;

            // Remove "new Color(" and ")" from the string
            value = value.Replace("new Color(", "").Replace(")", "");

            string[] parts = value.Split(',');

            float r, g, b, a;

            float.TryParse(parts[0].Replace('f', ' ').Trim(), out r);
            float.TryParse(parts[1].Replace('f', ' ').Trim(), out g);
            float.TryParse(parts[2].Replace('f', ' ').Trim(), out b);
            float.TryParse(parts[3].Replace('f', ' ').Trim(), out a);

            return new Color(r, g, b, a);
        }



    }

}