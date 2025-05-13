using FrameLabs.Utilities.NodeEditor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Represents a visual-only element in the graph
/// </summary>
public abstract class GraphWidget : NodeElement, IGraphElementView, ICopyableElement
{
    protected string typeID;
    public string ElementID { get; private set; }
    public string ElementType => typeID;

    public virtual float Height => 12;
    public virtual float Width => 12;

    public GraphWidget(string widgetName = "Widget", string id = null)
        : base(widgetName, id)
    {
        ElementID = id ?? Guid.NewGuid().ToString();
        nodeID = ElementID;

        AddToClassList("graph-widget");
    }

    /// <summary>
    /// Visual-only widgets usually do not need any extra layout unless overridden.
    /// </summary>
    protected override void SetupUI(string name) { }
        

    /// <summary>
    /// GraphWidgets typically do not have runtime logic or ports.
    /// </summary>
    public override void EnableInteraction(bool enabled)
    {
        pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
        bodyContainer.pickingMode = pickingMode;
    }

    public abstract object CreateCopyData(bool copyId);
    
}
