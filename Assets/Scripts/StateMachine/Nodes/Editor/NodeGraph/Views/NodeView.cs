using System;
using UnityEngine;
using FrameLabs.AI.Nodes;
using System.Linq;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEditor;

namespace FrameLabs.Utilities.NodeEditor
{
    /// <summary>
    /// Represents a visual node within the graph editor. 
    /// Inherits from NodeElement and provides methods for managing ports, state, and size.
    /// </summary>
    public class NodeView : NodeElement, IGraphElementView, ICopyableElement
    {
        protected Label titleBar;
        protected VisualElement titleContainer;
        protected VisualElement portsContainer;
        protected VisualElement inputContainer;
        protected VisualElement outputContainer;
        protected VisualElement settingsContainer;

        protected readonly List<PortElement> inputPorts = new();
        protected readonly List<PortElement> outputPorts = new();

        public virtual float Height => 150f;
        public virtual float Width => 200f;

        public AI.Nodes.Node NodeData { get; private set; }
        public NodeView ParentView { get; set; }

        public string Title
        {
            get => titleBar?.text ?? string.Empty;
            set
            {
                if (titleBar != null)
                    titleBar.text = value;
            }
        }

        public string ElementID => NodeID;
        public string ElementType => "NodeView";

        public NodeView(Node data, string title = "Node", string id = null)
            : base(title, id)
        {
            NodeData = data ?? throw new ArgumentNullException(nameof(data));

            string styleSheetPath = Utility.Instance.FindFileInPath(Application.dataPath, "NodeStyle.uss");
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);

            if (styleSheet != null)
                styleSheets.Add(styleSheet);
            else
                Debug.LogWarning($"[NodeView] Failed to load NodeStyle.uss at path: {styleSheetPath}");

            AddToClassList("node-element");

            InputManager.Instance.RegisterElement(this);
            InputManager.Instance.SetEventHandler(this, new NodeInputEventHandler(this));

            SetPosition(new Rect(0, 0, Width, Height));
        }

        protected override void SetupUI(string nodeTitle)
        {
            titleContainer = new VisualElement { name = "title-container" };
            titleContainer.AddToClassList("node-title");
            hierarchy.Insert(0, titleContainer);

            titleBar = new Label(nodeTitle) { name = "title-label" };
            titleBar.AddToClassList( "title-label" );
            titleContainer.Add(titleBar);

            portsContainer = new VisualElement { name = "node-ports" };
            portsContainer.AddToClassList("node-ports");
            bodyContainer.Add(portsContainer);

            inputContainer = new VisualElement { name = "input-container" };
            inputContainer.AddToClassList("input-container");
            portsContainer.Add(inputContainer);

            outputContainer = new VisualElement { name = "output-container" };
            outputContainer.AddToClassList("output-container");
            portsContainer.Add(outputContainer);

            settingsContainer = new VisualElement { name = "node-settings" };
            settingsContainer.AddToClassList("node-settings");
            bodyContainer.Add(settingsContainer);
        }

        public override void EnableInteraction(bool enabled)
        {
            var mode = enabled ? PickingMode.Position : PickingMode.Ignore;

            titleContainer.pickingMode = mode;
            bodyContainer.pickingMode = mode;
            portsContainer.pickingMode = mode;
            inputContainer.pickingMode = mode;
            outputContainer.pickingMode = mode;
            settingsContainer.pickingMode = mode;
        }

        public PortElement AddPort(NodeView nodeView, string portName, PortDirection direction, PortCapacity capacity, int branchIndex = 0)
        {
            if (nodeView == null)
                throw new ArgumentNullException(nameof(nodeView));

            var port = new PortElement(nodeView, portName, direction, capacity, branchIndex);
            portsContainer.Add(port);

            if (direction == PortDirection.Input)
            {
                inputContainer.Add(port);
                inputPorts.Add(port);
            }
            else
            {
                outputContainer.Add(port);
                outputPorts.Add(port);
            }

            UpdateNodeElement();
            return port;
        }

        public void AddSettingsProperties(VisualElement element)
        {
            settingsContainer.Add(element);
        }

        protected virtual void UpdateNodeElement()
        {
            int totalPorts = inputPorts.Count + outputPorts.Count;
            float portSpacing = 22f;
            float baseHeight = 95f;

            float settingsHeight = settingsContainer?.childCount > 0 ? 30f : 0f;
            float newHeight = baseHeight + (totalPorts * portSpacing) + settingsHeight;

            var current = GetPosition();
            SetPosition(new Rect(current.x, current.y, resolvedStyle.width, newHeight));
        }

        public IEnumerable<PortElement> GetInputPorts() => inputPorts;
        public IEnumerable<PortElement> GetOutputPorts() => outputPorts;
        public IEnumerable<PortElement> GetAllPorts() => inputPorts.Concat(outputPorts);

        public object CreateCopyData(bool copyId)
        {
            if (NodeData == null)
            {
                Debug.LogWarning($"[NodeView] Cannot copy node {name}: NodeData is missing.");
                return null;
            }
            var newNodeData = GraphSerialization.Instance.CopyNodeData(this, copyId);

            return newNodeData;
        }
    }
}



