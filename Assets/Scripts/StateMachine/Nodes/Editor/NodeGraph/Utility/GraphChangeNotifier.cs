using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphChangeNotifier
    {
        private static readonly GraphChangeNotifier instance = new();
        public static GraphChangeNotifier Instance => instance;

        private readonly HashSet<string> dirtyTabs = new();

        private GraphChangeNotifier() { }

        public void MarkDirty(string tabName)
        {
            if (!string.IsNullOrEmpty(tabName) && dirtyTabs.Add(tabName))
            {
                Debug.Log($"[GraphChangeNotifier] Tab '{tabName}' marked as dirty.");
            }
        }

        public void ClearDirty(string tabName)
        {
            if (!string.IsNullOrEmpty(tabName) && dirtyTabs.Remove(tabName))
            {
                Debug.Log($"[GraphChangeNotifier] Tab '{tabName}' marked as clean.");
            }
        }

        public bool IsDirty(string tabName)
        {
            return !string.IsNullOrEmpty(tabName) && dirtyTabs.Contains(tabName);
        }

        public IEnumerable<string> GetDirtyTabs()
        {
            return dirtyTabs;
        }

        public void ClearAll()
        {
            dirtyTabs.Clear();
            Debug.Log("[GraphChangeNotifier] Cleared all dirty tabs.");
        }
    }
}
