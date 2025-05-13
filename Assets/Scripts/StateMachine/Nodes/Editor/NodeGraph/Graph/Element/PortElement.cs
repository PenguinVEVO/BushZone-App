
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using UnityEditor;


namespace FrameLabs.Utilities.NodeEditor
{
    public enum PortDirection
    {
        Input,
        Output
    }

    public enum PortCapacity
    {
        Single,
        Multi
    }

    public class PortElement : GraphElement, IEdgeConnector
    {
        private static readonly StyleColor InputPortColor = new StyleColor( new Color( 0, 122 / 255f, 255 / 255f ) );
        private static readonly StyleColor OutputPortColor = new StyleColor( new Color( 1f, 165 / 255f, 0f ) );

        private PortEventHandler inputHandler;
        private VisualElement connectorBox;
        private VisualElement connectorCap;
        private Label connectorText;

        public NodeView ParentView { get; private set; }
        public PortDirection Direction { get; private set; }
        public PortCapacity Capacity { get; private set; }
        public Type PortType { get; private set; }
        public EdgeElement ConnectedEdge 
        { 
            get {  return connectedEdge; } 
        }

        private EdgeElement connectedEdge; 
        private HashSet<EdgeElement> inputEdges = new HashSet<EdgeElement>();

        public IEnumerable<EdgeElement> Connections => Direction == PortDirection.Output ?
            connectedEdge != null ? new List<EdgeElement> { connectedEdge } : new List<EdgeElement>() :
            inputEdges;

        public bool Connected => Direction == PortDirection.Output ? connectedEdge != null : inputEdges.Count > 0;

        public bool AllowMultiDrag { get; set; } = true;
        public bool Highlight { get; set; } = false;

        public event Action<EdgeElement> OnEdgeConnected;
        public event Action<EdgeElement> OnEdgeDisconnected;

        public int BranchIndex { get; private set; }
        public string PortName { get; private set; }

        public VisualElement ConnectorCap => connectorCap;

        public Vector2 GetAnchorWorldPosition()
        {
            var graphView = GetGraphView();

            if (ConnectorCap != null && graphView != null)
            {
                return graphView.WorldToLocal(ConnectorCap.worldBound.center);
            }

            return worldBound.center;
        }

        public PortElement(NodeView parentNode, string portName, PortDirection direction,
                           PortCapacity capacity, int branchIndex = 0, Type portType = null)
            : this(parentNode, portName, direction, capacity, branchIndex, portType, isNodeView: true)
        {
            if (parentNode == null)
                throw new ArgumentNullException(nameof(parentNode));
        }

        public PortElement(VisualElement parent, string portName, PortDirection direction,
                           PortCapacity capacity, int branchIndex = 0, Type portType = null)
            : this(parent, portName, direction, capacity, branchIndex, portType, isNodeView: false) { }


        private PortElement(VisualElement parent, string portName, PortDirection direction,
                            PortCapacity capacity, int branchIndex, Type portType, bool isNodeView)
        {
            if (parent == null)
                throw new ArgumentNullException(nameof(parent));

            ParentView = parent as NodeView;
            PortName = portName ?? throw new ArgumentNullException(nameof(portName));
            Direction = direction;
            Capacity = capacity;
            BranchIndex = branchIndex;
            PortType = portType ?? typeof(object);

            AddToClassList("port-element");
            AddToClassList(direction.ToString().ToLowerInvariant());

            if (!string.IsNullOrEmpty(portName))
                tooltip = portName;

            if (isNodeView)
                LoadStyleSheet("PortStyle.uss");

            CreateUIElements();
            CreateCaps();
            RefreshElement();

            if (isNodeView)
                schedule.Execute(RegisterEvents);
        }

        private void LoadStyleSheet(string fileName)
        {
            string basePath = Application.dataPath;
            string filePath = Utility.Instance.FindFileInPath(basePath, fileName);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(filePath);
            if (styleSheet != null)
            {
                styleSheets.Add(styleSheet);
            }
            else
            {
                Debug.LogWarning($"[PortElement] {fileName} not found at path: {filePath}");
            }
        }

        private void CreateCaps()
        {
            var cap = new VisualElement { name = "port-cap" };
            cap.AddToClassList("port-cap");
            Add(cap);
        }

