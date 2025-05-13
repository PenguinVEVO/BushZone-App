using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using FrameLabs.Utilities.NodeEditor;

public class UnifiedElementDragger<T> : MouseManipulator where T : NodeElement
{
    private readonly Func<List<T>> getSelection;
    private Vector2 dragStartMousePosition;
    private Dictionary<T, Vector2> dragStartOffsets;
    private bool waitingForDragStart;
    private bool isDragging;
    private const float DragThreshold = 3f;

    public UnifiedElementDragger(Func<List<T>> getSelectionFunc)
    {
        getSelection = getSelectionFunc ?? throw new ArgumentNullException(nameof(getSelectionFunc));

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
        if (!CanStartManipulation(evt)) return;
        if (target is not T element) return;

        dragStartMousePosition = target.ChangeCoordinatesTo(target.parent, evt.localMousePosition);

        waitingForDragStart = true;
        isDragging = false;
        dragStartOffsets = null;

        evt.StopPropagation();
    }

    private void OnMouseMove(MouseMoveEvent evt)
    {
        if (!isDragging && !waitingForDragStart) return;

        Vector2 currentMousePos = target.ChangeCoordinatesTo(target.parent, evt.localMousePosition);

        if (waitingForDragStart)
        {
            if (Vector2.Distance(currentMousePos, dragStartMousePosition) >= DragThreshold)
            {
                waitingForDragStart = false;
                isDragging = true;
                target.CaptureMouse();

                dragStartOffsets = new Dictionary<T, Vector2>();
                List<T> selection = getSelection();

                foreach (var element in selection)
                {
                    if (element == null || element.panel == null)
                        continue;

                    dragStartOffsets[element] = element.GetPosition().position;
                }
            }
        }

        if (isDragging && dragStartOffsets != null)
        {
            float zoom = 1f;
            if (target.parent is NodeGraphView graphView)
                zoom = graphView.GetZoomScale();

            Vector2 delta = (currentMousePos - dragStartMousePosition) / zoom;

            foreach (var kvp in dragStartOffsets)
            {
                if (kvp.Key == null || kvp.Key.panel == null)
                    continue;

                Vector2 newPos = kvp.Value + delta;
                kvp.Key.SetPosition(new Rect(newPos, kvp.Key.GetPosition().size));
            }

            evt.StopPropagation();
        }
    }

    private void OnMouseUp(MouseUpEvent evt)
    {
        if (!CanStopManipulation(evt)) return;

        waitingForDragStart = false;
        isDragging = false;
        dragStartOffsets?.Clear();
        target.ReleaseMouse();
        evt.StopPropagation();
    }
}
