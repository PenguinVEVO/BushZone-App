using FrameLabs.Utilities.NodeEditor;
using System.Collections.Generic;
using System;
using System.Linq;

public class DeleteLinkCommand : Command
{
    private readonly StateGraphView graphView;
    private readonly List<EdgeElement> edgesToDelete;

    public DeleteLinkCommand(StateGraphView graphView, EdgeElement edge)
    {
        this.graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
        if (edge == null) throw new ArgumentNullException(nameof(edge));
        edgesToDelete = new List<EdgeElement> { edge };
    }

    public DeleteLinkCommand(StateGraphView graphView, IEnumerable<EdgeElement> edges)
    {
        this.graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
        edgesToDelete = edges?.Where(e => e != null).ToList() ?? new List<EdgeElement>();
    }

    public override void Execute()
    {
        foreach (var edge in edgesToDelete)
        {
            string edgeKey = $"{edge.OutputNodeID}-{edge.InputNodeID}";

            edge.UnregisterFromPins();

            if (edge.OutputConnector is PortElement outPort)
                outPort.Disconnect(edge);
            else if (edge.OutputConnector is PinAnchor outPin)
                outPin.Disconnect(edge);

            if (edge.InputConnector is PortElement inPort)
                inPort.Disconnect(edge);
            else if (edge.InputConnector is PinAnchor inPin)
                inPin.Disconnect(edge);

            graphView.RemoveEdge(edge);
            graphView.Linker?.UnregisterEdge(edgeKey);

            if (edge.OutputPort != null)
                edge.OutputPort.ParentView?.OnEdgeRemoved(edge);

            if (edge.InputPort != null)
                edge.InputPort.ParentView?.OnEdgeRemoved(edge);
        }

        GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
    }

    public override void Undo()
    {
        foreach (var edge in edgesToDelete)
        {
            string edgeKey = $"{edge.OutputNodeID}-{edge.InputNodeID}";

            if (edge.OutputConnector is PortElement outPort)
                outPort.Connect(edge);
            else if (edge.OutputConnector is PinAnchor outPin)
                outPin.Connect(edge);

            if (edge.InputConnector is PortElement inPort)
                inPort.Connect(edge);
            else if (edge.InputConnector is PinAnchor inPin)
                inPin.Connect(edge);

            graphView.AddEdge(edge);

            edge.RegisterWithPins();

            graphView.Linker?.RegisterEdge(edgeKey, edge);

            if (edge.OutputPort != null)
                edge.OutputPort.ParentView?.OnEdgeCreated(edge);

            if (edge.InputPort != null)
                edge.InputPort.ParentView?.OnEdgeCreated(edge);

            edge.UpdateEdge();
        }

        GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
    }

    public override void Redo() => Execute();
}
