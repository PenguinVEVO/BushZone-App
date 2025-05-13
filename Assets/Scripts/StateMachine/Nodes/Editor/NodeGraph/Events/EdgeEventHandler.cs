using FrameLabs.Utilities.NodeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class EdgeEventHandler: EventHandler
{
    private EdgeElement edge;
    private const float ClickThreshold = 15f;
    private StateGraphView graphView;

    private readonly ProximityHighlighter highlighter;

    public EdgeEventHandler( EdgeElement edgeElement)
    {
        edge = edgeElement;

        if ( edge == null)
        {
            Debug.LogError( "[EdgeEventHandler] edge is null" );
        }

        edge.focusable = true;

        highlighter = new ProximityHighlighter( edge, GetDistanceToCurve, ClickThreshold, "edge-hovered" );
    }

    private float GetDistanceToCurve(Vector2 worldMousePos)
    {
        Vector2 localMousePos = edge.WorldToLocal(worldMousePos);
        float closestT = edge.BezierCurve.GetClosetPoint(localMousePos);
        Vector3 closestPoint = edge.BezierCurve.GetPositionAlongCurve(closestT);
        Vector2 closestPoint2D = new Vector2(closestPoint.x, closestPoint.y);

        return (closestPoint2D - localMousePos).magnitude;        
    }

    public override void KeyDown( KeyDownEvent evt )
    {
        if (evt.keyCode == KeyCode.Delete)
        {
            graphView = edge.GetFirstAncestorOfType<StateGraphView>();

            var command = new DeleteLinkCommand(graphView, edge);

            graphView.Undo.ExecuteCommand(command);
        }
    }
  
    public override void MouseDown(MouseDownEvent evt)
    {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            var graphView = edge.GetFirstAncestorOfType<StateGraphView>();

            if (GetDistanceToCurve(evt.mousePosition) < ClickThreshold)
            {
                graphView?.ClearSelection();
                graphView?.AddToSelection(edge);
                edge.ToggleSelection(true);
            }
            else
            {
                edge.ToggleSelection(false);
            }

            evt.StopPropagation();
        }
    }

}
