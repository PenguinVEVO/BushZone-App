using FrameLabs.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class NodeGraphData : ScriptableObject
{
   [ReadOnlyField] public string EditorVersion = "1.0";
    public List<NodeData> nodes = new List<NodeData>();
    public List<EdgeData> edges = new List<EdgeData>();
    public List<VisualElementData> visualElements = new();
    public Vector3 ScrollOffset;
    public float ZoomScale;
}

[Serializable]
public class VisualElementData
{
    public string Id;
    public string ElementType;
    public Vector2 position;
    public bool inputIsLeft = true;
}

[Serializable]
public class EdgeData
{
    public string outputNodeId;
    public string outputPortName;
    public string inputNodeId;
    public string inputPortName;
    public string outputPinId;
    public string inputPinId;
}

[Serializable]
public abstract class Property
{
    public string Key;
}

[Serializable]
public class Property<T>: Property
{
    public T Value;

    public Property(string key, T value)
    {
        Key = key;
        Value = value;
    }
}

[Serializable]
public class BoolProperty : Property<bool>
{
    public BoolProperty(string key, bool value) : base(key, value) { }
}

[Serializable]
public class FloatProperty : Property<float>
{
    public FloatProperty(string key, float value) : base(key, value) { }
}

[Serializable]
public class IntProperty : Property<int>
{
    public IntProperty(string key, int value) : base(key, value) { }
}

[Serializable]
public class ScriptableObjectProperty : Property<ScriptableObject>
{
    public ScriptableObjectProperty(string key, ScriptableObject value) : base(key, value) { }
}

[Serializable]
public class NodeData : ISerializationCallbackReceiver
{
    public bool executeOnce;
    public string id;
    public int priority;
    public string nodeType;
    public string name;
    public Vector2 position;

    // lists to store properties for Unity serialization
    [SerializeField][HideInInspector] private List<string> boolKeys = new List<string>();
    [SerializeField][HideInInspector] private List<bool> boolValues = new List<bool>();

    [SerializeField][HideInInspector] private List<string> floatKeys = new List<string>();
    [SerializeField][HideInInspector] private List<float> floatValues = new List<float>();

    [SerializeField][HideInInspector] private List<string> intKeys = new List<string>();
    [SerializeField][HideInInspector] private List<int> intValues = new List<int>();
  
    [SerializeField][HideInInspector] private List<string> scriptableObjectKeys = new List<string>();
    [SerializeField][HideInInspector] private List<ScriptableObject> scriptableObjectValues = new List<ScriptableObject>();


    [NonSerialized] public List<Property> properties = new List<Property>();
   
    // Called before Unity serializes the object
    public void OnBeforeSerialize()
    {
        boolKeys.Clear(); boolValues.Clear();
        floatKeys.Clear(); floatValues.Clear();
        intKeys.Clear();
        scriptableObjectKeys.Clear(); scriptableObjectValues.Clear();

        foreach (var property in properties)
        {
            switch (property)
            {
                case BoolProperty boolProp:
                    boolKeys.Add(boolProp.Key);
                    boolValues.Add(boolProp.Value);
                    break;

                case IntProperty intProp:
                    intKeys.Add(intProp.Key);
                    intValues.Add(intProp.Value);
                    break;

                case FloatProperty floatProp:
                    floatKeys.Add(floatProp.Key);
                    floatValues.Add(floatProp.Value);
                    break;

                case ScriptableObjectProperty soProp:
                    scriptableObjectKeys.Add(soProp.Key);
                    scriptableObjectValues.Add(soProp.Value);
                    break;
            }
        }
    }

    // Called after Unity deserializes the object
    public void OnAfterDeserialize()
    {
        properties.Clear();

        for( int i = 0; i < intKeys.Count; i++ )
            properties.Add( new IntProperty( intKeys[ i ], intValues[ i ] ) );

        for( int i = 0; i < boolKeys.Count; i++)
            properties.Add(new BoolProperty(boolKeys[i], boolValues[i]));

        for (int i = 0; i < floatKeys.Count; i++)
            properties.Add(new FloatProperty(floatKeys[i], floatValues[i]));

        for (int i = 0; i < scriptableObjectKeys.Count; i++)
            properties.Add(new ScriptableObjectProperty(scriptableObjectKeys[i], scriptableObjectValues[i]));
    }

    public void AddProperty(Property property)
    {
        properties.RemoveAll(p => p.Key == property.Key);
        properties.Add(property);
    }

    public T GetProperty<T>(string key)
    {
        var prop = properties.OfType<Property<T>>().FirstOrDefault(p => p.Key == key);
        return prop != null ? prop.Value : default;
    }
}




