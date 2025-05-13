using UnityEngine;
using UnityEngine.UIElements;


namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphZoom : MouseManipulator
    {
        private VisualElement targetElement;
        private float zoomScale = 1f;
        private float minZoom = 0.5f;
        private float maxZoom = 3.0f;
        private float zoomStep = 0.03f;

        private Vector2 baseSize = Vector2.zero;

        public GraphZoom()
        {
            activators.Clear();
        }

        protected override void RegisterCallbacksOnTarget()
        {
            targetElement = target;

            targetElement.RegisterCallback<WheelEvent>(OnWheel);
            targetElement.RegisterCallback<KeyDownEvent>(OnKeyDown);
            targetElement.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            if (targetElement != null)
            {
                targetElement.UnregisterCallback<WheelEvent>(OnWheel);
                targetElement.UnregisterCallback<KeyDownEvent>(OnKeyDown);
                targetElement.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                targetElement = null;
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (baseSize == Vector2.zero && evt.newRect.size.magnitude > 0f)
                baseSize = evt.newRect.size;
        }

        private void OnWheel(WheelEvent evt)
        {
            if (targetElement == null || targetElement.parent == null)
                return;

            float delta = Mathf.Sign(evt.delta.y) * zoomStep;
            float newScale = Mathf.Clamp(zoomScale + delta, minZoom, maxZoom);
            if (Mathf.Approximately(newScale, zoomScale))
                return;

            Vector2 worldMouse = evt.mousePosition;
            Vector2 localMouse = targetElement.WorldToLocal(worldMouse);
            Vector2 currentTranslation = GetTranslation(targetElement);

            Vector2 preZoomOffset = (localMouse - currentTranslation) / zoomScale;
            Vector2 postZoomOffset = preZoomOffset * newScale;
            Vector2 offset = localMouse - postZoomOffset;

            Vector2 newTranslation = ClampTranslationToBounds(offset, newScale);

            zoomScale = newScale;
            targetElement.style.scale = new Scale(new Vector3(zoomScale, zoomScale, 1));
            targetElement.style.translate = new Translate(newTranslation.x, newTranslation.y, 0);

            if (targetElement is NodeGraphView graphView)
            {
                graphView.UpdateAllEdges();
                graphView.Grid?.SetZoomState(zoomScale, newTranslation);
            }

            evt.StopImmediatePropagation();
        }

        private Vector2 ClampTranslationToBounds(Vector2 desiredTranslation, float scale)
        {
            if (targetElement == null || targetElement.parent == null)
                return desiredTranslation;

            Vector2 parentSize = new Vector2( targetElement.parent.resolvedStyle.width,
                targetElement.parent.resolvedStyle.height);

            Vector2 contentSize = baseSize * scale;

            float minX = Mathf.Min(0f, parentSize.x - contentSize.x);
            float minY = Mathf.Min(0f, parentSize.y - contentSize.y);
            float maxX = 0f;
            float maxY = 0f;

            float clampedX = Mathf.Clamp(desiredTranslation.x, minX, maxX);
            float clampedY = Mathf.Clamp(desiredTranslation.y, minY, maxY);

            return new Vector2(clampedX, clampedY);
        }

        private Vector2 GetTranslation(VisualElement element)
        {
            var t = element.resolvedStyle.translate;
            return new Vector2(t.x, t.y);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.F)
            {
                ResetView();
                evt.StopImmediatePropagation();
            }
        }

        public void SetupZoom(float standardScale = 1.0f, float zoomFactor = 3.0f, float zoomStep = 0.03f)
        {
            minZoom = standardScale * 0.5f;
            maxZoom = standardScale * zoomFactor;
            this.zoomStep = Mathf.Clamp(zoomStep, 0.01f, 0.2f);

            if (targetElement != null)
            {
                zoomScale = standardScale;
                targetElement.style.scale = new Scale(new Vector3(zoomScale, zoomScale, 1));
                targetElement.style.translate = new Translate(0, 0, 0);
            }
        }

        public void ResetView()
        {
            zoomScale = 1f;

            if (targetElement != null)
            {
                targetElement.style.scale = new Scale(Vector3.one);
                targetElement.style.translate = new Translate(0, 0, 0);

                if (baseSize != Vector2.zero)
                {
                    targetElement.style.minWidth = baseSize.x;
                    targetElement.style.minHeight = baseSize.y;
                }

                if (targetElement is NodeGraphView graphView)
                {
                    graphView.UpdateAllEdges();
                    graphView.Grid?.SetZoomState(zoomScale, Vector2.zero);
                }
            }
        }

        public float GetZoomScale() => zoomScale;
    }
}