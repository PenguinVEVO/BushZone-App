using FrameLabs.Utilities.NodeEditor;
using FrameLabs.Utilities.NodeEditor.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public enum GraphViewRenderMode
{
    Default,
    Edges,
    Nodes,
    Debug
}

public struct NodeGraphViewChanged
{
    public List<GraphElement> elementsToRemove;
    public List<EdgeElement> edgesToCreate;
    public List<GraphElement> movedElements;
    public Vector2 moveDelta;
}

public class NodeGraphView : VisualElement
{
    public delegate NodeGraphViewChanged GraphViewChangedDelegate(NodeGraphViewChanged change);
    public GraphViewChangedDelegate graphViewChanged;

    public readonly List<NodeView> nodes = new();
    public readonly List<EdgeElement> edges = new();
    public readonly List<GraphWidget> visualElements = new(); 

    public event Action<NodeView> OnNodeAdded;
    public event Action<NodeView> OnNodeRemoved;
    public event Action<EdgeElement> OnEdgeCreated;
    public event Action<EdgeElement> OnEdgeRemoved;
    public event Action<GraphWidget> OnVisualElementAdded;
    public event Action<GraphWidget> OnVisualElementRemoved;

    protected VisualElement gridLayer;
    protected VisualElement edgeLayer;
    protected VisualElement nodeLayer;
    protected VisualElement overlayLayer;

    private readonly float minZoom = 1.0f;
    private readonly float zoomFactor = 3.0f;
    private readonly float zoomStep = 0.2f;

    private readonly GraphDragger dragger;
    private readonly GraphZoom zoom;
    public readonly GraphGrid Grid;
    private readonly HashSet<GraphElement> selectedElements = new();
    public IEnumerable<GraphElement> Selection => selectedElements;

    public IEnumerable<GraphWidget> GetSelectedVisuals() => Selection.OfType<GraphWidget>();
    public IEnumerable<NodeView> GetSelectedNodes() => selectedElements.OfType<NodeView>();
    public IEnumerable<EdgeElement> GetSelectedEdges() => selectedElements.OfType<EdgeElement>();

    public float GetZoomScale() => zoom.GetZoomScale();

    public NodeGraphView()
    {
        style.flexGrow = 1;
        style.justifyContent = Justify.Center;
        style.alignItems = Align.Center;

        AddToClassList("custom-graph-view");

        gridLayer = new VisualElement { name = "grid-layer" };
        gridLayer.pickingMode = PickingMode.Ignore;
        gridLayer.style.flexGrow = 1;
        gridLayer.style.flexShrink = 1;

        edgeLayer = new VisualElement { name = "edge-layer" };
        edgeLayer.pickingMode = PickingMode.Ignore;

        nodeLayer = new VisualElement { name = "node-layer" };
        nodeLayer.pickingMode = PickingMode.Ignore;

        overlayLayer = new VisualElement { name = "overlay-layer" };
        overlayLayer.pickingMode = PickingMode.Ignore;

        Add(gridLayer);
        Add(edgeLayer);
        Add(nodeLayer);
        Add(overlayLayer);

        Grid = new GraphGrid();
        Grid.AddToClassList("graph-grid");
        Grid.SendToBack();
        Grid.style.flexGrow = 1;
        Grid.style.flexShrink = 1;
        Grid.focusable = true;
        gridLayer.Add(Grid);

        zoom = new GraphZoom();
        this.AddManipulator(zoom);
        zoom.SetupZoom( minZoom, zoomFactor, zoomStep );


        dragger = new GraphDragger();
        this.AddManipulator(dragger);

        var graphViewRoot = LayerManager.Instance.GetLayer("Workspace/GraphView");

        if (graphViewRoot != null && !Contains(graphViewRoot))
        {
            Add(graphViewRoot);
            graphViewRoot.style.flexGrow = 1;
        }

        RegisterCallback<GeometryChangedEvent>(OnMainContainerChanged);
    }

    private void OnMainContainerChanged(GeometryChangedEvent evt)
    {
        foreach (var edge in edges)
        {
            edge.UpdateEdge();
        }
    }

    public void UpdateAllEdges()
    {
        foreach (var edge in edges)
        {
            edge.UpdateEdge();
        }
    }

    public VisualElement GetOverlayLayer => overlayLayer;
    public VisualElement GetEdgeLayer => edgeLayer;
    public VisualElement GetNodeLayer => nodeLayer;
    public VisualElement GetGridLayer => gridLayer;

    public bool IsSelected(GraphElement element) => selectedElements.Contains(element);

    public void SetLayerOrder(params VisualElement[] orderedLayers)
    {
        for (int i = 0; i < orderedLayers.Length; i++)
        {
            var layer = orderedLayers[i];
            if (layer?.parent == this)
            {
                this.Remove(layer);
                this.Insert(i, layer);
            }
        }
    }

    public void ApplyRenderMode(GraphViewRenderMode mode)
    {
        switch (mode)
        {
            case GraphViewRenderMode.Edges:
                SetLayerOrder(gridLayer, nodeLayer, overlayLayer, edgeLayer);
                break;
            case GraphViewRenderMode.Nodes:
                SetLayerOrder(gridLayer, edgeLayer, overlayLayer, nodeLayer);
                break;
            case GraphViewRenderMode.Debug:
                SetLayerOrder(gridLayer, edgeLayer, nodeLayer, overlayLayer);
                overlayLayer.style.backgroundColor = new Color(0, 0, 0, 0.2f);
                break;
            default:
                SetLayerOrder(gridLayer, edgeLayer, nodeLayer, overlayLayer);
                overlayLayer.style.backgroundColor = Color.clear;
                break;
        }
    }

