using System.Collections.Generic;
using UnityEngine;

namespace BZApp.Audio
{
    [CreateAssetMenu(fileName = "NewMusicLibrary", menuName = "Audio/Audio Manager/New Music Library")]
    public class MusicLibrary : ScriptableObject
    {
        // Configuration
        [SerializeField] private MusicMetadata[] tracks;

        // Exposed properties
        public MusicMetadata[] TracksById => tracks;
        public Dictionary<string, int> TracksByName;

        /// <summary>
        /// Initialise the music metadata dictionary for use.
        /// </summary>
        public void InitialiseDictionary()
        {
            if (TracksByName != null) return;
            TracksByName = new Dictionary<string, int>();
            for (int i = 0; i < tracks.Length; i++)
                TracksByName.Add(tracks[i].SongName, i);
        }
    }
}