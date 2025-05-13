using System;

namespace FrameLabs.Utilities.NodeEditor
{
    public class CreateLinkCommand : Command
    {
        private EdgeElement edge;
        private readonly IEdgeConnector output;
        private readonly IEdgeConnector input;
        private readonly StateGraphView graphView;
        private readonly NodeLinker linker;

        private string edgeKey;

        public EdgeElement Edge => edge;

        public CreateLinkCommand(IEdgeConnector output, IEdgeConnector input, StateGraphView graphView)
        {
            this.output = output ?? throw new ArgumentNullException(nameof(output));
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
            this.linker = graphView.Linker ?? throw new InvalidOperationException("GraphView must have a valid NodeLinker.");
        }

        public override void Execute()
        {
            if (edge != null) return;

            edge = new EdgeElement(output, input);
            edgeKey = Guid.NewGuid().ToString();

            Connect(output);
            Connect(input);

            graphView.AddEdge(edge);
            edge.UpdateEdge();
            linker.RegisterEdge(edgeKey, edge);

            output.TryNotifyCreated(edge);
            input.TryNotifyCreated(edge);

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        public override void Undo()
        {
            if (edge == null) return;

            Disconnect(output);
            Disconnect(input);

            graphView.RemoveEdge(edge);
            if (!string.IsNullOrEmpty(edgeKey))
                linker.UnregisterEdge(edgeKey);

            output.TryNotifyRemoved(edge);
            input.TryNotifyRemoved(edge);

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        public override void Redo()
        {
            if (edge == null)
            {
                Execute();
                return;
            }

            Connect(output);
            Connect(input);

            graphView.AddEdge(edge);
            linker.RegisterEdge(edgeKey, edge);

            output.TryNotifyCreated(edge);
            input.TryNotifyCreated(edge);

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        private void Connect(IEdgeConnector connector)
        {
            switch (connector)
            {
                case PortElement port:
                    port.Connect(edge);
                    break;
                case PinAnchor pin:
                    pin.Connect(edge);
                    pin.ParentPin?.AddEdge(edge);
                    break;
            }
        }

        private void Disconnect(IEdgeConnector connector)
        {
            switch (connector)
            {
                case PortElement port:
                    port.Disconnect(edge);
                    break;
                case PinAnchor pin:
                    pin.Disconnect(edge);
                    pin.ParentPin?.RemoveEdge(edge);
                    break;
            }
        }
    }

}