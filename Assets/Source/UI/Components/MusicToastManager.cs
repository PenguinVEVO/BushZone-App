//  Music Toast Manager
//  By Mitchel Smith
//  November 2025

using BZApp.Systems.Audio;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BZApp.GUI.Systems.Components
{
    [AddComponentMenu("UI/GUI Components/Music Toast Manager")]
    public class MusicToastManager : GuiComponent
	{
        /* ======================[#]  CONFIGURATION  [#]====================== */

        [Header("Animation Settings")]
		[SerializeField] private float slideDistance;
		[SerializeField] private AnimationCurve slideInCurve;
		[SerializeField] private float toastHoldTime;
		[SerializeField] private AnimationCurve slideOutCurve;
		private float holdTimeElapsed = 0;

        /* ======================[#]  DEPENDENCIES  [#]====================== */

        [Header("Dependencies")]
		[SerializeField] private Image musicToastPanel;
		[SerializeField] private Image albumArtImage;
		[SerializeField] private TextMeshProUGUI songTitleText;
		[SerializeField] private TextMeshProUGUI songArtistText;
		[SerializeField] private TextMeshProUGUI songAlbumText;

		/* ======================[#]  INTERNAL REFERENCES  [#]====================== */

		// Internal values
		private bool callForNext = false;

		// Internal references
		private MusicData queuedData;

		// Coroutine references
		private Coroutine toastCoroutine;
		private Coroutine toastSlideCoroutine;

        /* ======================[#]  PUBLIC MUSIC TOAST API  [#]====================== */

        /// <summary>
        /// Invoke the music toast panel to display song information.
        /// </summary>
        /// <param name="dataToDisplay">The metadata object of the song.</param>
        public void InvokeToast(MusicData dataToDisplay)
		{
			if (toastCoroutine != null)
			{
                holdTimeElapsed = toastHoldTime;
                queuedData = dataToDisplay;
                callForNext = true;
            }
            else 
				toastCoroutine = StartCoroutine(PlayToastAnimation(dataToDisplay));
        }

		/// <summary>
		/// Manually invoke the dismissal of the music toast panel.
		/// </summary>
		public void DismissToast()
		{
			// This check has to stay here in the case of other classes calling this
			if (toastCoroutine != null)
				holdTimeElapsed = toastHoldTime;
			else
				Debug.LogWarning("[WARNING] MusicToastManager: Cannot dismiss music toast as it is not currently active.");
		}

        /* ======================[#]  INTERNAL FUNCTIONS  [#]====================== */

        private void SetMusicData(MusicData data)
		{
			albumArtImage.sprite = data.AlbumArt;
			songTitleText.text = data.SongName;
			songArtistText.text = ""; // Clear it from the previous use
			for (int i = 0; i < data.ArtistNames.Length; i++)
			{
				songArtistText.text += data.ArtistNames[i];
				if (data.ArtistNames[i] != data.ArtistNames[^1])
					songArtistText.text += ", ";
			}
			songAlbumText.text = data.AlbumName;
		}
        
		/* ======================[#]  SEQUENCE COROUTINES  [#]====================== */

		private IEnumerator PlayToastAnimation(MusicData dataToDisplay)
		{
			// Slide in
            if (toastSlideCoroutine == null)
			{
                SetMusicData(dataToDisplay);
                toastSlideCoroutine = StartCoroutine(SlideToast(slideInCurve, slideDistance));
            }
            else
			{
                holdTimeElapsed = toastHoldTime;
				callForNext = true;
            }
			yield return new WaitUntil(() => toastSlideCoroutine == null);

            // Hold
            holdTimeElapsed = 0;
            while (holdTimeElapsed < toastHoldTime)
			{
				holdTimeElapsed += Time.deltaTime;
				yield return null;
			}

			// Slide out
			toastSlideCoroutine = StartCoroutine(SlideToast(slideOutCurve, -slideDistance));
            yield return new WaitUntil(() => toastSlideCoroutine == null);
			if (callForNext)
			{
				callForNext = false; // Set false for the next sequence
				toastCoroutine = StartCoroutine(PlayToastAnimation(queuedData));
			}
			else 
				toastCoroutine = null;
        }

        private IEnumerator SlideToast(AnimationCurve curve, float slideAmount)
		{
			float timeElapsed = 0;
			float endTime = curve.keys[^1].time;
			float startPoint = musicToastPanel.rectTransform.anchoredPosition.x;
			float endPoint = startPoint + slideAmount;

			while (timeElapsed < endTime)
			{
				musicToastPanel.rectTransform.anchoredPosition = 
					new(Mathf.Lerp(startPoint, endPoint, curve.Evaluate(timeElapsed)), musicToastPanel.rectTransform.anchoredPosition.y);
				timeElapsed += Time.deltaTime;
				yield return null;
			}

            musicToastPanel.rectTransform.anchoredPosition = new(endPoint, musicToastPanel.rectTransform.anchoredPosition.y);
			toastSlideCoroutine = null;
        }
	}
}