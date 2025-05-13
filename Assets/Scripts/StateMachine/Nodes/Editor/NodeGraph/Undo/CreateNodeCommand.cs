using System;
using UnityEngine;
using UnityEngine.UIElements;


namespace FrameLabs.Utilities.NodeEditor
{
    public class CreateNodeCommand<T> : Command where T : NodeView
    {
        private T nodeView;
        private readonly AI.Nodes.Node stateNode;
        private readonly StateGraphView graphView;
        private readonly Vector2 position;

        private readonly Func<AI.Nodes.Node, T> createNodeView;
        private readonly Action<T> onNodeCreated;

        private bool created = false;

        public CreateNodeCommand(
            AI.Nodes.Node stateNode,
            Vector2 position,
            StateGraphView graphView,
            Func<AI.Nodes.Node, T> createNodeView,
            Action<T> onNodeCreated)
        {
            this.stateNode = stateNode ?? throw new ArgumentNullException(nameof(stateNode));
            this.position = position;
            this.graphView = graphView ?? throw new ArgumentNullException(nameof(graphView));
            this.createNodeView = createNodeView ?? throw new ArgumentNullException(nameof(createNodeView));
            this.onNodeCreated = onNodeCreated;
        }

        public override void Execute()
        {
            if (nodeView == null)
            {
                nodeView = createNodeView(stateNode);
                created = true;
            }

            graphView.AddNode(nodeView);
            SetNodePosition(nodeView, position);

            if (created)
            {
                onNodeCreated?.Invoke(nodeView);
                GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
            }
        }

        public override void Undo()
        {
            if (nodeView != null)
            {
                graphView.RemoveNode(nodeView);
            }

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        public override void Redo()
        {
            if (nodeView != null)
            {
                graphView.AddNode(nodeView);
                SetNodePosition(nodeView, position);
            }

            GraphChangeNotifier.Instance.MarkDirty(graphView.OwningTab.Name);
        }

        private void SetNodePosition(NodeView node, Vector2 worldPosition)
        {
            if (node == null) return;

            Vector2 localPosition = graphView.contentContainer.WorldToLocal(worldPosition);
            var width = node.resolvedStyle.width > 0 ? node.resolvedStyle.width : node.Width;
            var height = node.resolvedStyle.height > 0 ? node.resolvedStyle.height : node.Height;

            node.SetPosition(new Rect(localPosition, new Vector2(width, height)));
        }
    }
}
