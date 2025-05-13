using System;
using UnityEditor;
using FrameLabs.AI.Nodes;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using FrameLabs.AI.Extension;
using System.Linq;

namespace FrameLabs.Utilities.NodeEditor
{
    public class ActionNodeView : NodeView
    {
        private ActionNode actionNode;
        private int selectedExtensionTypeIndex = 0;
        private string[] extensionDisplayNames;
        private string[] extensionTypeNames;

        private ObjectField stateExtensionField;
        private PopupField<string> extensionTypeDropdown;
        private Button createExtensionButton;


        public override float Height => 195;
        public override float Width => 250;


        public ActionNodeView(ActionNode node, string id = null, int defaultSelection = 0) : base(node, "Action Node", id)
        {
            actionNode = node ?? throw new ArgumentNullException(nameof(node));

            // Customize the title container
            titleContainer.style.backgroundColor = new Color(0.82f, 0.58f, 0.059f);

            var titleLabel = this.Q<Label>("title-label");
            if (titleLabel != null)
            {
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.fontSize = 16;
                titleLabel.style.color = Color.black;
            }

            LoadExtensions();

            if (actionNode.stateExtension != null)
            {
                string extensionTypeName = actionNode.stateExtension.GetType().AssemblyQualifiedName;
                int index = Array.IndexOf(extensionTypeNames, extensionTypeName);

                if (index >= 0)
                {
                    selectedExtensionTypeIndex = index;
                }

                Title = actionNode.stateExtension.name;
            }
            else
            {
                selectedExtensionTypeIndex = defaultSelection;
            }

            // Initialize UI components
            AddSettingsProperties(CreateExecuteOnceToggle());
            AddSettingsProperties(CreateStateExtensionField());
            AddSettingsProperties(CreateExtensionSelectionDropdown());
            AddSettingsProperties(CreateNewExtensionButton());

            CreateInputPort();
            CreateOutputPorts();

        }


        private Toggle CreateExecuteOnceToggle()
        {
            var executeOnceToggle = new Toggle("Execute Once")
            {
                value = actionNode.executeOnce
            };

            executeOnceToggle.RegisterValueChangedCallback(evt =>
            {
                actionNode.executeOnce = evt.newValue;
            });

            return executeOnceToggle;
        }

        private void LoadExtensions()
        {
            extensionDisplayNames = Utility.Instance.GetAssembly<StateExtension>();
            extensionTypeNames = Utility.Instance.GetAssembly<StateExtension>(true);

            if (actionNode.stateExtension != null)
            {
                string currentTypeName = actionNode.stateExtension.GetType().AssemblyQualifiedName;
                int index = Array.IndexOf(extensionTypeNames, currentTypeName);

                if (index >= 0)
                {
                    selectedExtensionTypeIndex = index;
                }
                else
                {
                    Debug.LogWarning($"[ActionNodeView] StateExtension type '{actionNode.stateExtension.GetType().Name}' not found in type list.");
                }
            }
        }

        private ObjectField CreateStateExtensionField()
        {
            stateExtensionField = new ObjectField("State Extension")
            {
                objectType = typeof(StateExtension),
                value = actionNode.stateExtension
            };

            stateExtensionField.RegisterValueChangedCallback(evt =>
            {

                StateExtension stateExtension = (StateExtension)evt.newValue;
                actionNode.stateExtension = stateExtension;

                if (stateExtension != null)
                {
                    string typeName = stateExtension.GetType().AssemblyQualifiedName;
                    int newIndex = Array.IndexOf(extensionTypeNames, typeName);
                    if (newIndex >= 0)
                    {
                        selectedExtensionTypeIndex = newIndex;
                        extensionTypeDropdown.SetValueWithoutNotify(extensionDisplayNames[newIndex]);
                    }
                    else
                    {
                        Debug.LogWarning($"[ActionNodeView] Could not find dropdown index for {typeName}");
                    }
                }

                ClearOutputPorts(); 
                CreateOutputPorts();

                if( stateExtension != null )
                {
                    Title = stateExtension.name;
                }
                else
                {
                    Title = "Action Node";
                }
            });

            return stateExtensionField;
        }

