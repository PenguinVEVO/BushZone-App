using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class EdgeElement : GraphElement
    {
        public PortElement OutputPort { get; private set; }
        public PortElement InputPort { get; private set; }

        public string OutputNodeID { get; private set; }
        public string InputNodeID { get; private set; }

        public bool IsFloating { get; private set; }
        public Vector2 FloatingEndPosition { get; private set; }

        private readonly StateGraphView graphView;
        private BezierCurve bezierCurve;

        private IEdgeConnector outputAnchor;
        private IEdgeConnector inputAnchor;
        public IEdgeConnector OutputConnector => outputAnchor;
        public IEdgeConnector InputConnector => inputAnchor;

        private Color edgeStrokeColor = Color.green;
        private float edgeStrokeWidth = 3f;


        public EdgeElement( IEdgeConnector output, IEdgeConnector input = null, bool isTemporary = false )
        {
            if (ClassListContains("graphElement"))
                RemoveFromClassList("graphElement");

            style.borderTopWidth = 0;
            style.borderBottomWidth = 0;
            style.borderLeftWidth = 0;
            style.borderRightWidth = 0;

            RegisterCallback<CustomStyleResolvedEvent>(evt =>
            {
                StyleUtility.TryGetCustomStyle(evt, "--stroke-color", ref edgeStrokeColor);
                StyleUtility.TryGetCustomStyle(evt, "--stroke-width", ref edgeStrokeWidth);
            });

            if( output == null )
                throw new ArgumentNullException( nameof( output ), "Output connector cannot be null." );

            outputAnchor = output;
            inputAnchor = input;
            IsFloating = ( input == null );

            // Capture PortElements if available
            OutputPort = output as PortElement;
            InputPort = input as PortElement;

            if( OutputPort != null )
            {
                OutputNodeID = OutputPort.ParentView?.NodeID;
                graphView = OutputPort.GetFirstAncestorOfType<StateGraphView>();
                RegisterMovementCallback( OutputPort );
            }

            if( InputPort != null )
            {
                InputNodeID = InputPort.ParentView?.NodeID;
                RegisterMovementCallback( InputPort );
            }

            LoadEdgeStyles();

            AddToClassList( "edge-element" );

            if( isTemporary == true )
            {
                pickingMode = PickingMode.Ignore;
                AddToClassList("edge-floating");
            }
            else
            {
                pickingMode = PickingMode.Position;

                var edgeEvent = new EdgeEventHandler(this);
                InputManager.Instance.RegisterElement(this);
                InputManager.Instance.SetEventHandler(this, edgeEvent);
            }

            bezierCurve = new BezierCurve( Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero, 0.1f );
            generateVisualContent += OnGenerateVisualContent;
        }

        public BezierCurve BezierCurve => bezierCurve;

        public void SetConnectector(IEdgeConnector output, IEdgeConnector input)
        {
            outputAnchor = output;
            inputAnchor = input;
        }

        private void LoadEdgeStyles()
        {
            string styleSheetPath = Application.dataPath;
            string file = Utility.Instance.FindFileInPath(styleSheetPath, "EdgeLink.uss");

            if (!string.IsNullOrEmpty(file))
            {
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(file);
                if (styleSheet != null)
                {
                    styleSheets.Add(styleSheet);
                }
                else
                {
                    Debug.LogWarning($"EdgeLink.uss was found but could not be loaded: {file}");
                }
            }
            else
            {
                Debug.LogWarning("EdgeLink.uss not found! Ensure the file exists in the project.");
            }
        }

        private void RegisterMovementCallback(VisualElement target)
        {
            if (target != null)
            {
                target.RegisterCallback<GeometryChangedEvent>(_ => UpdateEdge());
            }
        }

        private void RegisterMovementCallback(PortElement port)
        {
            if (port?.ParentView is VisualElement ve)
            {
                RegisterMovementCallback(ve);
            }
        }

        private void OnGenerateVisualContent(MeshGenerationContext context)
        {
            if (bezierCurve.CurvePoints.Count < 2)
                return;

            var painter = context.painter2D;

            bool isFloating = ClassListContains("edge-floating");
            bool isSelected = ClassListContains("edge-selected");
            bool isHovered = ClassListContains("edge-hovered");
            
            painter.lineWidth = edgeStrokeWidth;
            painter.strokeColor = edgeStrokeColor;

            if (isFloating)
            {
                VisualElementUtility.DrawDashedLine(painter, bezierCurve.CurvePoints, dashLength: 6f, gapLength: 3f);
            }
            else
            {
                painter.BeginPath();
                painter.MoveTo(bezierCurve.CurvePoints[0]);
                for (int i = 1; i < bezierCurve.CurvePoints.Count; i++)
                    painter.LineTo(bezierCurve.CurvePoints[i]);
                painter.Stroke();
            }
        }

        public void SetInput(IEdgeConnector connector)
        {
            inputAnchor = connector;
            IsFloating = false;

            if (connector is PortElement port)
            {
                InputPort = port;
                InputNodeID = port.ParentView?.NodeID;
                RegisterMovementCallback(port);
            }
            else
            {
                InputPort = null;
                InputNodeID = null;
            }

            UpdateEdge();
            MarkDirtyRepaint();
        }


        public void RegisterWithPins()
        {
            if (OutputConnector is VisualElement outputElement)
            {
                var outPin = outputElement.GetFirstAncestorOfType<ReroutePinElement>();
                outPin?.AddEdge(this);
            }

            if (InputConnector is VisualElement inputElement)
            {
                var inPin = inputElement.GetFirstAncestorOfType<ReroutePinElement>();
                inPin?.AddEdge(this);
            }
        }

        public void UnregisterFromPins()
        {
            if (OutputConnector is VisualElement outVE)
            {
                var outPin = outVE.GetFirstAncestorOfType<ReroutePinElement>();
                outPin?.RemoveEdge(this);
            }

            if (InputConnector is VisualElement inVE)
            {
                var inPin = inVE.GetFirstAncestorOfType<ReroutePinElement>();
                inPin?.RemoveEdge(this);
            }
        }

        public void UpdateFloatingEnd(Vector2 position, VisualElement graphView)
        {
            FloatingEndPosition = graphView.WorldToLocal(position);
            IsFloating = true;
            UpdateEdge();
        }

        public void UpdateEdge()
        {
            if( outputAnchor == null ) return;

            Vector2 worldStart = outputAnchor.GetAnchorWorldPosition();
            Vector2 worldEnd = ( IsFloating || inputAnchor == null )
                ? FloatingEndPosition
                : inputAnchor.GetAnchorWorldPosition();

            if( worldStart == worldEnd ) return;

            Vector2 delta = worldEnd - worldStart;

            Vector2 controlPoint1;
            Vector2 controlPoint2;

            // Distance thresholds
            const float dominantAxisThreshold = 40f;

            // Preferred direction, horizontal unless very vertical
            bool isHorizontalPreferred = Mathf.Abs( delta.x ) > dominantAxisThreshold || Mathf.Abs( delta.x ) > Mathf.Abs( delta.y );


            // Smart orthogonal bend (horizontal or vertical)
            if( isHorizontalPreferred )
            {
                // Primary flow: horizontal
                float midX = ( worldStart.x + worldEnd.x ) * 0.5f;

                controlPoint1 = new Vector2( midX, worldStart.y );
                controlPoint2 = new Vector2( midX, worldEnd.y );
            }
            else
            {
                // Primary flow: vertical
                float midY = ( worldStart.y + worldEnd.y ) * 0.5f;

                controlPoint1 = new Vector2( worldStart.x, midY );
                controlPoint2 = new Vector2( worldEnd.x, midY );
            }

            bezierCurve.SetCurve( worldStart, controlPoint1, controlPoint2, worldEnd, 0.1f );

            // Bounding box setup
            float minX = Mathf.Min( worldStart.x, worldEnd.x, controlPoint1.x, controlPoint2.x );
            float maxX = Mathf.Max( worldStart.x, worldEnd.x, controlPoint1.x, controlPoint2.x );
            float minY = Mathf.Min( worldStart.y, worldEnd.y, controlPoint1.y, controlPoint2.y );
            float maxY = Mathf.Max( worldStart.y, worldEnd.y, controlPoint1.y, controlPoint2.y );

            float padding = 5f;
            Rect boundingBox = new Rect(
                minX - padding,
                minY - padding,
                ( maxX - minX ) + ( padding * 2 ),
                ( maxY - minY ) + ( padding * 2 )
            );

            SetPosition( boundingBox );

            // Apply curve in local space
            Vector2 localStart = worldStart - boundingBox.position;
            Vector2 localEnd = worldEnd - boundingBox.position;
            Vector2 localControl1 = controlPoint1 - boundingBox.position;
            Vector2 localControl2 = controlPoint2 - boundingBox.position;

            bezierCurve.SetCurve( localStart, localControl1, localControl2, localEnd, 0.1f );
            MarkDirtyRepaint();
        }



        public void ToggleSelection(bool value)
        {
            EnableInClassList("edge-selected", value);
            MarkDirtyRepaint();
        }

        public void Disconnect()
        {
            UnregisterFromPins();

            OutputPort?.Disconnect(this);
            InputPort?.Disconnect(this);
            parent?.Remove(this);
        }
    }
}
