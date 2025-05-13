using FrameLabs.Utilities.NodeEditor;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CreateWidgetCommand<T> : Command where T : GraphWidget
{
    private T widget;
    private readonly StateGraphView graphView;
    private readonly Vector2 position;
    private readonly Func<StateGraphView, T> createWidgetView;
    private readonly Action<T> onWidgetCreated;

    private bool created = false;

    public T CreatedWidget => widget;

    public CreateWidgetCommand(
        StateGraphView graphView,
        Vector2 position,
        Func<StateGraphView, T> createWidgetView,
        Action<T> onWidgetCreated)
    {
        this.graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
        this.position = position;
        this.createWidgetView = createWidgetView ?? throw new ArgumentNullException(nameof(createWidgetView));
        this.onWidgetCreated = onWidgetCreated;
    }

    public override void Execute()
    {
        if (widget == null)
        {
            widget = createWidgetView(graphView);
            created = true;
        }

        graphView.AddVisualGraphElement(widget);
        SetWidgetPosition(widget, position);

        if (created)
        {
            onWidgetCreated?.Invoke(widget);
            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }
    }

    public override void Undo()
    {
        if (widget != null)
        {
            graphView.RemoveVisualGraphElement(widget);
        }

        GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
    }

    public override void Redo()
    {
        if (widget != null)
        {
            graphView.AddVisualGraphElement(widget);
            SetWidgetPosition(widget, position);
        }

        GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
    }

    private void SetWidgetPosition(T widget, Vector2 worldPosition)
    {
        if (widget == null) return;

        Vector2 localPosition = graphView.contentContainer.WorldToLocal(worldPosition);
        var width = widget.resolvedStyle.width > 0 ? widget.resolvedStyle.width : widget.Width;
        var height = widget.resolvedStyle.height > 0 ? widget.resolvedStyle.height : widget.Height;

        widget.SetPosition(new Rect(localPosition, new Vector2(height, width)));
    }
}
