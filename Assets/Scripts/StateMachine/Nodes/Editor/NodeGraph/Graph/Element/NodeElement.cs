using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;


namespace FrameLabs.Utilities.NodeEditor
{

    public abstract class NodeElement : GraphElement
    {
        protected string nodeID;
        protected VisualElement bodyContainer;

        public string NodeID
        {
            get => nodeID;
            set => nodeID = value;
        }

        public NodeElement(string nodeTitle = "Node", string nodeId = null)
        {
            nodeID = nodeId ?? Guid.NewGuid().ToString();
            focusable = true;

            if (ClassListContains("graphElement"))
                RemoveFromClassList("graphElement");

            AddToClassList("node-element");
            style.position = Position.Absolute;
            SetPosition(new Rect(0, 0, 200, 200));

            // Core body container
            bodyContainer = new VisualElement { name = "body-container" };
            bodyContainer.AddToClassList("node-body");
            hierarchy.Add(bodyContainer);

            // Hook USS
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);

            // Derived classes can extend structure
            SetupUI(nodeTitle);

            // Optional default caps
            capabilities |= Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable;
        }

        /// <summary>
        /// Allows child classes to define additional visual structure.
        /// </summary>
        protected abstract void SetupUI(string nodeTitle);

        /// <summary>
        /// Respond to resolved custom USS variables (optional override).
        /// </summary>
        protected virtual void OnCustomStyleResolved(CustomStyleResolvedEvent evt) { }

        /// <summary>
        /// Enable/disable pointer interaction (override if needed).
        /// </summary>
        public virtual void EnableInteraction(bool enabled)
        {
            pickingMode = enabled ? PickingMode.Position : PickingMode.Ignore;
            bodyContainer.pickingMode = pickingMode;
        }

        /// <summary>
        /// Toggle selection highlight.
        /// </summary>
        public virtual void ToggleSelection(bool selected)
        {
            EnableInClassList("node-selected", selected);
        }
        
    }


}