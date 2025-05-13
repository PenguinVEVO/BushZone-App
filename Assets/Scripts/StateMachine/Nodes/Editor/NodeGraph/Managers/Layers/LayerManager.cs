using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor.Layer
{
    public class LayerManager
    {
        private static readonly LayerManager instance = new();
        public static LayerManager Instance => instance;

        private VisualElement root;
        private LayerNode rootNode;
        private Dictionary<string, LayerNode> flatLookup = new();

        public void Initialize(VisualElement rootElement)
        {
            root = rootElement ?? throw new ArgumentNullException(nameof(rootElement));
            rootNode = new LayerNode("Root", null, root, null);
            flatLookup.Clear();
        }

        public void AddLayer(string path, LayerConfig config)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Invalid layer path.");
            if (config == null) config = new LayerConfig();

            string[] parts = path.Split('/');
            LayerNode current = rootNode;
            string currentPath = "";

            foreach (string part in parts)
            {
                currentPath = string.IsNullOrEmpty(currentPath) ? part : currentPath + "/" + part;

                if (!flatLookup.TryGetValue(currentPath, out var node))
                {
                    var newElement = new VisualElement { name = part };
                    
                    newElement.style.position = config.Position;
                    newElement.style.overflow = config.Overflow;

                    if (config.FlexGrow)
                        newElement.style.flexGrow = 1;

                    foreach (var cls in config.StyleClasses)
                        newElement.AddToClassList(cls);

                    if (config.Order.HasValue)
                    {
                        int clampedIndex = Mathf.Clamp(config.Order.Value, 0, current.Element.childCount);
                        current.Element.Insert(clampedIndex, newElement);
                    }
                    else
                    {
                        current.Element.Add(newElement);
                    }

                    node = new LayerNode(part, current, newElement, config.Order);
                    current.Children.Add(node);
                    flatLookup[currentPath] = node;
                }

                current = node;
            }
        }

        public List<VisualElement> GetSubLayers(string path, bool recursive = false)
        {
            if (!flatLookup.TryGetValue(path, out var parentNode))
                return new List<VisualElement>();

            var result = new List<VisualElement>();

            void CollectChildren(LayerNode node)
            {
                foreach (var child in node.Children)
                {
                    result.Add(child.Element);

                    if (recursive)
                        CollectChildren(child);
                }
            }

            CollectChildren(parentNode);
            return result;
        }


        public VisualElement GetLayer(string path)
        {
            return flatLookup.TryGetValue(path, out var node) ? node.Element : null;
        }

        public void ClearLayers()
        {
            foreach (var node in flatLookup.Values)
            {
                if (node.Element != null && node.Element != root)
                {
                    node.Element.RemoveFromHierarchy();
                }
            }

            flatLookup.Clear();
            rootNode.Children.Clear();
        }
    }


}