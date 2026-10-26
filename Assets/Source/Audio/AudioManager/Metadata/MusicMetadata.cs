using UnityEngine;

namespace BZApp.Audio
{
    [CreateAssetMenu(fileName = "NewMusicData", menuName = "Audio/Audio Manager/New Music Metadata")]
    public class MusicMetadata : ScriptableObject
    {
        [Header("Metadata")]
        public string SongName;
        public string[] ArtistNames;
        public string AlbumName;
        public Sprite AlbumArt;

        [Header("Reference")]
        public AudioClip SongClip;
    }
}