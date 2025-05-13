using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/*
 * Name: Utility.cs
 * Date: 2/07/2024
 * Modified: 26/07/2024
 */
namespace FrameLabs.Utilities
{
    /// <summary>
    /// A wrapper class for a generic list to facilitate JSON serialization and deserialization.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    [Serializable]
    public class Listwrapper<T>
    {
        /// <summary>
        /// The list of elements to be serialized or deserialized.
        /// </summary>
        public List<T> list = new List<T>();
    }

    /// <summary>
    /// A class containing useful utility methods.
    /// </summary>
    public sealed class Utility
    {
        public Utility() { }

        private static readonly Lazy<Utility> instance = new Lazy<Utility>();
        public static Utility Instance
        {
            get
            {
                return instance.Value;
            }
        }

        /// <summary>
        /// Compares two unique identifiers to determine if they are equal.
        /// </summary>
        /// <param name="id1">The first unique identifier.</param>
        /// <param name="id2">The second unique identifier.</param>
        /// <returns>Returns true if the two unique identifiers match; otherwise, false.</returns>
        public bool CompareID( string id1, string id2 )
        {
            bool result = false;

            Guid guid1 = default;
            Guid guid2 = default;

            if( guid1 != null && guid2 != null )
            {
                Guid.TryParse( id1, out guid1 );
                Guid.TryParse( id2, out guid2 );

                result = guid1.Equals( guid2 );
            }

            return result;
        }

        /// <summary>
        /// Gets all serializable components attached to the given GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject to retrieve serializable components from.</param>
        /// <returns>A list of serializable components attached to the GameObject.</returns>
        public List<Component> GetSerializableComponents( GameObject gameObject )
        {
            List<Component> serializableComponents = new List<Component>();

            // Retrieve all components attached to the GameObject.
            Component[] allComponents = gameObject.GetComponents<Component>();

            // Iterate through each component and check if it is serializable.
            foreach( Component component in allComponents )
            {
                if( IsSerializable( component ) )
                {
                    serializableComponents.Add( component );
                }
            }

            return serializableComponents;
        }

        /// <summary>
        /// Determines whether the specified component is serializable.
        /// </summary>
        /// <param name="component">The component to check for serializability.</param>
        /// <returns>True if the component is serializable; otherwise, false.</returns>
        public bool IsSerializable( Component component )
        {
            Type type = component.GetType();

            // Check if the type is marked as serializable or has the [Serializable] attribute.
            return type.IsSerializable || type.GetCustomAttributes( typeof( SerializableAttribute ), true ).Length > 0;
        }

        /// <summary>
        /// Converts the character at the specified index of the input string to uppercase.
        /// </summary>
        /// <param name="value">The input string from which the character will be converted.</param>
        /// <param name="charIndex">The index of the character in the string to convert to uppercase.</param>
        /// <returns>The uppercase character at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when charIndex is outside the bounds of the input string.</exception>
        public char CharacterToUppercase( string value, int charIndex )
        {
            // Ensure the charIndex is within the valid range of the string
            if( charIndex < 0 || charIndex >= value.Length )
            {
                throw new ArgumentOutOfRangeException( nameof( charIndex ), "Index is outside the bounds of the string." );
            }

            // Convert the character at the specified index to uppercase
            char result = char.ToUpper( value[ charIndex ] );

            return result;
        }

        /// <summary>
        /// Converts the character at the specified index of the input string to lowercase.
        /// </summary>
        /// <param name="value">The input string from which the character will be converted.</param>
        /// <param name="charIndex">The index of the character in the string to convert to lowercase.</param>
        /// <returns>The lowercase character at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when charIndex is outside the bounds of the input string.</exception>
        public char CharacterToLower( string value, int charIndex )
        {
            // Ensure the charIndex is within the valid range of the string
            if( charIndex < 0 || charIndex >= value.Length )
            {
                throw new ArgumentOutOfRangeException( nameof( charIndex ), "Index is outside the bounds of the string." );
            }

            // Convert the character at the specified index to lowercase
            char result = char.ToLower( value[ charIndex ] );

            return result;
        }

        /// <summary>
        /// Retrieves the names of all non-abstract subclasses of the specified type.
        /// </summary>
        /// <typeparam name="T">The base type to search for subclasses.</typeparam>
        /// <param name="qualifiedName">If true, returns fully qualified names (including namespace and assembly information); otherwise, returns only the type names.</param>
        /// <returns>An array of strings representing the names or fully qualified names of the subclasses of T.</returns>
        public string[] GetAssembly<T>( bool qualifiedName = false )
        {
            // Initialize the result array to null
            string[] result = null;

            // Get all non-abstract subclasses of the type T from the assembly
            var classTypes = Assembly.GetAssembly( typeof( T ) )
                              .GetTypes().Where( type => type.IsSubclassOf( typeof( T ) ) && !type.IsAbstract ).ToArray();

            // If qualifiedName is true, get the fully qualified names of the types
            // Otherwise, get just the names of the types
            if( qualifiedName )
                result = classTypes.Select( t => t.AssemblyQualifiedName ).ToArray();
            else
                result = classTypes.Select( t => t.Name ).ToArray();

            // Return the array of names or fully qualified names
            return result;
        }

        public float RoundToPixelGrid( float value, float scaleFactor = 1.0f )
        {
            return Mathf.Round( value * scaleFactor ) / scaleFactor;
        }

        public Vector3 RoundToPixelGrid( Vector3 vector, float scaleFactor = 1.0f )
        {
            return new Vector3(
                Mathf.Round( vector.x * scaleFactor ) / scaleFactor,
                Mathf.Round( vector.y * scaleFactor ) / scaleFactor,
                Mathf.Round( vector.z * scaleFactor ) / scaleFactor
            );
        }

        /// <summary>
        /// Searches for a specified file within a given directory and its subdirectories.
        /// </summary>
        /// <param name="path">The root directory to begin searching in.</param>
        /// <param name="fileName">The name of the file to find (including extension).</param>
        /// <returns>
        /// The relative path of the found file, replacing backslashes with forward slashes 
        /// and converting the full path to a Unity-friendly "Assets" path. 
        /// Returns an empty string if the file is not found.
        /// </returns>
        public string FindFileInPath( string path, string fileName )
        {
            string[] files = Directory.GetFiles( path, "*", SearchOption.AllDirectories );

            foreach( string file in files )
            {
                if( Path.GetFileName( file ) == fileName )
                {
                    return file.Replace( '\\', '/' ).Replace( Application.dataPath, "Assets" );
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Generates a Unique Identifier.
        /// </summary>
        /// <returns>Returns a new unique identifier as a string.</returns>
        public string GenerateUUID()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString();
        }
    }
}
