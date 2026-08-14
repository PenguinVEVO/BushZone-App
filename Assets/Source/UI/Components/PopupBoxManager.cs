// Popup Box Manager
// By Sayori Fazackerley
// January 2026

using BZApp.GUI.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BZApp.GUI.Systems.Components
{
    [AddComponentMenu("UI/GUI Components/Popup Box Manager")]
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
            popupImageComponent.rectTransform.localScale = originalPopupBoxSize;
            popupTextComponent.rectTransform.localScale = originalPopupBoxSize;
            exitButtonComponent.rectTransform.localScale = originalPopupBoxSize;
            isActive = false;
        }

        private void Activated()
        {
            foreach (var component in popupElements)
                if (component is Image image)
                    image.enabled = true;
                else if (component is TextMeshProUGUI text)
                    text.enabled = true;
            StartCoroutine(InputDetection(true));
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
            StartCoroutine(guiSystem.FadeElementsOverTime(new Image[1] { popupBackgroundComponent }, ExtColour.ClearWhite, darkenedBackgroundColor, fadeTime));
            StartCoroutine(guiSystem.FadeElementsOverTime(new Image[1] { popupImageComponent }, ExtColour.ClearWhite, Color.white, fadeTime));
            StartCoroutine(guiSystem.FadeElementsOverTime(new TextMeshProUGUI[1] { popupTextComponent }, Color.clear, Color.black, fadeTime));
            yield return StartCoroutine(guiSystem.ScaleElementsOverTime(popupImageComponent.rectTransform, sizeUpCurve));
            Activated();
        }

        private IEnumerator PopupDismiss()
        {
            StartCoroutine(guiSystem.FadeElementsOverTime(new Image[1] { popupBackgroundComponent }, darkenedBackgroundColor, ExtColour.ClearWhite, fadeTime));
            StartCoroutine(guiSystem.FadeElementsOverTime(new Image[1] { popupImageComponent }, Color.white, ExtColour.ClearWhite, fadeTime));
            StartCoroutine(guiSystem.FadeElementsOverTime(new TextMeshProUGUI[1] { popupTextComponent }, Color.black, Color.clear, fadeTime));
            exitButtonComponent.enabled = false;
            yield return StartCoroutine(guiSystem.ScaleElementsOverTime(popupImageComponent.rectTransform, sizeDownCurve));
            Deactivated();
        }

        private IEnumerator InputDetection(bool isActivated)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                DismissPopupBox();
                isActive = false;
            }
            while (isActive == true)
            {
                StartCoroutine(guiSystem.FadeElementsOverTime(exitButtonComponent, Color.white, ExtColour.ClearWhite, fadeTime));
                yield return new WaitForSeconds(fadeTime);
                StartCoroutine(guiSystem.FadeElementsOverTime(exitButtonComponent, ExtColour.ClearWhite, Color.white, fadeTime));
                yield return new WaitForSeconds(fadeTime);
            }
        }
    }
}