using UnityEngine;

namespace BZApp.Utilities
{
    // Note about this class:
    // The main purpose is to provide a simple way to print things to the console for use with Unity Events, where
    // calling via script is not otherwise available.
    public class ConsoleLogger : MonoBehaviour
    {
        /// <summary>
        /// Print a debug/info log to the console.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        public void PrintLog(string message) => Debug.Log(message);

        /// <summary>
        /// Print a warning log to the console.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        public void PrintWarning(string message) => Debug.LogWarning(message);
        
        /// <summary>
        /// Print an error/fatal log to the console.
        /// </summary>
        /// <param name="message">The message to print to the console.</param>
        public void PrintError(string message) => Debug.LogError(message);
    }
}