        private void RegisterEvents()
        {
            inputHandler = new PortEventHandler( this );
            InputManager.Instance.RegisterElement( connectorCap );
            InputManager.Instance.SetEventHandler( connectorCap, inputHandler );
        }

        /// <summary>
        /// Creates the UI elements for the port.
        /// </summary>
        private void CreateUIElements()
        {
            connectorBox = new VisualElement { name = "connector-box" };
            connectorBox.AddToClassList( "connector-box" );

            connectorText = new Label( $"{PortName} [{BranchIndex}]" );

            connectorCap = new VisualElement { name = "connector-cap" };

            if( Direction == PortDirection.Input )
            {
                connectorText.AddToClassList( "connector-text-input" );
                connectorCap.AddToClassList( "connector-cap-input" );
                connectorCap.style.backgroundColor = InputPortColor;

                connectorBox.style.flexDirection = FlexDirection.Row;
                connectorBox.Add( connectorCap );
                connectorBox.Add( connectorText );
            }
            else
            {
                connectorText.AddToClassList( "connector-text-output" );
                connectorCap.AddToClassList( "connector-cap-output" );
                connectorCap.style.backgroundColor = OutputPortColor;

                connectorBox.style.flexDirection = FlexDirection.Row;
                connectorBox.Add( connectorText );
                connectorBox.Add( connectorCap );
            }

            Add( connectorBox );
        }

        /// <summary>
        /// Connects an edge to the port.
        /// </summary>
        public void Connect( EdgeElement edge )
        {
            var graphView = GetGraphView();

            if ( Direction == PortDirection.Output )
            {
                if( connectedEdge != null )
                {
                    graphView.OwningTab.ShowEditorMessage( $"[PortElement] Output port {PortName} already has a connection. Disconnecting current connection" , MessageCategory.Warning);
                    connectedEdge.Disconnect();
                }

                connectedEdge = edge;
            }
            else
            {
                inputEdges.Add( edge );
            }

            RefreshElement();
            OnEdgeConnected?.Invoke( edge );
        }

        /// <summary>
        /// Connects this port to another port, ensuring proper constraints.
        /// </summary>
        public EdgeElement ConnectTo( PortElement targetPort )
        {
            var graphView = GetGraphView();

            if (targetPort == null)            
                graphView.OwningTab.ShowEditorMessage($"{nameof(targetPort)}, Target port cannot be null.", MessageCategory.Warning);
            
            if( Direction == targetPort.Direction )
                graphView.OwningTab.ShowEditorMessage("Cannot connect two ports with the same direction.", MessageCategory.Warning);

            if( targetPort.Connected && targetPort.Direction == PortDirection.Output )
                graphView.OwningTab.ShowEditorMessage($"Target output port {targetPort.PortName} already has a connection.", MessageCategory.Warning);

            var edge = new EdgeElement( this, targetPort );

            Connect( edge );
            targetPort.Connect( edge );

            return edge;
        }

        /// <summary>
        /// Disconnects a specific edge from this port.
        /// </summary>
        public void Disconnect( EdgeElement edge )
        {
            if( Direction == PortDirection.Output )
            {
                if( connectedEdge == edge )
                    connectedEdge = null;
            }
            else
            {
                inputEdges.Remove( edge );
            }

            RefreshElement();
            OnEdgeDisconnected?.Invoke( edge );
        }

        /// <summary>
        /// Disconnects all edges from this port.
        /// </summary>
        public void DisconnectAll()
        {
            if( Direction == PortDirection.Output )
            {
                if( connectedEdge != null )
                {
                    EdgeElement edge = connectedEdge;
                    connectedEdge = null;
                    edge.Disconnect();
                }
            }
            else
            {
                foreach( var edge in inputEdges.ToList() )
                {
                    Disconnect( edge );
                }
            }
        }

        /// <summary>
        /// Updates the port UI.
        /// </summary>
        public void RefreshElement()
        {
            connectorCap.MarkDirtyRepaint();
        }

        /// <summary>
        /// Gets the GraphView for this port.
        /// </summary>
        public StateGraphView GetGraphView()
        {
            return ParentView?.GetFirstAncestorOfType<StateGraphView>();
        }      
    }

}

