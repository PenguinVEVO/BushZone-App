using UnityEngine;

namespace Mitchel.Utilities
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