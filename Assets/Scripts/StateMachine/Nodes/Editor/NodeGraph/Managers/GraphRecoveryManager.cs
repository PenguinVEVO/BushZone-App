using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor
{
    public static class GraphRecoveryManager
    {
        private const string RecoveryFolder = "Assets/Cache";

        /// <summary>
        /// Save a recovery snapshot to disk as a .asset file using GraphWriter.
        /// </summary>
        public static void SaveRecoverySnapshot(string name, NodeGraphData data)
        {
            if (string.IsNullOrWhiteSpace(name) || data == null)
            {
                Debug.LogWarning("[GraphRecoveryManager] Invalid recovery name or data.");
                return;
            }

            string recoveryPath = Path.Combine(RecoveryFolder, $"{name}_recovery.asset").Replace("\\", "/");

            if (GraphWriter.Instance.WriteGraph(data, recoveryPath))
            {
                Debug.Log($"[GraphRecoveryManager] Recovery snapshot saved to: {recoveryPath}");
            }
            else
            {
                Debug.LogError($"[GraphRecoveryManager] Failed to save recovery snapshot for {name}");
            }
        }

        /// <summary>
        /// Loads all available recovery graphs from disk and returns them.
        /// </summary>
        public static Dictionary<string, NodeGraphData> LoadRecovery()
        {
            Dictionary<string, NodeGraphData> recoveredGraphs = new();

            if (!Directory.Exists(RecoveryFolder))
                return recoveredGraphs;

            string[] recoveryFiles = Directory.GetFiles(RecoveryFolder, "*_recovery.asset", SearchOption.TopDirectoryOnly);
            foreach (var filePath in recoveryFiles)
            {
                string assetPath = filePath.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                NodeGraphData data = AssetDatabase.LoadAssetAtPath<NodeGraphData>(assetPath);

                if (data != null)
                {
                    string tabName = Path.GetFileNameWithoutExtension(filePath).Replace("_recovery", "");
                    recoveredGraphs[tabName] = data;

                    Debug.Log($"[GraphRecoveryManager] Loaded recovery graph from: {assetPath}");
                }
            }

            return recoveredGraphs;
        }

        /// <summary>
        /// Call after recovery confirmed to clean up old recovery files.
        /// </summary>
        public static void ClearRecovery()
        {
            if (!Directory.Exists(RecoveryFolder))
                return;

            string[] files = Directory.GetFiles(RecoveryFolder, "*_recovery.asset");
            foreach (string file in files)
            {
                string assetPath = file.Replace(Application.dataPath, "Assets").Replace("\\", "/");
                AssetDatabase.DeleteAsset(assetPath);
            }

            Debug.Log("[GraphRecoveryManager] Cleared all recovery snapshots.");
        }
    }
}
