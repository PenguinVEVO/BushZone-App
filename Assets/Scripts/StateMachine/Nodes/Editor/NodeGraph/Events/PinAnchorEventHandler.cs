using FrameLabs.Utilities.NodeEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PinAnchorEventHandler : EventHandler
{
    private readonly PinAnchor pin;
    private StateGraphView graphView;
    private NodeLinker linker;


    public PinAnchorEventHandler(PinAnchor anchor)
    {
        this.pin = anchor;

        pin.schedule.Execute(() =>
        {
            graphView = VisualElementUtility.FindInAncestors<StateGraphView>(pin, returnLast: false);
            linker = graphView?.Linker;
        });
    }

    public override void MouseDown(MouseDownEvent evt)
    {
        if (evt.button != (int)MouseButton.LeftMouse) return;
        if (graphView == null || linker == null) return;

        if (!linker.IsLinking)
        {
            linker.StartLinkFromAnchor(pin, evt.mousePosition);
            evt.StopPropagation();
        }
    }

    public override void MouseUp(MouseUpEvent evt)
    {
        if (graphView == null || linker == null) return;

        VisualElement picked = graphView.panel.Pick(evt.mousePosition);
        if (picked == null)
        {
            linker.CancelActiveLink();
            return;
        }

        // First check for port
        PortElement port = picked.GetFirstAncestorOfType<PortElement>();
        if (port != null && port.Direction == PortDirection.Input)
        {
            Debug.Log("[PinInput] Finalizing to input port");
            linker.FinalizeLink(port);
            evt.StopPropagation();
            return;
        }

        // Fallback: check for another pin
        PinAnchor anchor = picked.GetFirstAncestorOfType<PinAnchor>();
        if (anchor != null)
        {
            Debug.Log("[PinInput] Finalizing to another reroute pin");
            linker.FinalizeLink(anchor);
            evt.StopPropagation();
            return;
        }

        Debug.LogWarning("[PinInput] Finalize failed — no valid target");
        linker.CancelActiveLink();
        evt.StopPropagation();
    }


}
