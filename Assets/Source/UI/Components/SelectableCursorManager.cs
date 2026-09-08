//  Selectables Cursor Manager
//  By Mitchel Smith
//  Created February 2026
//
//  =============================[!]  AI ASSISTANCE DISCLAIMER  [!]=============================
//  This script was created with the help of ChatGPT. The areas assisted with AI are as follows:
//  • Formula for scaling image sizeDelta with AnimationCurve:
//    (Noted in 'IEnumerator PlayCursorEngageAnimation(AnimationCurve, bool)')
//  ============================================================================================

using System.Collections;
using BZApp.GUI.Utilities;
using BZApp.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BZApp.GUI.Systems.Components
{
	public class SelectableCursorManager : GuiComponent
	{
        /* ======================[#]  CONFIGURATION  [#]====================== */

		[Header("General Cursor Settings")]
		[SerializeField] private Vector2 cursorSizePadding = new(50, 50);

		[Header("Cursor Animation Settings")]
		[SerializeField] private AnimationCurve cursorEngageAnimationCurve;
		[Tooltip("The curve for the expanding/shrinking idle animation for the cursor. Ensure that both ends of the curve each end at 1" +
				 "to maintain a good looking loop.")]
		[SerializeField] private AnimationCurve cursorIdleAnimationCurve;
		[SerializeField] private AnimationCurve cursorMoveCurve;
		
        /* ======================[#]  DEPENDENCIES  [#]====================== */

		[Header("Prefab Dependencies")]
		[SerializeField] private GameObject cursorPrefab;
		
		[Header("Object Dependencies")]
		[SerializeField] private Image mouseBlockerImage;
		[SerializeField] private GameObject cursorParentObject;

		/* ======================[#]  DEBUG SETTINGS  [#]====================== */

		[Header("Debug Settings")]
		[SerializeField] private LogOptions logOptions = new();

        /* ======================[#]  INTERNAL VARIABLES  [#]====================== */

        // Internal flags
        public bool IsEngaged { get; private set; }

		// Object/component references
		private Selectable currentSelected;
		private Image cursorImage;

		// Coroutine references
		private Coroutine cursorEngageCoroutine;
		private Coroutine cursorMoveCoroutine;
		private Coroutine cursorIdleCoroutine;

        /* ======================[#]  SELECTABLES FUNCTIONS  [#]====================== */

		/// <summary>
		/// Engage the cursor onto the current Selectable.
		/// </summary>
		public void EngageCursor()
		{
			if (IsEngaged) return;
			if (!currentSelected) GetCurrentSelected();

			// Summon the cursor
			if (!cursorImage)
				cursorImage = Instantiate(cursorPrefab, cursorParentObject.transform, false).GetComponent<Image>();
			else
				cursorImage.enabled = true;
			
			ImmediatelyMoveCursorToCurrentSelected();
			cursorEngageCoroutine = StartCoroutine(PlayCursorEngageAnimation(cursorEngageAnimationCurve, false));
		}

		/// <summary>
		/// Disengage the cursor from the current Selectable.
		/// </summary>
		public void DisengageCursor()
		{
			if (!IsEngaged) return;
            if (cursorIdleCoroutine != null)
                StopCoroutine(cursorIdleCoroutine);
            cursorEngageCoroutine = StartCoroutine(PlayCursorEngageAnimation(cursorEngageAnimationCurve, true));
        }

		/// <summary>
		/// Move the cursor's position and size to the specified Selectable.
		/// </summary>
		/// <param name="selectable">The selectable to move the cursor to, i.e. a Button.</param>
		public void MoveCursor(Selectable selectable)
		{
			if (selectable == currentSelected) return;
			Selectable oldSelectable = currentSelected;
			currentSelected = selectable;
			currentSelected.Select();

			// Cursor idle coroutine will be set back up at the end of the cursor move coroutine
			if (cursorIdleCoroutine != null)
				StopCoroutine(cursorIdleCoroutine);
			if (cursorMoveCoroutine != null)
				StopCoroutine(cursorMoveCoroutine);
			cursorMoveCoroutine = StartCoroutine(LerpCursor(oldSelectable.GetComponent<RectTransform>(), currentSelected.GetComponent<RectTransform>()));
			if (logOptions.ShowDebugLogs) Debug.Log($"[DEBUG] {GetType()}: Moving cursor to {selectable}.");
		}

        /// <summary>
        /// Get currentSelectedGameObject from the EventSystem.
        /// </summary>
        public void GetCurrentSelected()
        {
            if (EventSystem.current.currentSelectedGameObject)
                EventSystem.current.currentSelectedGameObject.TryGetComponent(out currentSelected);
            else
                EventSystem.current.firstSelectedGameObject.TryGetComponent(out currentSelected);
            if (logOptions.ShowInfoLogs) Debug.Log($"[INFO] {GetType()}: Found targeted Selectable \"{currentSelected.name}\" from EventSystem.");
        }

        /// <summary>
        /// Set the first selected Selectable and then engage the cursor.
        /// Ideal for setting a default option for an array of buttons.
        /// </summary>
        /// <param name="selectable">The Selectable (e.g. button) to have first selected.</param>
        public void SetFirstSelected(Selectable selectable)
        {
	        if (IsEngaged) return;
	        currentSelected = selectable;
	        currentSelected.Select();
			if (logOptions.ShowDebugLogs) Debug.Log($"[DEBUG] {GetType()}: Set first selected Selectable as {selectable}.");
	        EngageCursor();
        }

		private void ImmediatelyMoveCursorToCurrentSelected()
		{
			RectTransform selectedRectTransform = currentSelected.GetComponent<RectTransform>();
			cursorImage.rectTransform.position = selectedRectTransform.position;
			cursorImage.rectTransform.sizeDelta = new Vector2(selectedRectTransform.sizeDelta.x + cursorSizePadding.x, 
				                                              selectedRectTransform.sizeDelta.y + cursorSizePadding.y);
			if (logOptions.ShowDebugLogs) Debug.Log($"[DEBUG] {GetType()}: Immediately moved and resized cursor to {currentSelected} bounds.");
		}

		private Selectable GetNextSelectableFromDirection(AxisEventData axisData)
		{
            return axisData.moveDir switch
            {
                MoveDirection.Up => currentSelected.navigation.selectOnUp,
                MoveDirection.Down => currentSelected.navigation.selectOnDown,
                MoveDirection.Left => currentSelected.navigation.selectOnLeft,
                MoveDirection.Right => currentSelected.navigation.selectOnRight,
                _ => null,
            };
        }

        /* ======================[#]  SELECTABLE EVENTTRIGGER HOOKS  [#]====================== */

        public void OnMove(BaseEventData eventData)
		{
			if (eventData is AxisEventData axisData)
			{
				var newSelectable = GetNextSelectableFromDirection(axisData);
                MoveCursor(newSelectable);
                if (logOptions.ShowDebugLogs) Debug.Log($"[DEBUG] {GetType()}: Moved direction {axisData.moveDir} to {newSelectable}.");
                return;
            }

			Debug.LogError($"[ERROR] {GetType()}: OnMove called by unsupported event trigger! Event trigger must pass through AxisEventData.");
        }

        /* ======================[#]  COROUTINES  [#]====================== */

        private IEnumerator PlayCursorEngageAnimation(AnimationCurve curve, bool disengageCursor)
		{
			cursorImage.enabled = true;
			mouseBlockerImage.enabled = true;

			// All of this time setting is so that both scaling and fading can run in one while loop
			float curveDuration = cursorEngageAnimationCurve.keys[^1].time;
            float endTime = disengageCursor ? 0 : curveDuration;
			float timeElapsed = disengageCursor ? curveDuration : 0;

			RectTransform cursorRect = cursorImage.rectTransform;
			RectTransform currentSelectedRect = currentSelected.GetComponent<RectTransform>();
			Vector2 newSize = currentSelectedRect.sizeDelta + cursorSizePadding;
			
			// Run the scaling and fading of the cursor
			while (disengageCursor ? timeElapsed > endTime : timeElapsed < endTime) // Is still running?
			{
				// The idea here is that the AnimationCurve directly controls the size of the cursor, acting as
				// essentially a size multiplier while also allowing for a custom resizing curve.
				// ===
				// This formula was made with the help of ChatGPT (yes I know it's simple, I made this pretty late into
				// the night, cut me some slack here): https://chatgpt.com/share/698b2513-3620-8002-b3dd-64e93f1bc97b
				cursorRect.sizeDelta = newSize * curve.Evaluate(timeElapsed);
				
				float progress = Mathf.InverseLerp(0, curveDuration, timeElapsed);
				cursorImage.color = Color.Lerp(ExtColour.ClearWhite, Color.white, progress);
				
				timeElapsed += disengageCursor ? -Time.deltaTime : Time.deltaTime;
				yield return null;
			}

			cursorRect.sizeDelta = disengageCursor ? newSize * curve.keys[0].value : newSize * curve.keys[^1].value;
			cursorImage.color = disengageCursor ? ExtColour.ClearWhite : Color.white;

			if (!disengageCursor)
			{
				cursorIdleCoroutine = StartCoroutine(PlayIdleCursorAnimation());
				mouseBlockerImage.enabled = false;
                IsEngaged = true;
            }
			else
				IsEngaged = false;
			cursorEngageCoroutine = null;
		}

		private IEnumerator LerpCursor(RectTransform oldCursorPoint, RectTransform newCursorPoint)
		{
			float timeElapsed = 0;
			float endTime = cursorMoveCurve.keys[^1].time;

			Vector3 oldPosition = oldCursorPoint.position;
			Vector3 newPosition = newCursorPoint.position;
			Vector2 oldSize = new(oldCursorPoint.sizeDelta.x + cursorSizePadding.x, 
								  oldCursorPoint.sizeDelta.y + cursorSizePadding.y);
			Vector2 newSize = new(newCursorPoint.sizeDelta.x + cursorSizePadding.x, 
				                  newCursorPoint.sizeDelta.y + cursorSizePadding.y);

			// Move the cursor over time
			while (timeElapsed < endTime)
			{
				cursorImage.rectTransform.position = Vector3.Lerp(oldPosition, newPosition, cursorMoveCurve.Evaluate(timeElapsed));
				cursorImage.rectTransform.sizeDelta = Vector2.Lerp(oldSize, newSize, cursorMoveCurve.Evaluate(timeElapsed));
				timeElapsed += Time.deltaTime;
				yield return null;
			}

			// Complete the cursor move by setting it to the absolute end value
			cursorImage.rectTransform.position = newPosition;
			cursorImage.rectTransform.sizeDelta = newSize;

			cursorIdleCoroutine = StartCoroutine(PlayIdleCursorAnimation());
			cursorMoveCoroutine = null;
		}

		private IEnumerator PlayIdleCursorAnimation()
		{
			float endTime = cursorIdleAnimationCurve.keys[^1].time;

			RectTransform cursorRectTransform = cursorImage.rectTransform;
			Vector2 originalSize = cursorRectTransform.sizeDelta - cursorSizePadding;

			// Start loop of entire idle animation lifecycle
			while (true)
			{
				float timeElapsed = 0;
				
				// Start loop of idle animation increments
				while (timeElapsed < endTime)
				{
					cursorRectTransform.sizeDelta = originalSize + (cursorSizePadding * cursorIdleAnimationCurve.Evaluate(timeElapsed));
					timeElapsed += Time.deltaTime;
					yield return null;
				}

				cursorRectTransform.sizeDelta = originalSize;
			}
		}
	}
}