using System.Collections.Generic;
using UnityEngine;

namespace BZApp.Audio
{
    [CreateAssetMenu(fileName = "NewClipLibrary", menuName = "Audio/Audio Manager/New Clip Library")]
    public class ClipLibrary : ScriptableObject
    {
        // Configuration
        [SerializeField] private AudioClip[] clips;

        // Exposed properties
        public AudioClip[] ClipsById => clips;
        public Dictionary<string, int> ClipsByName;

        /// <summary>
        /// Initialise the clips dictionary for use.
        /// </summary>
        public void InitialiseDictionary()
        {
            if (ClipsByName != null) return;
            ClipsByName = new Dictionary<string, int>();
            for (int i = 0; i < clips.Length; i++)
                ClipsByName.Add(clips[i].name, i);
        }
    }
}