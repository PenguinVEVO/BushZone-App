// Popup Box Manager
// By Sayori Fazackerley
// January 2026

using BZApp.GUI.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace BZApp.GUI.Systems.Components
{
    public class PopupBoxManager : GuiComponent
    {
        //-------- General Variables --------\\

        [Header("General Settings")] 
        [SerializeField] private float fadeTime;
        [SerializeField] private Color darkenedBackgroundColor = new(0, 0, 0, 150 / 255);
        
        [SerializeField] private AnimationCurve sizeUpCurve;
        [SerializeField] private AnimationCurve sizeDownCurve;

        //-------- Object References --------\\

        [Header("Object References")] [SerializeField]
        private Image popupImageComponent;
        [SerializeField] private Image popupBackgroundComponent;
        [SerializeField] private TextMeshProUGUI popupTextComponent;
        [SerializeField] private Image exitButtonComponent;

        //-------- Internal Variables --------\\

        private Vector3 originalPopupBoxSize;
        private Component[] popupElements;
        private bool isActive;

        //-------- Lifecycle Functions --------\\

        protected override void Awake()
        {
            base.Awake();
            popupElements = new Component[4]
            {
                popupBackgroundComponent,
                popupImageComponent,
                popupTextComponent,
                exitButtonComponent
            };
            originalPopupBoxSize = popupImageComponent.rectTransform.localScale;
        }

        private void Start() => Deactivated();

        //--------Internal Functions --------\\
        
        private void Deactivated()
        {
            foreach (var component in popupElements)
                if (component is Image image)
                    image.enabled = false;
                else if (component is TextMeshProUGUI text)
                    text.enabled = false;
            popupBackgroundComponent.color = Color.clear;
            isActive = false;
        }

        private void Activated()
        {
            foreach (var component in popupElements)
                if (component is Image image)
                    image.enabled = true;
                else if (component is TextMeshProUGUI text)
                    text.enabled = true;
            StartCoroutine(InputDetection());
        }

        //-------- Main Popup API --------\\

        public void InvokePopupBox(string textToSet)
        {
            if (isActive) return;
            isActive = true;
            
            StartCoroutine(PopupInvoke(textToSet));
        }

        public void DismissPopupBox()
        {
            StartCoroutine(PopupDismiss());
        }

        //-------- Internal Coroutines --------\\

        private IEnumerator PopupInvoke(string textToSet)
        {
            popupTextComponent.enabled = true;
            textToSet = textToSet.Replace("\\n", "\n");
            popupTextComponent.SetText(textToSet);
            popupImageComponent.enabled = true;
            popupBackgroundComponent.enabled = true;
            StartCoroutine(guiSystem.FadeComponentsOverTime(new Image[1] { popupBackgroundComponent }, ExtColour.ClearWhite, darkenedBackgroundColor, fadeTime));
            StartCoroutine(guiSystem.FadeComponentsOverTime(new Image[1] { popupImageComponent }, ExtColour.ClearWhite, Color.white, fadeTime));
            StartCoroutine(guiSystem.FadeComponentsOverTime(new TextMeshProUGUI[1] { popupTextComponent }, Color.clear, Color.black, fadeTime));
            yield return StartCoroutine(ScaleObjectOverTime(popupImageComponent.rectTransform, true));
            Activated();
        }

        private IEnumerator PopupDismiss()
        {
            StartCoroutine(guiSystem.FadeComponentsOverTime(new Image[1] { popupBackgroundComponent }, darkenedBackgroundColor, ExtColour.ClearWhite, fadeTime));
            StartCoroutine(guiSystem.FadeComponentsOverTime(new Image[1] { popupImageComponent }, Color.white, ExtColour.ClearWhite, fadeTime));
            StartCoroutine(guiSystem.FadeComponentsOverTime(new TextMeshProUGUI[1] { popupTextComponent }, Color.black, Color.clear, fadeTime));
            exitButtonComponent.enabled = false;
            yield return StartCoroutine(ScaleObjectOverTime(popupImageComponent.rectTransform, false));
            Deactivated();
        }

        private IEnumerator InputDetection()
        {
            yield return new WaitUntil(() => Keyboard.current.spaceKey.wasPressedThisFrame);
            DismissPopupBox();
        }

        //-------- Utility Coroutines --------\\
        
        private IEnumerator ScaleObjectOverTime(RectTransform element, bool isActivating)
        {
            AnimationCurve currentCurve = isActivating ? sizeUpCurve : sizeDownCurve;
            
            float elapsed = 0f;
            float duration = currentCurve.keys[^1].time;

            while (elapsed < duration)
            {
                element.localScale = originalPopupBoxSize * currentCurve.Evaluate(elapsed);

                elapsed += Time.deltaTime;
                yield return null;
            }

            element.localScale = originalPopupBoxSize * currentCurve.keys[^1].value;
        }
    }
}