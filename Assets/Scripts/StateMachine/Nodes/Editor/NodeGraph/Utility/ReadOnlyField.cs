using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities
{
    public class ReadOnlyField: PropertyAttribute { }

    [CustomPropertyDrawer( typeof( ReadOnlyField ) )]
    public class ReadOnly : PropertyDrawer
    {
        public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
        {
            GUI.enabled = false;
            EditorGUI.PropertyField( position, property, label );
            GUI.enabled = true; 
        }
    }

}
