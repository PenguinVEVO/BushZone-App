using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace FrameLabs.AI.Blackboard
{
    [Serializable]
    public class SerializableComponentWrapper
    {
        public string ComponentType;
        public string ComponentJson;
    }

    [Serializable]
    public class BlackboardEntry
    {
        public bool IsSerialized;
        public string Name;
        public FieldScope Scope;
        public DataType Type;
        public string Value;
    }

    public enum FieldScope
    {
        Public,
        Private,
    }

    public enum DataType
    {
        Int,
        Float,
        String,
        Bool,
        List,
        GameObject,
        Vector2,
        Vector3, 
        Vector4,
        Color
    }

    public class BlackboardData : ScriptableObject
    {
        public string Id { get; set; }

        public void SetValue<T>( string fieldName, T value )
        {
            if ( typeof(T) != value.GetType() ) 
            {
                Debug.LogWarning($"Type <{typeof(T)}>, is not value type::<{value.GetType()}>");
            }

            MemberInfo memberInfo = GetMemberInfo(fieldName);

            if (memberInfo != null)
            {
                if (memberInfo is FieldInfo field)
                {
                    field.SetValue(this, value);
                }
                else if (memberInfo is PropertyInfo property && property.CanWrite)
                {
                    property.SetValue(this, value);
                }
            }
        }

        public T GetValue<T>(string fieldName)
        {
            MemberInfo memberInfo = GetMemberInfo(fieldName);

            if (memberInfo != null)
            {
                if (memberInfo is FieldInfo field)
                {
                    return (T)field.GetValue(this);
                }
                else if (memberInfo is PropertyInfo property && property.CanRead)
                {
                    return (T)property.GetValue(this);
                }
            }

            return default;
        }


        private MemberInfo GetMemberInfo(string memberName)
        {
            Type type = GetType();

            MemberInfo member = type.GetField(memberName, BindingFlags.Public | BindingFlags.NonPublic | 
                BindingFlags.Instance) ?? (MemberInfo)type.GetProperty(memberName, BindingFlags.Public | 
                BindingFlags.NonPublic | BindingFlags.Instance);

            return member;
        }

        public virtual BlackboardData Clone()
        {
            BlackboardData clone = CreateInstance<BlackboardData>();
            clone.Id = Id;

            return clone;
        }
    }

    public class BlackboardDataEntry : ScriptableObject
    {
        public string Namespace;
        public string Classname;
        public List<BlackboardEntry> Entries = new List<BlackboardEntry>();
    }

}