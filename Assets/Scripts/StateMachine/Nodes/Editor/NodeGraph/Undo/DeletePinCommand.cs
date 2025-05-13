using FrameLabs.Utilities.NodeEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;

public class DeletePinCommand : Command
{
    private readonly StateGraphView graphView;

    private List<ReroutePinElement> deletedPins;
    private List<EdgeElement> deletedEdges;

    public DeletePinCommand(StateGraphView graphView)
    {
        this.graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
    }

    public override void Execute()
    {
        var selectedPins = graphView.Selection.OfType<ReroutePinElement>().ToList();
        if (selectedPins.Count == 0) return;

        deletedPins = new List<ReroutePinElement>();
        deletedEdges = new List<EdgeElement>();
        var seenEdges = new HashSet<EdgeElement>();

        foreach (var pin in selectedPins)
        {
            foreach (var edge in pin.GetConnectedEdges().ToList())
            {
                if (seenEdges.Add(edge))
                {
                    graphView.RemoveEdge(edge);
                    edge.Disconnect();
                    deletedEdges.Add(edge);
                }
            }

            graphView.RemoveVisualGraphElement(pin);
            deletedPins.Add(pin);
        }

        GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
    }
    public override void Undo()
    {
        graphView.AddElements(deletedPins.Cast<GraphElement>());
        graphView.ReconnectEdges(deletedEdges);

        GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
    }

    public override void Redo()
    {
        foreach (var edge in deletedEdges)
        {
            graphView.RemoveEdge(edge);
            edge.Disconnect();
        }

        foreach (var pin in deletedPins)
        {
            graphView.RemoveVisualGraphElement(pin);
        }

        GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
    }
}
