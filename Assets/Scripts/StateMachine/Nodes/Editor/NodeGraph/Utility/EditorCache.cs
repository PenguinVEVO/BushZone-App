using System;
using System.Collections.Generic;
using UnityEngine;


namespace FrameLabs.Utilities.NodeEditor
{
    [Serializable]
    public class TabEntry
    {
        public string Name;
        public NodeGraphData GraphData;
    }

    public class EditorCache : ScriptableObject
    {
        [ReadOnlyField] public List<string> TabNames = new();
        [ReadOnlyField] public string ActiveTabName;
        [ReadOnlyField] public List<TabEntry> TabData = new();
        [ReadOnlyField] public List<string> TabHistory = new();
    }   
}