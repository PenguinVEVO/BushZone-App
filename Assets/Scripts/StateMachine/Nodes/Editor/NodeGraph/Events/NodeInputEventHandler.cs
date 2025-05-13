using UnityEngine;
using UnityEngine.UIElements;
using FrameLabs.Utilities.NodeEditor;
using System.Linq;

public class NodeInputEventHandler : EventHandler
{
    private readonly NodeElement node;
    private StateGraphView graphView;

    public NodeInputEventHandler(NodeElement node)
    {
        this.node = node;

        node.schedule.Execute(() =>
        {
            graphView = node.GetFirstAncestorOfType<StateGraphView>();


            if( graphView != null)
            { 
            node.AddManipulator(new UnifiedElementDragger<NodeElement>(
                () => graphView.Selection.OfType<NodeElement>().ToList()));
                }
        });
    }

    public void CopySelection() => graphView.Undo.ExecuteCommand(new CopyCommand(graphView));
    public void CutSelection() => graphView.Undo.ExecuteCommand(new CutCommand(graphView));
    public void DeleteSelection() => graphView.Undo.ExecuteCommand(new DeleteNodeCommand(graphView));

    public override void KeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Delete)
        {
            DeleteSelection();
            evt.StopPropagation();
        }

        if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.C)
        {
            CopySelection();
            evt.StopPropagation();
        }
        else if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.X)
        {
            CutSelection();
            evt.StopPropagation();
        }
    }

    public override void MouseDown(MouseDownEvent evt)
    {
        if (evt.button != (int)MouseButton.LeftMouse)
            return;

        graphView = node.GetFirstAncestorOfType<StateGraphView>();
        if (graphView == null) return;

        var handler = graphView.GraphViewEvent as GraphViewEventHandler;
        if (handler != null)
        {
            if (evt.shiftKey || evt.ctrlKey || evt.commandKey)
                handler.ShiftClickIntent = true;
        }

        if (handler != null)
        {
            if (handler.ShouldClearSelection(node))
                graphView.ClearSelection();

            handler.SelectionBoxJustFinished = false;
            handler.ShiftClickIntent = false;
        }

        if (!graphView.IsSelected(node))
        {
            graphView.AddToSelection(node);
        }

        node.ToggleSelection(true);

        evt.StopPropagation();
    }

}