    public void ToggleSelection(GraphElement element)
    {
        if (IsSelected(element))
            RemoveFromSelection(element);
        else
            AddToSelection(element);
    }

    public void AddNode(NodeView nodeView)
    {
        if (nodes.Contains(nodeView)) return;
        nodes.Add(nodeView);
        nodeLayer.Add(nodeView);
        OnNodeAdded?.Invoke(nodeView);
    }

    public void RemoveNode(NodeView nodeView)
    {
        if (nodeView == null) return;
        nodes.Remove(nodeView);
        nodeView.RemoveFromHierarchy();
        OnNodeRemoved?.Invoke(nodeView);
    }

    public void AddEdge(EdgeElement edge)
    {
        if (edge == null || edges.Contains(edge)) return;
        edges.Add(edge);
        edgeLayer.Add(edge);
        MarkDirtyRepaint();
        OnEdgeCreated?.Invoke(edge);
    }

    public void RemoveEdge(EdgeElement edge)
    {
        if (edge == null || !edges.Remove(edge)) return;
        edge.RemoveFromHierarchy();
        MarkDirtyRepaint();
        OnEdgeRemoved?.Invoke(edge);
    }

    public void AddVisualGraphElement(GraphWidget element)
    {
        if (visualElements.Contains(element)) return;
        visualElements.Add(element);
        nodeLayer.Add(element);
        OnVisualElementAdded?.Invoke(element);
    }

    public void RemoveVisualGraphElement(GraphWidget element)
    {
        if (!visualElements.Contains(element)) return;
        visualElements.Remove(element);
        element.RemoveFromHierarchy();
        OnVisualElementRemoved?.Invoke(element);
    }

    public void AddElements(IEnumerable<GraphElement> elements)
    {
        foreach (var element in elements)
        {
            switch (element)
            {
                case NodeView node:
                    AddNode(node);
                    break;

                case ReroutePinElement pin:
                    AddVisualGraphElement(pin);
                    break;

                case EdgeElement edge:
                    AddEdge(edge);
                    break;

                default:
                    Debug.LogWarning($"[StateGraphView] Unknown element type during Add: {element.GetType().Name}");
                    break;
            }
        }
    }

    public void DeleteElements(IEnumerable<GraphElement> elementsToRemove)
    {
        if (elementsToRemove == null) return;

        var list = elementsToRemove.ToList();

        foreach (var edge in list.OfType<EdgeElement>())
            RemoveEdge(edge);

        foreach (var node in list.OfType<NodeView>())
            RemoveNode(node);

        foreach (var graphElement in list.OfType<GraphWidget>())
            RemoveVisualGraphElement(graphElement);

        foreach (var element in list)
            RemoveFromSelection(element);

        graphViewChanged?.Invoke(new NodeGraphViewChanged { elementsToRemove = list });
    }


    public void ClearGraph()
    {
        foreach (var edge in edges.ToArray())
            RemoveEdge(edge);

        foreach (var node in nodes.ToArray())
            RemoveNode(node);

        foreach (var graphElement in visualElements.ToArray())
            RemoveVisualGraphElement(graphElement);
    }

    public NodeView GetNodeById(string nodeId) => nodes.Find(n => n.NodeID == nodeId);

    public IEnumerable<EdgeElement> GetEdgesConnectedTo(NodeView nodeView)
    {
        foreach (var edge in edges)
        {
            if (edge.OutputPort?.ParentView == nodeView ||
                edge.InputPort?.ParentView == nodeView)
            {
                yield return edge;
            }
        }
    }

    public void AddToSelection(GraphElement element)
    {
        if (element != null && selectedElements.Add(element))
        {
            if (element is NodeElement node)
                node.ToggleSelection(true);
            else if (element is EdgeElement edge)
                edge.ToggleSelection(true);
            else if (element is GraphWidget visualGraphElement)
            {
                visualGraphElement.ToggleSelection(true);
            }

            element.MarkDirtyRepaint();
        }
    }

    public void RemoveFromSelection(GraphElement element)
    {
        if (element != null && selectedElements.Remove(element))
        {
            if (element is NodeElement node)
                node.ToggleSelection(false);
            else if (element is EdgeElement edge)
                edge.ToggleSelection(false);
            else if (element is GraphWidget visualGraphElement)
            {
                visualGraphElement.ToggleSelection(false);
            }

            element.MarkDirtyRepaint();
        }
    }

    public void ResetView()
    {
        zoom.ResetView();
    }

    public void ClearSelection()
    {
        foreach (var element in selectedElements.ToList())
        {
            switch (element)
            {
                case NodeElement node:
                    node.ToggleSelection(false);
                    break;
                case EdgeElement edge:
                    edge.ToggleSelection(false);
                    break;
            }

            element.MarkDirtyRepaint();
        }
        selectedElements.Clear();
    }

    public void SelectAll()
    {
        ClearSelection();
        foreach (var node in nodes) AddToSelection(node);
        foreach (var edge in edges) AddToSelection(edge);
        foreach (var visualElement in visualElements) AddToSelection(visualElement);
    }

    public void DeleteSelection()
    {
        foreach (var element in selectedElements.ToList())
        {
            switch (element)
            {
                case NodeView node:
                    RemoveNode(node);
                    break;
                case EdgeElement edge:
                    RemoveEdge(edge);
                    break;
                case GraphWidget visualGraphElement:
                    RemoveVisualGraphElement(visualGraphElement);
                    break;
            }
        }
        ClearSelection();
    }
}
