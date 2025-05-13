using FrameLabs.Utilities.NodeEditor;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;


public class PinElementEventHandler : EventHandler
{
    private readonly ReroutePinElement pin;
    private StateGraphView graphView;

    public PinElementEventHandler(ReroutePinElement reroutePin)
    {
        pin = reroutePin;

        pin.schedule.Execute(() =>
        {
            graphView = VisualElementUtility.FindInAncestors<StateGraphView>(pin, returnLast: false);

            if (graphView != null)
            {
                pin.AddManipulator(new UnifiedElementDragger<NodeElement>(
                    () => graphView.Selection.OfType<NodeElement>().ToList()));   
            }

        });
    }

    public void DeleteSelection()
    {
        graphView.Undo.ExecuteCommand(new DeletePinCommand(graphView));
    }

    public override void MouseDown(MouseDownEvent evt)
    {
        if (graphView == null || evt.button != (int)MouseButton.LeftMouse)
            return;

        var handler = graphView.GraphViewEvent as GraphViewEventHandler;
        if (handler != null)
        {
            if (evt.shiftKey || evt.ctrlKey || evt.commandKey)
                handler.ShiftClickIntent = true;
        }

        if (handler != null)
        {
            if (handler.ShouldClearSelection(pin))
                graphView.ClearSelection();

            handler.SelectionBoxJustFinished = false;
            handler.ShiftClickIntent = false;
        }

        if (!graphView.IsSelected(pin))
        {
            graphView.AddToSelection(pin);
        }

        pin.ToggleSelection(true);

        evt.StopPropagation();
    }

    public override void KeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.Delete && graphView?.IsSelected(pin) == true)
        {
            DeleteSelection();
            evt.StopPropagation();
        }
        else if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.C)
        {
            graphView.Undo.ExecuteCommand(new CopyCommand(graphView));
            evt.StopPropagation();
        }
        else if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.X)
        {
            graphView.Undo.ExecuteCommand(new CutCommand(graphView));
            evt.StopPropagation();
        }
    }
}

