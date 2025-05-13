using FrameLabs.AI.Blackboard;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities
{
    public sealed class EditorHelper
    {
        public EditorHelper() { }

        private static Lazy<EditorHelper> instance = new();

        public static EditorHelper Instance
        {
            get
            {
                return instance.Value;
            }
        }

        public string LastSelectedObject { get; set; }

        public void DrawList<T>( List<T> list, Action<T, int> drawItem, Action<int> removeItem )
        {
            for( int i = 0; i < list.Count; i++ )
            {
                EditorGUILayout.BeginVertical( "box" );

                drawItem( list[ i ], i );

                if( GUILayout.Button( "Remove Item" ) )
                {
                    removeItem( i );
                }

                EditorGUILayout.EndVertical();
            }
        }

        public bool DrawButton(string label)
        {
            return GUILayout.Button( label );
        }

        public void DrawPropertyField(SerializedProperty property, string label) 
        {
            EditorGUILayout.PropertyField(property, new GUIContent(label));
        }

        public string DrawTextField(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            value = EditorGUILayout.TextField(value);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public GameObject DrawObjectField( string label, GameObject value )
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField( label );
            value = (GameObject)EditorGUILayout.ObjectField( value, typeof( GameObject ), true );
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public void DrawErrorMessage(bool showError, string errorMessage)
        {
            if (showError)
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }
        }

        public void DrawSection(string label, Action drawContent)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            drawContent.Invoke();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        public void DrawEntryValueField<T>(SerializedProperty valueProperty, string label)
        {
            string valueString = valueProperty.stringValue;

            if (typeof(T) == typeof(int))
            {
                int.TryParse(valueString, out int intValue);
                intValue = DrawIntField(label, intValue);
                valueProperty.stringValue = intValue.ToString();
            }
            else if (typeof(T) == typeof(float))
            {
                float.TryParse(valueString, out float floatValue);
                floatValue = DrawFloatField(label, floatValue);
                valueProperty.stringValue = floatValue.ToString();
            }
            else if (typeof(T) == typeof(string))
            {
                valueProperty.stringValue = DrawTextField(label, valueProperty.stringValue);
            }
            else if (typeof(T) == typeof(bool))
            {
                bool.TryParse(valueString, out bool boolValue);
                boolValue = DrawToggleField(label, boolValue);
                valueProperty.stringValue = boolValue.ToString().ToLower();
            }
            else if (typeof(T) == typeof(GameObject))
            {
                GameObject currentObject = null;
                if (!string.IsNullOrEmpty(valueString))
                {
                    currentObject = AssetDatabase.LoadAssetAtPath<GameObject>(valueString);
                }

                GameObject selectedObject = DrawObjectField(label, currentObject);

                if (selectedObject != null)
                {
                    string path = AssetDatabase.GetAssetPath(selectedObject);
                    valueProperty.stringValue = path;
                }
                else
                {
                    valueProperty.stringValue = string.Empty;
                }
            }
            else if (typeof(T) == typeof(Vector2))
            {
                Vector2 vec2 = Vector2.zero;
                if (!string.IsNullOrEmpty(valueString))
                {
                    string[] parts = valueString.Split(',');
                    if (parts.Length == 2 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y))
                    {
                        vec2 = new Vector2(x, y);
                    }
                }

                vec2 = DrawVector2Field(label, vec2);
                valueProperty.stringValue = $"{vec2.x},{vec2.y}";
            }
            else if (typeof(T) == typeof(Vector3))
            {
                Vector3 vec3 = Vector3.zero;
                if (!string.IsNullOrEmpty(valueString))
                {
                    string[] parts = valueString.Split(',');
                    if (parts.Length == 3 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y) &&
                        float.TryParse(parts[2], out float z))
                    {
                        vec3 = new Vector3(x, y, z);
                    }
                }

                vec3 = DrawVector3Field(label, vec3);
                valueProperty.stringValue = $"{vec3.x},{vec3.y},{vec3.z}";
            }
            else if (typeof(T) == typeof(Vector4))
            {
                Vector4 vec4 = Vector4.zero;
                if (!string.IsNullOrEmpty(valueString))
                {
                    string[] parts = valueString.Split(',');
                    if (parts.Length == 4 &&
                        float.TryParse(parts[0], out float x) &&
                        float.TryParse(parts[1], out float y) &&
                        float.TryParse(parts[2], out float z) &&
                        float.TryParse(parts[3], out float w))
                    {
                        vec4 = new Vector4(x, y, z, w);
                    }
                }

                vec4 = DrawVector4Field(label, vec4);
                valueProperty.stringValue = $"{vec4.x},{vec4.y},{vec4.z},{vec4.w}";
            }
            else if (typeof(T) == typeof(Color))
            {
                Color color = Color.white;
                if (!string.IsNullOrEmpty(valueString))
                {
                    string[] parts = valueString.Split(',');
                    if (parts.Length == 4 &&
                        float.TryParse(parts[0], out float r) &&
                        float.TryParse(parts[1], out float g) &&
                        float.TryParse(parts[2], out float b) &&
                        float.TryParse(parts[3], out float a))
                    {
                        color = new Color(r, g, b, a);
                    }
                }

                color = DrawColorField(label, color);
                valueProperty.stringValue = $"{color.r},{color.g},{color.b},{color.a}";
            }
        }


        public void DrawListField<T>( SerializedProperty listProperty, Func<T> drawElement )
        {
            for( int i = 0; i < listProperty.arraySize; i++ )
            {
                SerializedProperty element = listProperty.GetArrayElementAtIndex( i );

                EditorGUILayout.BeginHorizontal();
                drawElement.Invoke();

                if( GUILayout.Button( "-" ) )
                {
                    listProperty.DeleteArrayElementAtIndex( i );
                }
                EditorGUILayout.EndHorizontal();
            }

            if( GUILayout.Button( "+" ) )
            {
                listProperty.arraySize++;
                drawElement.Invoke();
            }
        }

        public void InitializeSerializedProperties(SerializedObject serializedObject, 
            string[] propertyNames, out Dictionary<string, SerializedProperty> properties)
        {
            properties = new Dictionary<string, SerializedProperty>();
            foreach (string propertyName in propertyNames)
            {
                properties[propertyName] = serializedObject.FindProperty(propertyName);
            }
        }



        public int DrawIntField(string label, int value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            value = EditorGUILayout.IntField(value);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public float DrawFloatField(string label, float value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            value = EditorGUILayout.FloatField(value);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public bool DrawToggleField(string label, bool value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            value = EditorGUILayout.Toggle(value);
            EditorGUILayout.EndHorizontal();
            return value;
        }

        public Enum DrawEnumPopup(string label, Enum selected)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            selected = EditorGUILayout.EnumPopup(selected);
            EditorGUILayout.EndHorizontal();
            return selected;
        }

        public void DrawLabelField(string label, string value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);
            EditorGUILayout.LabelField(value);
            EditorGUILayout.EndHorizontal();
        }

        public Vector2 DrawVector2Field(string label, Vector2 value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("X", GUILayout.Width(30));
            value.x = EditorGUILayout.FloatField(value.x, GUILayout.Width(60));
            EditorGUILayout.LabelField("Y", GUILayout.Width(30));
            value.y = EditorGUILayout.FloatField(value.y, GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();

            return value;
        }


        public Vector3 DrawVector3Field(string label, Vector3 value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("X", GUILayout.Width(15));
            value.x = EditorGUILayout.FloatField(value.x, GUILayout.Width(50));
            EditorGUILayout.LabelField("Y", GUILayout.Width(15));
            value.y = EditorGUILayout.FloatField(value.y, GUILayout.Width(50));
            EditorGUILayout.LabelField("Z", GUILayout.Width(15));
            value.z = EditorGUILayout.FloatField(value.z, GUILayout.Width(50));

            EditorGUILayout.EndHorizontal();

            return value;
        }


        public Vector4 DrawVector4Field(string label, Vector4 value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(label);
            GUILayout.FlexibleSpace();

            EditorGUILayout.LabelField("X", GUILayout.Width(15));
            value.x = EditorGUILayout.FloatField(value.x, GUILayout.Width(50));
            EditorGUILayout.LabelField("Y", GUILayout.Width(15));
            value.y = EditorGUILayout.FloatField(value.y, GUILayout.Width(50));
            EditorGUILayout.LabelField("Z", GUILayout.Width(15));
            value.z = EditorGUILayout.FloatField(value.z, GUILayout.Width(50));
            EditorGUILayout.LabelField("W", GUILayout.Width(15));
            value.w = EditorGUILayout.FloatField(value.w, GUILayout.Width(50));

            EditorGUILayout.EndHorizontal();

            return value;
        }


        public Color DrawColorField(string label, Color value)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label);

            value = EditorGUILayout.ColorField(value);

            EditorGUILayout.EndHorizontal();

            return value;
        }

    }
}
