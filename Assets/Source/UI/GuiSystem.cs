//  GUI System
//  By Mitchel Smith
//  Created November 2025
//
//  =============================[!]  AI ASSISTANCE DISCLAIMER  [!]=============================
//  This script was created with the help of ChatGPT. The areas assisted with AI are as follows:
//  • Automatic GuiComponent reference architecture setup:
//    https://chatgpt.com/share/694eafda-2c50-8002-8697-b3c255b417e7
//  ============================================================================================

using BZApp.Systems.GUI.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BZApp.Systems.GUI
{
    [AddComponentMenu("UI/GUI System/GUI System")]
    public class GuiSystem : MonoBehaviour
    {
        /* ======================[#]  INTERNAL REFERENCES  [#]====================== */

        // GuiComponents dictionary
        private Dictionary<Type, GuiComponent> guiComponents = new();

        // Accessor properties
        public static GuiSystem Instance { get; private set; }
        
        /* ======================[#]  LIFECYCLE FUNCTIONS  [#]====================== */

        protected virtual void Awake()
        {
            if (Instance == null)
                Destroy(Instance);
            Instance = this;
        }
        
        /* ======================[#]  GUICOMPONENT ACCESS API  [#]====================== */

        /// <summary>
        /// Register a GUI Component to the GUI System's registry.
        /// </summary>
        /// <param name="guiComponent">The GUI Component to register.</param>
        public void Register(GuiComponent guiComponent)
        {
            var componentType = guiComponent.GetType();
            if (guiComponents.ContainsKey(componentType)) // Note: TryAdd is not used here because reference is hard added
            {
                Debug.LogError($"[ERROR] GuiSystem: A reference for type {componentType} is already registered!");
                return;
            }
            
            guiComponents[componentType] = guiComponent;
        }
        
        /// <summary>
        /// Get a registered GUI Component from the GUI System's registry.
        /// </summary>
        /// <typeparam name="T">The name of the GUI Component you're trying to access.</typeparam>
        public T Get<T>() where T : GuiComponent
        {
            if (guiComponents.TryGetValue(typeof(T), out GuiComponent guiComponent))
                return (T)guiComponent;
            
            Debug.LogError($"[ERROR] GuiSystem: Could not find GUI Component of type {typeof(T)}.\n" +
                           $"Ensure that {typeof(T)} is properly registered to the GUI System before trying to access it.");
            return null;
        }

        /* ======================[#]  UTILITY UI FUNCTIONS: FADING  [#]====================== */

        /// <summary>
        /// Fade an array of components between two colours over a period of time.
        /// </summary>
        /// <param name="components">The array of components to fade.</param>
        /// <param name="oldColour">The (starting) colour to fade from.</param>
        /// <param name="newColour">The (finishing) colour to fade to.</param>
        /// <param name="fadeTime">The period of time to fade the colours over.</param>
        public IEnumerator FadeComponentsOverTime(Component[] components, Color oldColour, Color newColour, float fadeTime)
        {
            float timeElapsed = 0;

            // Check components array for unsupported components
            foreach (Component c in components)
            {
                if (c is Image || c is TextMeshProUGUI) continue;
                Debug.LogError($"[ERROR] GuiSystem.FadeComponentsOverTime: Unsupported component type detected! \n" +
                    "Make sure the array you're passing through to this function only contains Image and TextMeshProUGUI components.");
                yield break;
            }

            // Fade the component colours over time
            while (timeElapsed < fadeTime)
            {
                foreach (Component c in components)
                {
                    switch (c)
                    {
                        case Image img:
                            img.color = Color.Lerp(oldColour, newColour, timeElapsed / fadeTime);
                            break;
                        case TextMeshProUGUI text:
                            text.color = Color.Lerp(oldColour, newColour, timeElapsed / fadeTime);
                            break;
                    }
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Complete the fade by setting the component colours to the absolute end values
            foreach (Component c in components)
            {
                switch (c)
                {
                    case Image img:
                        img.color = newColour;
                        break;
                    case TextMeshProUGUI text:
                        text.color = newColour;
                        break;
                }
            }
        }

        /// <summary>
        /// Fade an array of components between two colours over a period of time using an AnimationCurve.
        /// </summary>
        /// <param name="components">The array of components to fade.</param>
        /// <param name="oldColour">The (starting) colour to fade from.</param>
        /// <param name="newColour">The (finishing) colour to fade to.</param>
        /// <param name="timeCurve">The curve to fade the colours with.</param>
        public IEnumerator FadeComponentsOverTime(Component[] components, Color oldColour, Color newColour, AnimationCurve timeCurve)
        {
            float timeElapsed = 0;
            float endTime = timeCurve.keys[^1].time;

            // Check components array for unsupported components
            foreach (Component c in components)
            {
                if (c is Image || c is TextMeshProUGUI) continue;
                Debug.LogError($"[ERROR] GuiSystem.FadeComponentsOverTime: Unsupported component type detected! \n" +
                    "Make sure the array you're passing through to this function only contains Image and TextMeshProUGUI components.");
                yield break;
            }

            // Fade the component colours over time
            while (timeElapsed < endTime)
            {
                foreach (Component c in components)
                {
                    switch (c)
                    {
                        case Image img:
                            float imgFadeProgress = Mathf.Clamp01(timeCurve.Evaluate(timeElapsed));
                            img.color = Color.Lerp(oldColour, newColour, imgFadeProgress);
                            break;
                        case TextMeshProUGUI text:
                            float textFadeProgress = Mathf.Clamp01(timeCurve.Evaluate(timeElapsed));
                            text.color = Color.Lerp(oldColour, newColour, textFadeProgress);
                            break;
                    }
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Complete the fade by setting the component colours to the absolute end values
            foreach (Component c in components)
            {
                switch (c)
                {
                    case Image img:
                        img.color = newColour;
                        break;
                    case TextMeshProUGUI text:
                        text.color = newColour;
                        break;
                }
            }
        }

        /* ======================[#]  UTILITY UI FUNCTIONS: SCALING  [#]====================== */

        /// <summary>
        /// Scale an array of components between two values over a period of time.
        /// </summary>
        /// <param name="components">The array of components to scale.</param>
        /// <param name="oldColour">The (starting) value to scale from.</param>
        /// <param name="newColour">The (finishing) value to scale to.</param>
        /// <param name="fadeTime">The period of time to scale the components over.</param>
        public IEnumerator ScaleComponentsOverTime(Component[] components, Vector3 oldScale, Vector3 newScale, float scaleTime)
        {
            float timeElapsed = 0;

            // Check components array for unsupported components
            foreach (Component c in components)
            {
                if (c is RectTransform) continue;
                Debug.LogError($"[ERROR] GuiSystem.FadeComponentsOverTime: Unsupported component type detected! \n" +
                    "Make sure the array you're passing through to this function only contains RectTransform components.");
                yield break;
            }

            // Scale the components over time
            while (timeElapsed < scaleTime)
            {
                foreach (Component c in components)
                {
                    switch (c)
                    {
                        case Image img:
                            img.rectTransform.localScale = Vector3.Lerp(oldScale, newScale, timeElapsed / scaleTime);
                            break;
                        case TextMeshProUGUI text:
                            text.rectTransform.localScale = Vector3.Lerp(oldScale, newScale, timeElapsed / scaleTime);
                            break;
                    }
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Complete the scaling by setting the component scale to the absolute end values
            foreach (Component c in components)
            {
                switch (c)
                {
                    case Image img:
                        img.rectTransform.localScale = newScale;
                        break;
                    case TextMeshProUGUI text:
                        text.rectTransform.localScale = newScale;
                        break;
                }
            }
        }

        /// <summary>
        /// Scale an array of components between two values over a period of time using an AnimationCurve.
        /// </summary>
        /// <param name="components">The array of components to scale.</param>
        /// <param name="oldColour">The (starting) value to scale from.</param>
        /// <param name="newColour">The (finishing) value to scale to.</param>
        /// <param name="timeCurve">The curve to scale the components with.</param>
        public IEnumerator ScaleComponentsOverTime(Component[] components, Vector3 oldScale, Vector3 newScale, AnimationCurve timeCurve)
        {
            float timeElapsed = 0;
            float endTime = timeCurve.keys[^1].time;

            // Check components array for unsupported components
            foreach (Component c in components)
            {
                if (c is RectTransform) continue;
                Debug.LogError($"[ERROR] GuiSystem.FadeComponentsOverTime: Unsupported component type detected! \n" +
                    "Make sure the array you're passing through to this function only contains RectTransform components.");
                yield break;
            }

            // Scale the components over time
            while (timeElapsed < endTime)
            {
                foreach (Component c in components)
                {
                    switch (c)
                    {
                        case Image img:
                            float imgFadeProgress = Mathf.Clamp01(timeCurve.Evaluate(timeElapsed));
                            img.rectTransform.localScale = Vector3.Lerp(oldScale, newScale, imgFadeProgress);
                            break;
                        case TextMeshProUGUI text:
                            float textFadeProgress = Mathf.Clamp01(timeCurve.Evaluate(timeElapsed));
                            text.rectTransform.localScale = Vector3.Lerp(oldScale, newScale, textFadeProgress);
                            break;
                    }
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // Complete the scaling by setting the component scale to the absolute end values
            foreach (Component c in components)
            {
                switch (c)
                {
                    case Image img:
                        img.rectTransform.localScale = newScale;
                        break;
                    case TextMeshProUGUI text:
                        text.rectTransform.localScale = newScale;
                        break;
                }
            }
        }

        #region ======================[#]  OBSOLETE FUNCTIONS  [#]======================

        [Obsolete("Use the \"GuiSystem.FadeComponentsOverTime\" coroutine instead. For fading single components, initialise a " +
            "one-length array in the component parameter when calling the coroutine.")]
        public void FadeComponent(Component component, Color oldColour, Color newColour, float fadeTime)
        {
            StartCoroutine(FadeComponentsOverTime(new Component[1] { component }, oldColour, newColour, fadeTime));
        }

        [Obsolete("Use the \"GuiSystem.FadeComponentsOverTime\" coroutine instead.")]
        public void FadeComponent(Component[] components, Color oldColour, Color newColour, float fadeTime)
        {
            StartCoroutine(FadeComponentsOverTime(components, oldColour, newColour, fadeTime));
        }

        #endregion
    }
}