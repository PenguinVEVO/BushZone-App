using UnityEngine;
using System.Collections.Generic;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public class CompilerRegistryAsset : ScriptableObject
    {
        public List<string> compilerTypeNames = new();
    }
}
