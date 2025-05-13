using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace FrameLabs.Utilities.NodeEditor
{
    public class ProximityHighlighter
    {
        private readonly VisualElement targetElement;
        private readonly Func<Vector2, float> getDistanceToVisual;
        private readonly float threshold;
        private readonly string className;

        private bool isHovered;

        public ProximityHighlighter(
            VisualElement element,
            Func<Vector2, float> distanceFunc,
            float threshold = 15f,
            string hoverClass = "hovered")
        {
            targetElement = element ?? throw new ArgumentNullException(nameof(element));
            getDistanceToVisual = distanceFunc ?? throw new ArgumentNullException(nameof(distanceFunc));
            this.threshold = threshold;
            className = hoverClass;

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            targetElement.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            targetElement.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            float distance = getDistanceToVisual.Invoke(evt.mousePosition);

            if (distance < threshold)
            {
                if (!isHovered)
                {
                    isHovered = true;
                    targetElement.AddToClassList(className);
                    targetElement.MarkDirtyRepaint();
                }
            }
            else if (isHovered)
            {
                isHovered = false;
                targetElement.RemoveFromClassList(className);
                targetElement.MarkDirtyRepaint();
            }
        }

        private void OnMouseLeave(MouseLeaveEvent evt)
        {
            if (isHovered)
            {
                isHovered = false;
                targetElement.RemoveFromClassList(className);
                targetElement.MarkDirtyRepaint();
            }
        }      
    }
}
