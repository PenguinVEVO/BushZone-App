using UnityEngine;

namespace BZApp.Utilities
{
    // Note about this class:
    // The main purpose is to provide a simple way to print things to the console for use with Unity Events, where
    // calling via script is not otherwise available.
    public class ConsoleLogger : MonoBehaviour
    {
        public void PrintLog(string message) 
            => Debug.Log(message);
        
        public void PrintWarning(string message) 
            => Debug.LogWarning(message);
        
        public void PrintError(string message) 
            => Debug.LogError(message);
    }
}