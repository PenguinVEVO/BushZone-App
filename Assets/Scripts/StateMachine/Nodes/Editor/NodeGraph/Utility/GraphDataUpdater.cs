using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    [InitializeOnLoad]
    public static class GraphDataUpdater
    {
        private static double updateInterval = 4.0;
        private static double nextUpdateTime;
        
        private static Queue<string> dirtyTabQueue = new();
        private static readonly HashSet<string> enqueuedTabs = new();
        private static readonly Dictionary<string, int> lastTabHashes = new();

        static GraphDataUpdater()
        {
            EditorApplication.update += Update;
            nextUpdateTime = EditorApplication.timeSinceStartup + updateInterval;
        }

        public static void ForceUpdateAllDirtyTabs()
        {
            foreach (var tabName in GraphChangeNotifier.Instance.GetDirtyTabs())
            {
                UpdateTab(tabName);
            }
        }

        private static void Update()
        {
            if( !NodeGraphEditor.IsAutoSave )
                return;

            if (EditorApplication.timeSinceStartup < nextUpdateTime)
                return;

            nextUpdateTime = EditorApplication.timeSinceStartup + updateInterval;

            var dirtyTabs = GraphChangeNotifier.Instance.GetDirtyTabs();

            foreach (var tab in dirtyTabs)
            {
                if (enqueuedTabs.Add(tab))
                    dirtyTabQueue.Enqueue(tab);
            }

            int batchSize = 2;
            while (batchSize-- > 0 && dirtyTabQueue.Count > 0)
            {
                string tabName = dirtyTabQueue.Dequeue();
                UpdateTab(tabName);
            }
        }

        private static void UpdateTab(string tabName)
        {
            var tab = TabManager.Instance.GetTabByName(tabName);
            if (tab == null || tab.GraphView == null)
                return;

            var data = tab.SerializeGraphData();
            int currentHash = tab.GetLastSerializedHash();

            if (lastTabHashes.TryGetValue(tabName, out int lastHash) && currentHash == lastHash)
                return;

            lastTabHashes[tabName] = currentHash;
            GraphSessionManager.Instance.RegisterTab(tabName, data);

            Debug.Log($"[GraphDataUpdater] Updated graph data for tab: {tabName}");
        }
    }
}
