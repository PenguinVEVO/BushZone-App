using UnityEngine;

namespace BZApp.GUI.Utilities
{
    public static class ExtColour
    {
        public static Color ClearWhite
        {
            get
            {
                Color result = Color.white;
                result.a = 0;
                return result;
            }
        }
    }
}