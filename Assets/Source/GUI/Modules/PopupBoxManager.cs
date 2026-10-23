//  Popup Box Manager
//  By Artronoth
//  Created January 2026

using BZApp.GUI.Utilities;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BZApp.GUI.Modules
{
    [AddComponentMenu("UI/GUI System Modules/PopupBoxManager")]
    public class PopupBoxManager : GuiSystemModule
    {
        //-------- Configuration --------\\

        [Header("General Settings")] 
        [SerializeField] private float fadeTime;
        [SerializeField] private Color darkenedBackgroundColor = new(0, 0, 0, 150 / 255);

        [Header("Animation Settings")]
        [SerializeField] private AnimationCurve sizeUpCurve;
        [SerializeField] private AnimationCurve sizeDownCurve;
        [SerializeField] private AnimationCurve buttonCurve;

        [Header("Input Settings")]
        [SerializeField] private string confirmActionMapName;
        [SerializeField] private string confirmActionName;

        //-------- Dependencies --------\\

        [Header("Dependencies")] 
        [SerializeField] private Image popupImageComponent;
        [SerializeField] private Image popupBackgroundComponent;
        [SerializeField] private TextMeshProUGUI popupTextComponent;
        [SerializeField] private Image exitButtonComponent;

        //-------- Internal Variables --------\\

        private Vector3 originalPopupBoxSize;
        private Component[] popupElements;
        private bool isActive;
        private Coroutine flash;
        private InputAction confirmAction;
        private GlyphsManager glyphsManager;
        private TrackableGlyph confirmGlyph;

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
            confirmAction = InputSystem.actions.FindActionMap(confirmActionMapName).FindAction(confirmActionName);
        }

        private void Start()
        {
            glyphsManager = GuiSystem.Instance.Get<GlyphsManager>();
            Deactivated();
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
            exitButtonComponent.sprite = glyphsManager.GetGlyph(confirmActionMapName, confirmActionName);
            confirmGlyph = new TrackableGlyph(exitButtonComponent, confirmActionMapName, confirmActionName);
            glyphsManager.StartTrackingGlyph(confirmGlyph);
            flash = StartCoroutine(ButtonFlash());
            confirmAction.performed += OnConfirm;
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
            StopCoroutine(flash);
            glyphsManager.StopTrackingGlyph(confirmGlyph);
            Deactivated();
        }

        private IEnumerator ButtonFlash()
        { 
            float flashTime = buttonCurve.keys[^1].time;
            while (true)
            {
                StartCoroutine(guiSystem.FadeElementsOverTime(exitButtonComponent, ExtColour.ClearWhite, Color.white, buttonCurve));
                yield return new WaitForSeconds(flashTime);
            }
        }

        //-------- Input System Hooks --------\\

        private void OnConfirm(InputAction.CallbackContext context)
        {
            confirmAction.performed -= OnConfirm;
            DismissPopupBox();
        }
    }
}