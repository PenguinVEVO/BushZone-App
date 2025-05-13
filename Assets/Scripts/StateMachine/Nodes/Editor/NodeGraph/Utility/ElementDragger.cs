using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class ElementDragger : MouseManipulator
    {
        private Vector2 dragStartMousePosition;
        private Vector2 dragStartNodePosition;
        private bool isDragging;


        public bool Enabled { get; set; } = true;

        public ElementDragger()
        {
            activators.Add(new ManipulatorActivationFilter
            {
                button = MouseButton.LeftMouse
            });
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (!Enabled || !CanStartManipulation(evt)) return;
            if (target is not GraphElement node) return;

            dragStartMousePosition = evt.mousePosition;
            dragStartNodePosition = node.GetPosition().position;

            isDragging = true;
            target.CaptureMouse();
            evt.StopPropagation();
        }


        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!Enabled || !isDragging || !target.HasMouseCapture()) return;
            if (target is not GraphElement node) return;

            Vector2 delta = evt.mousePosition - dragStartMousePosition;
            Vector2 newPos = dragStartNodePosition + delta;

            node.SetPosition(new Rect(newPos, node.GetPosition().size));
            evt.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!Enabled || !CanStopManipulation(evt)) return;

            isDragging = false;
            target.ReleaseMouse();
            evt.StopPropagation();
        }
    }
}