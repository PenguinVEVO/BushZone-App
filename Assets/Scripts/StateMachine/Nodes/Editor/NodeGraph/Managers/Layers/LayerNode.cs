using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor.Layer
{
    public class LayerConfig
    {
        public int? Order;
        public StyleEnum<Position> Position = new(UnityEngine.UIElements.Position.Absolute);
        public StyleEnum<Overflow> Overflow = new(UnityEngine.UIElements.Overflow.Visible);
        public bool FlexGrow = true;
        public string[] StyleClasses = Array.Empty<string>();
    }

    public class LayerNode
    {
        public string Name;
        public VisualElement Element;
        LayerNode Parent;
        public List<LayerNode> Children = new();
        public int? Order;

        public string FullPath => Parent == null ? Name : $"{Parent.FullPath}/{Name}";

        public LayerNode(string name, LayerNode parent, VisualElement element, int? order)
        {
            Name = name;
            Parent = parent;
            Element = element;
            Order = order;
        }
    }
}