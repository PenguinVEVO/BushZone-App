using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class ReroutePinElement : GraphWidget
    {
        public string ID { get; private set; }
        public const string TypeID = "ReroutePin";

        private VisualElement dotVisual;
        private PinAnchor leftAnchor;
        private PinAnchor rightAnchor;
        
        private PinAnchor inputAnchor;
        private PinAnchor outputAnchor;

        private readonly List<EdgeElement> connectedEdges = new();

        public PinAnchor GetLeftAnchor() => leftAnchor;
        public PinAnchor GetRightAnchor() => rightAnchor;

        public VisualElement GetInputAnchor() => inputAnchor ?? leftAnchor;
        public VisualElement GetOutputAnchor() => outputAnchor ?? rightAnchor;

        public ReroutePinElement(string id = null) : base("Reroute Pin", id)
        {
            ID = id ?? ElementID;
            typeID = TypeID;

            string styleSheetPath = $"{Application.dataPath}";
            string file = Utility.Instance.FindFileInPath(styleSheetPath, "PinStyle.uss");

            StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(file);
            if (styleSheet != null)
                styleSheets.Add(styleSheet);
            else
                Debug.LogError($"[ReroutePin] Could not load stylesheet at: {file}");

            style.position = Position.Absolute;
            AddToClassList("reroute-pin");

            dotVisual = new VisualElement();
            dotVisual.AddToClassList("reroute-dot");
            Add(dotVisual);

            leftAnchor = new PinAnchor(this);
            leftAnchor.AddToClassList("anchor-left");

            rightAnchor = new PinAnchor(this);
            rightAnchor.AddToClassList("anchor-right");

            Add(leftAnchor);
            Add(rightAnchor);

            InputManager.Instance.RegisterElement(this);
            InputManager.Instance.SetEventHandler(this, new PinElementEventHandler(this));

            capabilities |= Capabilities.Movable | Capabilities.Selectable | Capabilities.Deletable;

            RegisterCallback<GeometryChangedEvent>( evt => 
            {
                 schedule.Execute( UpdateConnectedEdges).ExecuteLater(1); 
            });
        }

        public bool IsInputLeft()
        {
            return inputAnchor == leftAnchor;
        }

        public bool IsInputRight()
        {
            return inputAnchor == rightAnchor;
        }

        public override void ToggleSelection(bool selected)
        {
            dotVisual.EnableInClassList("pin-selected", selected);
            MarkDirtyRepaint();
        }

        public List<EdgeElement> GetConnectedEdges() => connectedEdges;

        public void AddEdge(EdgeElement edge)
        {
            if (!connectedEdges.Contains(edge))
                connectedEdges.Add(edge);

            if (inputAnchor == null || outputAnchor == null)
            {
                if (edge.InputConnector is PinAnchor input && input.ParentPin == this)
                {
                    AssignDirection(input);
                }
                else if (edge.OutputConnector is PinAnchor output && output.ParentPin == this)
                {
                    AssignDirection((output == leftAnchor) ? rightAnchor : leftAnchor);
                }
            }
        }

        public void RemoveEdge(EdgeElement edge)
        {
            connectedEdges.Remove(edge);

            if (connectedEdges.Count == 0)
            {
                inputAnchor?.ResetVisualStyle();
                outputAnchor?.ResetVisualStyle();

                inputAnchor = null;
                outputAnchor = null;
            }
        }

        public void UpdateConnectedEdges()
        {
            foreach (var edge in connectedEdges)
            {
                if(edge == null) continue;

                edge.UpdateEdge();                
            }
        }

        public void AssignDirection(PinAnchor input)
        {
            inputAnchor = input;
            outputAnchor = (input == leftAnchor) ? rightAnchor : leftAnchor;

            inputAnchor.SetAsInput();
            outputAnchor.SetAsOutput();
        }

        public void SetAnchorLayoutFromFlow(Vector2 start, Vector2 end)
        {
            bool leftToRight = start.x < end.x;

            if (leftToRight)
                AssignDirection(leftAnchor);
            else
                AssignDirection(rightAnchor);
        }

        public override object CreateCopyData(bool copyId)
        {
            return GraphSerialization.Instance.CopyPinData(this, copyId);
        }

    }


}
