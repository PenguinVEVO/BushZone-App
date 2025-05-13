using FrameLabs.Utilities.NodeEditor;
using System;
using UnityEditor.UIElements;
using UnityEngine;

/// <summary>
/// Factory class to create visual-only graph elements from serialized data.
/// </summary>
public static class GraphElementFactory
{
    /// <summary>
    /// Creates a graph element from the provided data.
    /// </summary>
    /// <param name="data">Serialized visual element data.</param>
    /// <returns>The created visual graph element, or null if the type is unrecognized.</returns>
    public static GraphWidget CreateElementFromData(VisualElementData data)
    {
        // Handle element creation based on type
        switch (data.ElementType)
        {
            case ReroutePinElement.TypeID:
                return CreateReroutePinElement(data);
            default:
                Debug.LogWarning($"[GraphElementFactory] Unrecognized visual element type: {data.ElementType}");
                return null;
        }
    }

    /// <summary>
    /// Copies VisualElementData from an existing ReroutePinElement.
    /// </summary>
    /// <summary>
    /// Copies VisualElementData from a generic GraphWidget.
    /// </summary>
    public static VisualElementData CopyGraphWidgetData(GraphWidget widget, bool copyId = true)
    {
        if (widget == null) return null;

        var copiedData = new VisualElementData
        {
            ElementType = widget.ElementType,
            Id = copyId ? widget.ElementID : Guid.NewGuid().ToString(),
            position = widget.GetPosition().position
        };

        return copiedData;
    }


    /// <summary>
    /// Creates a ReroutePinElement from serialized data.
    /// </summary>
    private static ReroutePinElement CreateReroutePinElement(VisualElementData data)
    {
        var reroutePin = new ReroutePinElement(data.Id);
        reroutePin.SetPosition(new Rect(data.position, new Vector2(10, 10)));

        // Assign direction based on saved inputIsLeft property
        if (data.inputIsLeft)
        {
            reroutePin.AssignDirection(reroutePin.GetLeftAnchor());
        }
        else
        {
            reroutePin.AssignDirection(reroutePin.GetRightAnchor());
        }

        return reroutePin;
    }


}
