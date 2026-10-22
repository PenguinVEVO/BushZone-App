//  Audio Manager
//  By PenguinVEVO
//  Created October 2025

using System;
using BZApp.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BZApp.Audio
{
    [AddComponentMenu("Audio/Audio Manager")]
    public class AudioManager : MonoBehaviour
    {
        // ======================[#]  CONFIGURATION  [#]======================

        [Header("Audio Library")]
        [SerializeField] private MusicLibrary musicLibrary;
        [SerializeField] private ClipLibrary sfxLibrary;

        [Header("General Settings")]
        [Tooltip("Automatically load the Audio Manager and associated objects into Don't Destroy On Load. " +
            "Note that this will cause conflictions if a scene-local Audio Manager exists.")]
        [SerializeField] private bool loadIntoDDOL = true;

        [Header("Voice Settings")]
        [SerializeField] private TransitionType musicTransitionType = TransitionType.FadeOut;
        [Tooltip("The curve for fading in music. Will be used when crossfading multiple music voices. " +
            "Make sure to only adjust the timing as the value must be 0-1.")]
        [SerializeField] private AnimationCurve musicFadeInCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Tooltip("The curve for fading out music. Will be used when either fading out to a new song on one music voice or crossfading multiple music voices. " +
            "Make sure to only adjust the timing as the value must be 0-1.")]
        [SerializeField] private AnimationCurve musicFadeOutCurve = AnimationCurve.Linear(0, 1, 1, 0);
        [Space(5)]
        [Tooltip("Note: a minimum of two SFX voices is required—one for general SFX and another for system sounds, such as error messages.")]
        [SerializeField, Range(2, 4)] private int sfxVoiceCount = 2;
        [SerializeField, Range(1, 2)] private int fallbackSfxVoice = 1;

        [Header("Debug Settings")]
        [SerializeField, Range(0, 1)] private float globalVolume = 1f;
        [SerializeField] private LogOptions logOptions;
        
        // ======================[#]  INTERNAL VALUES/REFERENCES  [#]====================== 
        
        // Internal values
        private float currentGlobalVolume;
        private enum TransitionType { FadeOut, Crossfade }

        // Object references
        private GameObject audioSourcesParent;
        
        // Value/reference collections
        private readonly Dictionary<string, int> musicClipDictionary = new();
        private readonly List<AudioSource> musicVoices = new();
        private readonly List<AudioSource> sfxVoices = new();
        
        // Coroutine references
        private Coroutine musicFadeCoroutine;
        
        // ======================[#]  EXPOSED PROPERTIES/EVENT HOOKS  [#]======================
        
        // Exposed properties
        public static AudioManager Instance { get; private set; }
        
        // Events
        public static event Action<MusicMetadata> OnMusicStarted;
        public static event Action OnMusicStopped;
        
        // ======================[#]  LIFECYCLE FUNCTIONS  [#]======================

        private void Awake()
        {
            // Initialising singleton
            if (Instance != null)
                Destroy(Instance);
            Instance = this;

            // Setting up the voices
            audioSourcesParent = new GameObject("Audio Manager Voices");
            for (int i = 0; i <= (int)musicTransitionType; i++)
            {
                GameObject newMusicVoiceObject = new($"Music Voice {i + 1}")
                {
                    transform = { parent = audioSourcesParent.transform }
                };
                AudioSource newMusicVoice = newMusicVoiceObject.AddComponent<AudioSource>();
                musicVoices.Add(newMusicVoice);
            }
            for (int i = 0; i <  sfxVoiceCount; i++)
            {
                GameObject newSfxVoiceObject = new($"SFX Voice {i + 1}")
                {
                    transform = { parent = audioSourcesParent.transform }
                };
                AudioSource newSfxVoice = newSfxVoiceObject.AddComponent<AudioSource>();
                sfxVoices.Add(newSfxVoice);
            }
            // Setting DDOL if enabled
            if (loadIntoDDOL)
            {
                gameObject.transform.parent = null;
                DontDestroyOnLoad(gameObject);
                DontDestroyOnLoad(audioSourcesParent);
                audioSourcesParent.transform.parent = gameObject.transform;
            }

            // Initialise sound ID dictionaries
            musicLibrary.InitialiseDictionary();
            sfxLibrary.InitialiseDictionary();
        }

        private void Start() => UpdateGlobalVolumes();

        private void Update()
        {
            // Updating the global volume of voices (for debugging)
            if (currentGlobalVolume != globalVolume) UpdateGlobalVolumes();
        }

        // ======================[#]  PUBLIC AUDIO MANAGER API  [#]======================

        /// <summary>
        /// Play a music track from the music track library.
        /// </summary>
        /// <param name="clipName">The name of the music track (used to get the ID).</param>
        public void PlayMusic(string clipName) => PlayMusic(musicClipDictionary[clipName]);

        /// <summary>
        /// Play a music track from the music track library.
        /// </summary>
        /// <param name="id">The ID of the music track.</param>
        public void PlayMusic(int id)
        {
            switch (musicTransitionType)
            {
                case TransitionType.FadeOut:
                    if (!musicVoices[^1].isPlaying)
                    {
                        musicVoices[^1].clip = musicLibrary.TracksById[id].SongClip;
                        musicVoices[0].volume = currentGlobalVolume;
                        musicVoices[^1].Play();
                        OnMusicStarted?.Invoke(musicLibrary.TracksById[id]);
                        if (logOptions.ShowInfoLogs) Debug.Log($"[INFO] Music clip with ID {id} now playing.");
                        return;
                    }
                    if (musicFadeCoroutine != null)
                        StopCoroutine(musicFadeCoroutine);
                    musicFadeCoroutine = StartCoroutine(SingleMusicVoiceFade(musicVoices[^1], musicLibrary.TracksById[id]));
                    if (logOptions.ShowInfoLogs) Debug.Log($"[INFO] Music clip with ID {id} now playing.");
                    break;
                case TransitionType.Crossfade:
                    // If no voices are playing anything
                    if (!musicVoices.Any(v => v.isPlaying))
                    {
                        musicVoices[0].clip = musicLibrary.TracksById[id].SongClip;
                        musicVoices[0].volume = currentGlobalVolume;
                        musicVoices[0].Play();
                        OnMusicStarted?.Invoke(musicLibrary.TracksById[id]);
                        return;
                    }

                    // If either one of the two voices are already playing music
                    AudioSource oldVoice = null, newVoice = null;
                    for (int i = 0; i < musicVoices.Count; i++)
                    {
                        if (musicVoices[i].isPlaying) oldVoice = musicVoices[i];
                        else newVoice = musicVoices[i];
                    }
                    if (musicFadeCoroutine != null)
                        StopCoroutine(musicFadeCoroutine);
                    musicFadeCoroutine = StartCoroutine(DoubleMusicVoiceCrossfade(oldVoice, newVoice, musicLibrary.TracksById[id]));
                    break;

            }
        }

        /// <summary>
        /// Play an SFX clip from the SFX clip library.
        /// </summary>
        /// <param name="id">The ID of the SFX clip.</param>
        public void PlaySFX(int id) => PlaySFX(id, false);
        
        /// <summary>
        /// Play an SFX clip from the SFX clip library.
        /// </summary>
        /// <param name="clipName">The name of the SFX clip (used to get the ID).</param>
        public void PlaySFX(string clipName) => PlaySFX(sfxLibrary.ClipsByName[clipName], false);

        /// <summary>
        /// Play an SFX clip from the SFX clip library.
        /// </summary>
        /// <param name="id">SFX clip id</param>
        /// <param name="overrideCurrentVoice">If the specified clip is playing on a voice already, override that voice</param>
        public void PlaySFX(int id, bool overrideCurrentVoice)
        {
            for (int i = 0; i < sfxVoices.Count - 1; i++)
            {
                switch (overrideCurrentVoice)
                {
                    case true:
                        if (sfxVoices[i].clip != sfxLibrary.ClipsById[id])
                        {
                            if (i != sfxVoices.Count - 1) continue;
                            // If it reaches this point, the clip isn't currently playing anywhere so just go to a free voice as normal
                            PlaySFX(id, false);
                            return;
                        }
                        sfxVoices[i].Stop();
                        sfxVoices[i].clip = sfxLibrary.ClipsById[id];
                        sfxVoices[i].Play();
                        if (logOptions.ShowInfoLogs) Debug.Log($"[INFO] Overriding clip now playing on SFX voice {i}.");
                        break;
                    case false:
                        if (sfxVoices[i].isPlaying) continue;
                        sfxVoices[i].clip = sfxLibrary.ClipsById[id];
                        sfxVoices[i].Play();
                        if (logOptions.ShowInfoLogs) Debug.Log($"[INFO] Clip now playing on SFX voice {i}.");
                        break;
                }
                return;
            }

            sfxVoices[fallbackSfxVoice].clip = sfxLibrary.ClipsById[id];
            sfxVoices[fallbackSfxVoice].Play();
            if (logOptions.ShowWarningLogs) Debug.LogWarning($"[WARNING] No free SFX voices were found. Clip now playing on SFX voice {fallbackSfxVoice} (fallback).");
        }

        /// <summary>
        /// Play an SFX clip from the SFX clip library.
        /// </summary>
        /// <param name="clipName">The name of the SFX clip (used to get the ID).</param>
        /// <param name="overrideCurrentVoice">If the specified clip is playing on a voice already, override that voice</param>
        public void PlaySFX(string clipName, bool overrideCurrentVoice) => PlaySFX(sfxLibrary.ClipsByName[clipName], overrideCurrentVoice);

        public bool IsMusicFadeRunning() { return musicFadeCoroutine == null; }
        
        // ======================[#]  UTILITY FUNCTIONS  [#]======================

        // This is intended to allow for quickly changing the source of the volume.
        // TODO: Fix fades using absolute 0-1 values instead of the current global volume
        private float GetVolume()
        {
            return currentGlobalVolume;
        }
        
        // ======================[#]  DEBUGGING FUNCTIONS  [#]======================

        private void UpdateGlobalVolumes()
        {
            currentGlobalVolume = globalVolume;
            foreach (AudioSource musicVoice in musicVoices)
                musicVoice.volume = globalVolume;
            foreach (AudioSource sfxVoice in sfxVoices)
                sfxVoice.volume = globalVolume;
        }
        
        // ======================[#]  INTERNAL COROUTINES  [#]======================

        private IEnumerator SingleMusicVoiceFade(AudioSource voiceToFade, MusicMetadata newClipData)
        {
            OnMusicStopped?.Invoke();

            float timeElapsed = 0;
            float endTime = musicFadeOutCurve.keys[^1].time;

            while (timeElapsed < endTime)
            {
                voiceToFade.volume = currentGlobalVolume * musicFadeOutCurve.Evaluate(timeElapsed);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            voiceToFade.Stop();
            voiceToFade.clip = newClipData.SongClip;
            voiceToFade.volume = currentGlobalVolume;
            voiceToFade.Play();
            OnMusicStarted?.Invoke(newClipData);
            musicFadeCoroutine = null;
        }

        private IEnumerator DoubleMusicVoiceCrossfade(AudioSource oldVoice, AudioSource newVoice, MusicMetadata newClipData)
        {
            float timeElapsed = 0;
            float oldVoiceEndTime = musicFadeOutCurve.keys[^1].time,
                newVoiceEndTime = musicFadeInCurve.keys[^1].time;
            float latestEndTime = oldVoiceEndTime >= newVoiceEndTime
                ? oldVoiceEndTime 
                : newVoiceEndTime;

            newVoice.clip = newClipData.SongClip;
            newVoice.Play();
            OnMusicStarted?.Invoke(newClipData);
            while (timeElapsed < latestEndTime)
            {
                oldVoice.volume = currentGlobalVolume * musicFadeOutCurve.Evaluate(timeElapsed);
                newVoice.volume = currentGlobalVolume * musicFadeInCurve.Evaluate(timeElapsed);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            newVoice.volume = currentGlobalVolume;
            oldVoice.Stop();
            musicFadeCoroutine = null;
        }
    }
}