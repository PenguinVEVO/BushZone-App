using UnityEngine;

namespace BZApp.Systems.Audio
{
    [CreateAssetMenu(fileName = "NewMusicData", menuName = "Audio/Music Data")]
    public class MusicData : ScriptableObject
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