using FrameLabs.AI.Nodes;
using FrameLabs.Utilities.NodeEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class GraphViewEventHandler : EventHandler
{
    private StateGraphView graphView;
    private Vector2 selectionStart;
    private VisualElement selectionBox;
    private Dictionary<NodeElement, Vector2> dragOffsets;
    private bool isDragging;
    private bool selectionBoxJustFinished = false;

    private bool shiftClickIntent = false;


    public void PasteSelection() => graphView.Undo.ExecuteCommand(new PasteCommand(graphView, graphView.MousePosition + new Vector2(10, 10)));

    public GraphViewEventHandler( StateGraphView view)
    {
        graphView = view;
    }
    private void StartBoxSelection( Vector2 startPos )
    {
        if( selectionBox == null )
        {
            selectionBox = new VisualElement();
            selectionBox.name = "selection-box";
            selectionBox.style.position = Position.Absolute;
            selectionBox.pickingMode = PickingMode.Ignore;
            graphView.Add( selectionBox );
        }

        selectionBox.style.left = startPos.x;
        selectionBox.style.top = startPos.y;
        selectionBox.style.width = 0;
        selectionBox.style.height = 0;
        selectionBox.style.display = DisplayStyle.Flex;

        foreach( var node in graphView.nodes.OfType<NodeElement>() )
        {
            node.pickingMode = PickingMode.Ignore;
            node.EnableInteraction( false );
        }
    
        foreach( var edge in graphView.edges.OfType<EdgeElement>() )
            edge.pickingMode = PickingMode.Ignore;

    }

    public bool ShouldClearSelection(NodeElement clickedElement)
    {
        bool isAlreadySelected = graphView.IsSelected(clickedElement);

        if (!ShiftClickIntent && !SelectionBoxJustFinished && !isAlreadySelected)
            return true;

        return false;
    }


    public bool SelectionBoxJustFinished
    {
        get => selectionBoxJustFinished;
        set => selectionBoxJustFinished = value;
    }

    public bool ShiftClickIntent
    {
        get => shiftClickIntent;
        set => shiftClickIntent = value;
    }

    public override void MouseDown(MouseDownEvent evt)
    {
        if (evt.button == (int)MouseButton.LeftMouse)
        {
            var clickedElement = graphView.panel.Pick(evt.mousePosition);

            if (clickedElement is NodeElement || clickedElement is EdgeElement)
                return;


            foreach (var edge in graphView.edges)
            {
                if (edge is EdgeElement element)
                {
                    element.ToggleSelection(false);
                    graphView.RemoveFromSelection(edge);
                }
            }

            foreach (var node in graphView.nodes)
            {
                if (node is NodeElement nodeElement)
                {
                    nodeElement.ToggleSelection(false);
                    graphView.RemoveFromSelection(node);
                }
            }

            foreach ( var graphElement in graphView.visualElements)
            {
                if( graphElement is ReroutePinElement routedElement )
                {
                    routedElement.ToggleSelection( false );
                    graphView.RemoveFromSelection( graphElement );
                }
            }

            selectionStart = graphView.WorldToLocal(evt.mousePosition);
            StartBoxSelection(selectionStart);

            evt.StopPropagation();
        }

        graphView.MousePosition = evt.mousePosition;
    }


    public override void MouseMove(MouseMoveEvent evt)
    {
        if (selectionBox != null && selectionBox.style.display == DisplayStyle.Flex)
        {
            Vector2 current = graphView.WorldToLocal(evt.mousePosition);
            float xMin = Mathf.Min(selectionStart.x, current.x);
            float yMin = Mathf.Min(selectionStart.y, current.y);
            float width = Mathf.Abs(current.x - selectionStart.x);
            float height = Mathf.Abs(current.y - selectionStart.y);

            selectionBox.style.left = xMin;
            selectionBox.style.top = yMin;
            selectionBox.style.width = width;
            selectionBox.style.height = height;

            evt.StopPropagation();
            return;
        }

        if (isDragging && dragOffsets != null && dragOffsets.Count > 0)
        {
            Vector2 currentMouse = graphView.contentContainer.WorldToLocal(evt.mousePosition);
            foreach (var pair in dragOffsets)
            {
                var node = pair.Key;
                Vector2 offset = pair.Value;
                node.SetPosition(new Rect(currentMouse + offset, node.GetPosition().size));
            }

            evt.StopPropagation();
        }

        graphView.Linker?.UpdateLinkPosition(evt.mousePosition);
    }

    public override void MouseUp(MouseUpEvent evt)
    {
        if (selectionBox != null && selectionBox.style.display == DisplayStyle.Flex)
        {
            if (selectionBox.style.display == DisplayStyle.Flex)
            {
                selectionBox.style.display = DisplayStyle.None;

                foreach( var node in graphView.nodes.OfType<NodeElement>() )
                {
                    node.pickingMode = PickingMode.Position;
                    node.EnableInteraction( true );
                }

                foreach( var edge in graphView.edges.OfType<EdgeElement>() )
                    edge.pickingMode = PickingMode.Position;

                foreach( var pin in graphView.visualElements.OfType<ReroutePinElement>() )
                    pin.pickingMode = PickingMode.Position;

                    Rect selectionRect = new Rect(
                    selectionBox.resolvedStyle.left,
                    selectionBox.resolvedStyle.top,
                    selectionBox.resolvedStyle.width,
                    selectionBox.resolvedStyle.height
                );

                foreach (var node in graphView.nodes.OfType<NodeElement>())
                {
                    Rect nodeRect = node.GetPosition();
                    if (selectionRect.Overlaps(nodeRect))
                    {
                        graphView.AddToSelection(node);
                        node.ToggleSelection(true);
                    }
                }

                foreach ( var pinElement in graphView.visualElements.OfType<ReroutePinElement>())
                {
                    Rect pinRect = pinElement.GetPosition();

                    if( selectionRect.Overlaps( pinRect ) )
                    {
                        graphView.AddToSelection( pinElement );
                        pinElement.ToggleSelection( true );
                    }
                }

                selectionBoxJustFinished = true;

                evt.StopPropagation();
            }
        }

        if (isDragging)
        {
            isDragging = false;
            dragOffsets?.Clear();
            graphView.ReleaseMouse();
            evt.StopPropagation();
        }

        graphView.Linker.CancelActiveLink();
    }


    public override void KeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode == KeyCode.F)
            graphView.ResetView();

        if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.Z)
        {
            graphView.Undo.Undo();
            evt.StopPropagation();
        }
        else if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.Y)
        {
            graphView.Undo.Redo();
            evt.StopPropagation();
        }
        else if ((evt.ctrlKey || evt.commandKey) && evt.keyCode == KeyCode.V)
        {
            PasteSelection();
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
        else if (evt.keyCode == KeyCode.Delete)
        {
            graphView.Undo.ExecuteCommand(new DeleteNodeCommand(graphView));
            graphView.Undo.ExecuteCommand( new DeletePinCommand(graphView));
            evt.StopPropagation();
        }
    }


    public void CreatePin()
    {
        var pin = new CreateWidgetCommand<ReroutePinElement>(
                    graphView,
                    graphView.MousePosition,  
                    (view) => new ReroutePinElement(),
                    (pin) => {  });

        graphView.Undo.ExecuteCommand(pin);
    }

    public void CreateExitNode()
    {
        var exitNode = ScriptableObject.CreateInstance<ExitNode>();
        graphView.Undo.ExecuteCommand(new CreateNodeCommand<ExitNodeView>(
            exitNode,
            graphView.MousePosition,
            graphView,
            node => new ExitNodeView((ExitNode)node),
            view => { }));
    }

    public void CreateStartNode()
    {
        var startNode = ScriptableObject.CreateInstance<StartNode>();
        graphView.Undo.ExecuteCommand(new CreateNodeCommand<StartNodeView>(
            startNode,
            graphView.MousePosition,
            graphView,
            node => new StartNodeView((StartNode)node),
            view => { }));
    }

    public void CreateActionNode()
    {
        var actionNode = ScriptableObject.CreateInstance<ActionNode>();
        graphView.Undo.ExecuteCommand(new CreateNodeCommand<ActionNodeView>(
            actionNode,
            graphView.MousePosition,
            graphView,
            node => new ActionNodeView((ActionNode)node),
            view => { }));
    }
}