        private PopupField<string> CreateExtensionSelectionDropdown()
        {
            var extensionList = extensionDisplayNames.ToList();

            extensionTypeDropdown = new PopupField<string>(
                extensionList,
                selectedExtensionTypeIndex
            );

            extensionTypeDropdown.RegisterValueChangedCallback(evt =>
            {
                selectedExtensionTypeIndex = extensionList.IndexOf(evt.newValue);
            });

            return extensionTypeDropdown;
        }

        private Button CreateNewExtensionButton()
        {
            createExtensionButton = new Button( CreateNewExtension ) { text = "Create New Extension" };

            return createExtensionButton;
        }

        private void CreateNewExtension()
        {
            if (extensionTypeNames.Length == 0)
                return;

            string selectedTypeName = extensionTypeNames[selectedExtensionTypeIndex];
            Type selectedType = Type.GetType(selectedTypeName);

            if (selectedType != null)
            {
                var newExtension = ScriptableObject.CreateInstance(selectedType) as StateExtension;
                string path = EditorUtility.SaveFilePanelInProject("Save Extension", "NewExtension", "asset", "Please enter a file name to save the extension to");

                if (path.Length > 0)
                {
                    AssetDatabase.CreateAsset(newExtension, path);
                    AssetDatabase.SaveAssets();
                    actionNode.stateExtension = newExtension;

                    stateExtensionField.value = newExtension;
                    LoadExtensions();
                }
            }
            else
            {
                Debug.LogError("Selected extension type could not be found.");
            }
        }

        private void CreateInputPort()
        {
            var inputPort = AddPort(this, "Input", PortDirection.Input, PortCapacity.Multi, 0);

            inputPort.OnEdgeConnected += OnEdgeCreated;
            inputPort.OnEdgeDisconnected += OnEdgeRemoved;

            inputPort.RefreshElement();
        }

        private void CreateOutputPorts()
        {
            if (actionNode.stateExtension?.ExitCodes != null)
            {
                foreach (var exitCode in actionNode.stateExtension.ExitCodes)
                {
                    if (exitCode.Value >= 0)
                    {
                        var output = AddPort(this, exitCode.Key, PortDirection.Output, PortCapacity.Single, exitCode.Value);
                        output.OnEdgeConnected += OnEdgeCreated;
                        output.OnEdgeDisconnected += OnEdgeRemoved;

                        output.RefreshElement();
                    }
                }
            }
        }

        /// <summary>
        /// Clears only output ports without affecting input ports.
        /// </summary>
        private void ClearOutputPorts()
        {
            foreach (var port in GetOutputPorts())
            {
                port.DisconnectAll();
                port.RemoveFromHierarchy(); // Removes from the UI without clearing the whole container
            }

            outputPorts.Clear(); // Only clears the output list, not the input list
        }

        public override void EnableInteraction( bool value )
        {
            base.EnableInteraction( value );

            if( ! value )
            {
                stateExtensionField.pickingMode = PickingMode.Ignore;
                createExtensionButton.pickingMode = PickingMode.Ignore;
                extensionTypeDropdown.pickingMode = PickingMode.Ignore;
            }
            else
            {
                stateExtensionField.pickingMode = PickingMode.Position;
                createExtensionButton.pickingMode= PickingMode.Position;
                extensionTypeDropdown.pickingMode= PickingMode.Position;
            }
        }

        public int CurrentSelectedIndex
        {
            get { return selectedExtensionTypeIndex; }
            set {  selectedExtensionTypeIndex = value; }
        }

        /// <summary>
        /// Called when an edge is connected to a port.
        /// </summary>
        public void OnEdgeCreated(EdgeElement edge)
        {
            if (edge.OutputPort == null || edge.InputPort == null)
                return;

            var childView = edge.InputPort.ParentView;

            if (childView != null)
            {
                int branchIndex = edge.OutputPort.BranchIndex;
                actionNode.MapExitCodeToChild(branchIndex, childView.NodeData);
            }
        }

        /// <summary>
        /// Called when an edge is removed from a port.
        /// </summary>
        public void OnEdgeRemoved(EdgeElement edge)
        {
            if (edge.OutputPort == null)
                return;

            int branchIndex = edge.OutputPort.BranchIndex;
            actionNode.RemoveExitCodeEntry(branchIndex);
        }
    }

}