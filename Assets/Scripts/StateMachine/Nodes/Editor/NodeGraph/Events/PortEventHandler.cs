using FrameLabs.Utilities.NodeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PortEventHandler : EventHandler
{
    private PortElement port;
    private StateGraphView graphView;
    private NodeLinker linker;

    public PortEventHandler(PortElement portElement)
    {
        port = portElement;
        graphView = portElement.GetGraphView();

        if (graphView == null)
        {
            Debug.LogError("[PortEventHandler] graphView is null. Ensure PortElement is attached to a valid GraphView.");
            return;
        }

        linker = graphView.Linker;

        if (linker == null)
        {
            Debug.LogError("[PortEventHandler] NodeLinker is null. Ensure it is initialized in StateGraphView.");
            return;
        }

        if (!InputManager.Instance.HasEventHandler(port))
        {
            InputManager.Instance.RegisterElement(port);
            InputManager.Instance.SetEventHandler(port, this);
        }
    }

    public override void MouseDown(MouseDownEvent evt)
    {
        if (evt.button != (int)MouseButton.LeftMouse) return;

        Debug.Log($"[PortEventHandler] Clicked on connector cap: {port.PortName}");

        evt.StopPropagation();

        if (port.Direction == PortDirection.Output)
        {
            Debug.Log($"[PortEventHandler] Start dragging from output: {port.PortName}");
            linker.StartLinkFromPort(port, evt.mousePosition);
        }
    }

    public override void MouseUp(MouseUpEvent evt)
    {
        if (graphView == null || linker == null) return;

        var picked = graphView.panel.Pick(evt.mousePosition);
        PortElement targetPort = picked.GetFirstAncestorOfType<PortElement>();

        if (targetPort != null && targetPort.Direction == PortDirection.Input)
        {
            linker.FinalizeLink(targetPort);
            evt.StopPropagation();
            return;
        }

        linker.CancelActiveLink();
    }

}

