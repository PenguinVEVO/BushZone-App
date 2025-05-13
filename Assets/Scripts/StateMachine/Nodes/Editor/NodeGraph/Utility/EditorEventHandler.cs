using System;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace FrameLabs.Utilities.NodeEditor
{
    [InitializeOnLoad]
    public static class EditorEventHandler
    {
        private static bool hasRestored = false;

        static EditorEventHandler()
        {
            EditorApplication.update += TryRestore;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeDomainReload;
        }

        private static void OnBeforeDomainReload()
        {
            GraphDataUpdater.ForceUpdateAllDirtyTabs();
            GraphSessionManager.Instance.RecordSnapShot();
        }

        /// <summary>
        /// Handles Unity's play mode transitions to ensure graph snapshots are saved before entering play mode,
        /// and that the editor state is restored after domain reload or play mode ends.
        /// </summary>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    Debug.Log("[EditorPlayerEventHandler] Saving recovery snapshot before entering Play Mode.");
                    GraphDataUpdater.ForceUpdateAllDirtyTabs();
                    GraphSessionManager.Instance.RecordSnapShot();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    Debug.Log("[EditorPlayerEventHandler] Entered Play Mode.");
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    Debug.Log("[EditorPlayerEventHandler] Re-entered Edit Mode. Attempting to restore graph editor.");
                    hasRestored = false;
                    TryRestore();
                    break;
            }
        }

        /// <summary>
        /// Called during Editor initialization to restore the graph editor cache.
        /// </summary>
        private static void TryRestore()
        {
            if (hasRestored &&
                (EditorApplication.isCompiling ||
                 EditorApplication.isUpdating ||
                 EditorApplication.isPlayingOrWillChangePlaymode))
                return;

            RestoreEditorState();
        }

        /// <summary>
        /// Attempts to restore the NodeGraphEditor cache.
        /// </summary>
        private static void RestoreEditorState()
        {
            var window = Resources.FindObjectsOfTypeAll<NodeGraphEditor>().FirstOrDefault();

            if (window != null)
            {
                hasRestored = true;
                window.TryLoadCache();
                EditorApplication.update -= TryRestore;

                Debug.Log("[EditorPlayerEventHandler] Graph cache restored successfully.");
            }
        }
    }

}