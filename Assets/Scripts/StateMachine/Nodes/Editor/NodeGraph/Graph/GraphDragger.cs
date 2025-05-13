using UnityEngine;
using UnityEngine.UIElements;


namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphDragger : MouseManipulator
    {
        private Vector2 startPosition;
        private bool isDragging;

        public GraphDragger()
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.MiddleMouse });
            isDragging = false;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<PointerDownEvent>(OnPointerDown);
            target.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            target.RegisterCallback<PointerUpEvent>(OnPointerUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            target.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            target.UnregisterCallback<PointerUpEvent>(OnPointerUp);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button == (int)MouseButton.MiddleMouse)
            {
                startPosition = evt.position;
                isDragging = true;
                target.CapturePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (isDragging && target.HasPointerCapture(evt.pointerId))
            {
                Vector2 currentPosition = evt.position;
                Vector2 delta = currentPosition - startPosition;

                var currentTranslate = target.resolvedStyle.translate;
                float newX = currentTranslate.x + delta.x;
                float newY = currentTranslate.y + delta.y;

                target.style.translate = new Translate(newX, newY, 0);
                startPosition = currentPosition;

                // Update grid
                var graphView = target.GetFirstAncestorOfType<NodeGraphView>();
                if (graphView != null)
                {
                    float zoom = GetZoom(target);
                    graphView.Grid.SetZoomState(zoom, new Vector2(newX, newY));
                }

                evt.StopPropagation();
            }
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (isDragging && target.HasPointerCapture(evt.pointerId))
            {
                isDragging = false;
                target.ReleasePointer(evt.pointerId);
                evt.StopPropagation();
            }
        }

        private float GetZoom(VisualElement element)
        {
            var s = element.resolvedStyle.scale;
            return s.value.x;
        }
    }

}