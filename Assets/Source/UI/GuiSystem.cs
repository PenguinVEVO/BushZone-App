//  GUI System
//  By Mitchel Smith
//  Created November 2025
//
//  =============================[!]  AI ASSISTANCE DISCLAIMER  [!]=============================
//  This script was created with the help of ChatGPT. The areas assisted with AI are as follows:
//  • Automatic GuiComponent reference architecture setup:
//    https://chatgpt.com/share/694eafda-2c50-8002-8697-b3c255b417e7
//  • Advice on FadeGraphicsOverTime overload structure (because it just looked wrong):
//    (IDE agent used, no link available)
//  • Array normalisation (brought over from another repository):
//    (IDE agent used, no link available)
//  ============================================================================================

using BZApp.GUI.Systems.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BZApp.GUI.Systems
{
    [AddComponentMenu("UI/GUI System")]
    public class GuiSystem : MonoBehaviour
    {
        // =========================[#]  INTERNAL REFERENCES  [#]=========================

        // GuiComponents dictionary
        private readonly Dictionary<Type, GuiComponent> guiComponents = new();

        // Accessor properties
        public static GuiSystem Instance { get; private set; }
        
        // =========================[#]  LIFECYCLE FUNCTIONS  [#]=========================

        protected virtual void Awake()
        {
            if (Instance == null)
                Destroy(Instance);
            Instance = this;
        }
        
        // =========================[#]  GUI COMPONENT ACCESS API  [#]=========================

        /// <summary>
        /// Register a GUI Component to the GUI System registry.
        /// </summary>
        /// <param name="guiComponent">The GUI Component to register.</param>
        public void Register(GuiComponent guiComponent)
        {
            var componentType = guiComponent.GetType();
            if (guiComponents.TryAdd(componentType, guiComponent)) return;
            Debug.LogWarning($"[WARNING] A reference for type \"{componentType}\" is already registered.");
        }
        
        /// <summary>
        /// Get a registered GUI Component from the GUI System registry.
        /// </summary>
        /// <typeparam name="T">The type of the GUI Component you're trying to access.</typeparam>
        public T Get<T>() where T : GuiComponent
        {
            return guiComponents.TryGetValue(typeof(T), out var guiComponent)
                ? (T)guiComponent : null;
        }

        /// <summary>
        /// Try and get a registered GUI Component from the GUI System registry.
        /// </summary>
        /// <typeparam name="T">The type of the GUI Component you're trying to access.</typeparam>
        /// <returns>True if component was found, otherwise false.</returns>
        public bool TryGet<T>(out T guiComponent) where T : GuiComponent
        {
            if (guiComponents.TryGetValue(typeof(T), out var component)) 
            {
                guiComponent = (T)component;
                return true;
            }
            guiComponent = null;
            return false;
        }

        // =========================[#]  UTILITY UI FUNCTIONS: GENERAL  [#]=========================

        /// <summary>
        /// Get the current colour of a Graphic.
        /// </summary>
        public static Color GetColour(Graphic graphic)
            => graphic switch
            {
                TextMeshProUGUI text => text.color,
                _ => graphic.color
            };

        /// <summary>
        /// Get an array of the current colours from an array/list of Graphics.
        /// </summary>
        public static Color[] GetColour(IReadOnlyList<Graphic> graphics)
        {
            Color[] resultingColours = new Color[graphics.Count];
            for (int i = 0; i < graphics.Count; i++)
                resultingColours[i] = GetColour(graphics[i]);
            return resultingColours;
        }

        /// <summary>
        /// Get the current colour of a Graphic with the alpha set to 0. Ideal for fading in/out elements.
        /// </summary>
        public static Color GetTransparentColour(Graphic graphic)
            => graphic switch
            {
                TextMeshProUGUI text => new Color(text.color.r, text.color.g, text.color.b, 0),
                _ => new Color(graphic.color.r, graphic.color.g, graphic.color.b, 0)
            };

        /// <summary>
        /// Get an array of the current colours from an array/list of Graphics, with the alpha values set to 0.
        /// </summary>
        public static Color[] GetTransparentColour(IReadOnlyList<Graphic> graphics)
        {
            Color[] resultingColours = new Color[graphics.Count];
            for (int i = 0; i < graphics.Count; i++)
                resultingColours[i] = GetTransparentColour(graphics[i]);
            return resultingColours;
        }

        // =========================[#]  UTILITY UI FUNCTIONS: FADING  [#]=========================

        /// <summary>
        /// Fade a Graphic between two colours over a period of time.
        /// </summary>
        /// <param name="targetGraphic">The Graphic to fade.</param>
        /// <param name="oldColour">The colour that the Graphic will fade from.</param>
        /// <param name="newColour">The colour that the Graphic will fade to.</param>
        /// <param name="fadeTime">The duration of the fade.</param>
        public IEnumerator FadeElementsOverTime(Graphic targetGraphic, Color oldColour, Color newColour, float fadeTime)
            => FadeElementsOverTime(new[] { targetGraphic }, oldColour, newColour, fadeTime);

        /// <summary>
        /// Fade a Graphic between two colours using an AnimationCurve.
        /// </summary>
        /// <param name="targetGraphic">The Graphic to fade.</param>
        /// <param name="oldColour">The colour that the Graphic will fade from.</param>
        /// <param name="newColour">The colour that the Graphic will fade to.</param>
        /// <param name="fadeCurve">The curve to control the value and time of the fade.</param>
        public IEnumerator FadeElementsOverTime(Graphic targetGraphic, Color oldColour, Color newColour, 
                                                AnimationCurve fadeCurve)
            => FadeElementsOverTime(new[] { targetGraphic }, oldColour, newColour, fadeCurve);

        /// <summary>
        /// Fade an array of Graphics between two colours over a period of time.
        /// </summary>
        /// <param name="targetGraphics">The array of Graphics to fade.</param>
        /// <param name="oldColour">The colour that the Graphics will fade from.</param>
        /// <param name="newColour">The colour that the Graphics will fade to.</param>
        /// <param name="fadeTime">The duration of the fade.</param>
        public IEnumerator FadeElementsOverTime(IReadOnlyList<Graphic> targetGraphics, Color oldColour, Color newColour, 
                                                float fadeTime)
            => FadeElementsOverTime(targetGraphics, new[] { oldColour }, new[] { newColour }, fadeTime);

        /// <summary>
        /// Fade an array of Graphics between two colours using an AnimationCurve.
        /// </summary>
        /// <param name="targetGraphics">The array of Graphics to fade.</param>
        /// <param name="oldColour">The colour that the Graphics will fade from.</param>
        /// <param name="newColour">The colour that the Graphics will fade to.</param>
        /// <param name="fadeCurve">The curve to control the value and time of the fade.</param>
        public IEnumerator FadeElementsOverTime(IReadOnlyList<Graphic> targetGraphics, Color oldColour, Color newColour, 
                                                AnimationCurve fadeCurve)
            => FadeElementsOverTime(targetGraphics, new[] { oldColour }, new[] { newColour }, fadeCurve);

        /// <summary>
        /// Fade an array of Graphics between two sets of colours over a period of time.
        /// </summary>
        /// <param name="targetGraphics">The array of Graphics to fade.</param>
        /// <param name="oldColours">The colours that the Graphics will fade from.</param>
        /// <param name="newColours">The colours that the Graphics will fade to.</param>
        /// <param name="fadeTime">The duration of the fade.</param>
        public IEnumerator FadeElementsOverTime(IReadOnlyList<Graphic> targetGraphics, IReadOnlyList<Color> oldColours, 
                                                IReadOnlyList<Color> newColours, float fadeTime)
            => FadeElementsOverTime(targetGraphics, oldColours, newColours, AnimationCurve.Linear(0, 0, fadeTime, 1));

        /// <summary>
        /// Fade an array of Graphics between two sets of colours using an AnimationCurve.
        /// </summary>
        /// <param name="targetGraphics">The array of Graphics to fade.</param>
        /// <param name="oldColours">The colours that the Graphics will fade from.</param>
        /// <param name="newColours">The colours that the Graphics will fade to.</param>
        /// <param name="fadeCurve">The curve to control the value and time of the fade.</param>
        /// <returns></returns>
        public IEnumerator FadeElementsOverTime(IReadOnlyList<Graphic> targetGraphics, IReadOnlyList<Color> oldColours, 
                                                IReadOnlyList<Color> newColours, AnimationCurve fadeCurve)
        {
            if (targetGraphics.Count <= 0) yield break;
            
            float timeElapsed = 0;
            float endTime = fadeCurve.keys[^1].time;
            Color[] oldNormalisedColours = NormaliseColours(targetGraphics.ToArray(), oldColours.ToArray());
            Color[] newNormalisedColours = NormaliseColours(targetGraphics.ToArray(), newColours.ToArray());
            
            while (timeElapsed < endTime)
            {
                timeElapsed = Mathf.Clamp(timeElapsed + Time.deltaTime, 0, endTime);
                for (int i = 0; i < targetGraphics.Count; i++)
                {
                    switch (targetGraphics[i])
                    {
                        case TextMeshProUGUI targetText: // Text has its own colour property as well as the inherited member
                            targetText.color = Color.Lerp(oldNormalisedColours[i], newNormalisedColours[i], 
                                fadeCurve.Evaluate(timeElapsed));
                            break;
                        default: // All other supported targets can just use the inherited Graphic.color member
                            targetGraphics[i].color = Color.Lerp(oldNormalisedColours[i], newNormalisedColours[i], 
                                fadeCurve.Evaluate(timeElapsed));
                            break;
                    }
                }
                yield return null;
            }
        }

        // =========================[#]  UTILITY UI FUNCTIONS: SCALING  [#]=========================

        /// <summary>
        /// Scale a RectTransform between two values over a period of time.
        /// </summary>
        /// <param name="targetRectTransform">The RectTransform to scale.</param>
        /// <param name="oldScale">The value that the RectTransforms will scale from.</param>
        /// <param name="newScale">The value that the RectTransforms will scale to.</param>
        /// <param name="scaleTime">The duration of the scale.</param>
        public IEnumerator ScaleElementsOverTime(RectTransform targetRectTransform, Vector3 oldScale,
                                                       Vector3 newScale, float scaleTime)
            => ScaleElementsOverTime(new[] { targetRectTransform }, oldScale, newScale, scaleTime);
        
        /// <summary>
        /// Scale a RectTransform using an AnimationCurve.
        /// </summary>
        /// <param name="targetRectTransform">The RectTransform to scale.</param>
        /// <param name="scaleCurve">The curve to control the value and time of the scale.</param>
        public IEnumerator ScaleElementsOverTime(RectTransform targetRectTransform, 
                                                       AnimationCurve scaleCurve)
            => ScaleElementsOverTime(new[] { targetRectTransform }, scaleCurve);
        
        /// <summary>
        /// Scale an array of RectTransforms between two values over a period of time.
        /// </summary>
        /// <param name="targetRectTransforms">The array of RectTransforms to scale.</param>
        /// <param name="oldScale">The value that the RectTransforms will scale from.</param>
        /// <param name="newScale">The value that the RectTransforms will scale to.</param>
        /// <param name="scaleTime">The duration of the scale.</param>
        public IEnumerator ScaleElementsOverTime(IReadOnlyList<RectTransform> targetRectTransforms, 
                                                       Vector3 oldScale, Vector3 newScale, float scaleTime)
            => ScaleElementsOverTime(targetRectTransforms, new[] { oldScale }, new[] { newScale }, scaleTime);

        /// <summary>
        /// Scale an array of RectTransforms between two sets of values over a period of time.
        /// </summary>
        /// <param name="targetRectTransforms">The array of RectTransforms to scale.</param>
        /// <param name="oldScales">The values that the RectTransforms will scale from.</param>
        /// <param name="newScales">The values that the RectTransforms will scale to.</param>
        /// <param name="scaleTime">The duration of the scale.</param>
        public IEnumerator ScaleElementsOverTime(IReadOnlyList<RectTransform> targetRectTransforms,
                                                       IReadOnlyList<Vector3> oldScales,
                                                       IReadOnlyList<Vector3> newScales, float scaleTime)
        {
            if (targetRectTransforms.Count <= 0) yield break;
            
            float timeElapsed = 0;
            Vector3[] oldNormalisedScales = NormaliseScales(targetRectTransforms.ToArray(), oldScales.ToArray());
            Vector3[] newNormalisedScales = NormaliseScales(targetRectTransforms.ToArray(), newScales.ToArray());
            
            while (timeElapsed < scaleTime)
            {
                timeElapsed = Mathf.Clamp(timeElapsed + Time.deltaTime, 0, scaleTime);
                for (int i = 0; i < targetRectTransforms.Count; i++)
                    targetRectTransforms[i].localScale = Vector3.Lerp(oldNormalisedScales[i], newNormalisedScales[i], timeElapsed / scaleTime);
                yield return null;
            }
        }

        /// <summary>
        /// Scale an array of RectTransforms between two sets of values using an AnimationCurve.
        /// </summary>
        /// <param name="targetRectTransforms">The array of RectTransforms to scale.</param>
        /// <param name="scaleCurve">The curve to control the start and end values and times of the scale.</param>
        public IEnumerator ScaleElementsOverTime(IReadOnlyList<RectTransform> targetRectTransforms, 
                                                       AnimationCurve scaleCurve)
        {
            if (targetRectTransforms.Count <= 0) yield break;
            
            float timeElapsed = 0;
            float endTime = scaleCurve.keys[^1].time;
            Vector3[] originalScales = new Vector3[targetRectTransforms.Count];
            for (int i = 0; i < targetRectTransforms.Count; i++)
                originalScales[i] = targetRectTransforms[i].localScale;
            
            while (timeElapsed < endTime)
            {
                timeElapsed = Mathf.Clamp(timeElapsed + Time.deltaTime, 0, endTime);
                for (int i = 0; i < targetRectTransforms.Count; i++)
                    targetRectTransforms[i].localScale = originalScales[i] * scaleCurve.Evaluate(timeElapsed);
                yield return null;
            }
        }
        
        // =========================[#]  INTERNAL HELPER FUNCTIONS  [#]=========================

        private Color[] NormaliseColours(Graphic[] graphics, Color[] targetColours)
        {
            // Unexpected case handling
            if (targetColours == null || targetColours.Length == 0)
            {
                Debug.LogWarning("[WARNING] Target colours array is null or empty. Defaulting to white colours.");
                var newDefaultColours = new Color[graphics.Length];
                for (int i = 0; i < graphics.Length; i++)
                    newDefaultColours[i] = graphics[i].color;
                return newDefaultColours;
            }
            if (targetColours.Length == graphics.Length)
                return targetColours;
            
            // Normalise the array if its length doesn't match
            var newColours = new Color[graphics.Length];
            for (int i = 0; i < targetColours.Length; i++)
                newColours[i] = targetColours[i % targetColours.Length];
            return newColours;
        }

        // TODO: Can this be merged into the above function somehow to save on line count?
        private Vector3[] NormaliseScales(RectTransform[] rectTransforms, Vector3[] targetScales)
        {
            // Unexpected case handling
            if (targetScales == null || targetScales.Length == 0)
            {
                Debug.LogWarning("[WARNING] Target scales array is null or empty. Defaulting to conventional 0/1 values.");
                var newDefaultScales = new Vector3[rectTransforms.Length];
                for (int i = 0; i < rectTransforms.Length; i++)
                    newDefaultScales[i] = rectTransforms[i].localScale;
                return newDefaultScales;
            }
            if (targetScales.Length == rectTransforms.Length)
                return targetScales;
            
            // Normalise the array if its length doesn't match
            var newScales = new Vector3[rectTransforms.Length];
            for (int i = 0; i < targetScales.Length; i++)
                newScales[i] = targetScales[i % targetScales.Length];
            return newScales;
        }
    }
}