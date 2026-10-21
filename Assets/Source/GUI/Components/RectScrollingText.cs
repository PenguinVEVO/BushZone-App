using System.Collections;
using TMPro;
using UnityEngine;

namespace BZApp.GUI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class RectScrollingText : MonoBehaviour
    {
        [Header("Scroll Settings")]
        [SerializeField] private bool runAutomatic = true;
        [SerializeField] private float scrollSpeed = 1f;
        [SerializeField] private float haltTime = 2f;
        
        [Header("Dependencies")]
        [SerializeField] private RectTransform boundingRectBox;
        
        // Values
        private Vector3 originalLocalPosition;
        
        // References
        private TextMeshProUGUI thisTextObject;
        private Coroutine scrollCoroutine;

        private void Awake()
        {
            thisTextObject = GetComponent<TextMeshProUGUI>();
            originalLocalPosition = thisTextObject.rectTransform.localPosition;
        }

        private void OnEnable()
        {
            if (!runAutomatic) return;
            StartScrolling();
        }

        private void OnDisable()
        {
            if (!runAutomatic) return;
            StopScrolling(false);
        }

        public void StartScrolling()
        {
            // This will only run if the text is actually wider than the bounding box
            if (thisTextObject.preferredWidth <= boundingRectBox.rect.width || scrollCoroutine != null) return;
            scrollCoroutine = StartCoroutine(ScrollText());
        }

        public void StopScrolling(bool preservePosition)
        {
            if (scrollCoroutine == null) return;
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
            if (preservePosition) return;
            thisTextObject.rectTransform.localPosition = originalLocalPosition;
        }

        private IEnumerator ScrollText()
        {
            bool isGoingRight = true;
            float startPointX = thisTextObject.rectTransform.localPosition.x;
            while (true)
            {
                // Halt stage
                yield return new WaitForSeconds(haltTime);
                
                // Moving stage
                float direction = isGoingRight ? 1 : -1;
                float endPointX = boundingRectBox.rect.width - thisTextObject.preferredWidth;
                float target = isGoingRight ? endPointX : startPointX;
                while (thisTextObject.rectTransform.localPosition.x != target)
                {
                    Vector3 newPos = thisTextObject.rectTransform.localPosition;
                    newPos.x = Mathf.Clamp(newPos.x - direction * scrollSpeed * Time.deltaTime, endPointX, startPointX);
                    thisTextObject.rectTransform.localPosition = newPos;
                    yield return null;
                }
                isGoingRight = !isGoingRight;
            }
        }
    }
}
