// Popup Box Manager
// By Sayori Fazackerley
// January 2026

using BZApp.Systems.GUI.Components;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Systems.GUI.Components
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

        private void Update()
        {
            if (Keyboard.current.numpad1Key.wasPressedThisFrame) 
            {
                popupTextComponent.enabled = false;
                popupTextComponent.enabled = true;
                Debug.Log("Test 1 completed");
            }
            if (Keyboard.current.numpad2Key.wasPressedThisFrame) 
            {
                popupTextComponent.ForceMeshUpdate();
                Debug.Log("Test 2 completed");
            }
            if (Keyboard.current.numpad3Key.wasPressedThisFrame) 
            {
                popupTextComponent.parseCtrlCharacters = false;
                popupTextComponent.parseCtrlCharacters = true;
                Debug.Log("Test 3 completed");
            }
        }

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
            
            popupTextComponent.enabled = true;
            popupTextComponent.parseCtrlCharacters = true;
            popupTextComponent.SetText(textToSet);
            popupImageComponent.enabled = true;
            popupBackgroundComponent.enabled = true;
            StartCoroutine(ScaleObjectOverTime(popupImageComponent.rectTransform, true));
            guiSystem.FadeComponent(popupBackgroundComponent, Color.clear, darkenedBackgroundColor, fadeTime);
            guiSystem.FadeComponent(popupImageComponent, Color.clear, Color.white, fadeTime);
        }

        public void DismissPopupBox()
        {
            StartCoroutine(ScaleObjectOverTime(popupImageComponent.rectTransform, false));
            guiSystem.FadeComponent(popupBackgroundComponent, darkenedBackgroundColor, Color.clear, fadeTime);
            guiSystem.FadeComponent(popupImageComponent, Color.white, Color.clear, fadeTime);
            exitButtonComponent.enabled = false;
        }

        //-------- Internal Coroutines --------\\

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
            if (isActivating) Activated();
            else Deactivated();
        }
    }
}
