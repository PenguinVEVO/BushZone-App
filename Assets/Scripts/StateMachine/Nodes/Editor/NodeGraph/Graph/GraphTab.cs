using FrameLabs.NodeEditor.Utilities;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class GraphTab
    {
        public string Name { get; private set; }
        public Button TabButton { get; private set; }
        public VisualElement ContentContainer { get; private set; }
        public StateGraphView GraphView { get; set; }
        public NodeGraphData GraphData { get; set; }

        private Action<string> switchTabCallback;
        private Action<GraphTab> closeTabCallback;
        private Action<GraphTab, string> renameTabCallback;

        private int lastHashCode;
        protected EditorMessageBox messageBox;


        public int GetLastSerializedHash() => lastHashCode;

        public GraphTab(
            string name,
            Action<string> switchCallback,
            Action<GraphTab> closeCallback,
            Action<GraphTab, string> renameCallback )
        {
            Name = name;
            switchTabCallback = switchCallback;
            closeTabCallback = closeCallback;
            renameTabCallback = renameCallback;

            GraphView = new StateGraphView(this) { name = name };
            GraphView.style.position = Position.Absolute;
            GraphView.style.left = 0;
            GraphView.style.right = 0;
            GraphView.style.top = 0;
            GraphView.style.bottom = 0;
            GraphView.style.overflow = Overflow.Hidden;

            ContentContainer = new VisualElement
            {
                name = $"{Name}_TabRoot"
            };

            ContentContainer.AddToClassList("graph-tab-root");
            ContentContainer.style.flexGrow = 1;
            ContentContainer.style.display = DisplayStyle.None;

            ContentContainer.Add(GraphView);
            messageBox = new EditorMessageBox();
            ContentContainer.Add(messageBox);

            TabButton = new Button( () => switchTabCallback( Name ) ) { text = name };
            TabButton.AddToClassList( "graph-tab-button" );

            AddContextMenu();
        }

        public void RegiserCallBacks( Action<string> switchTo, 
            Action<GraphTab>closeTab, Action<GraphTab, string> renameTab)
        {
            switchTabCallback = switchTo;
            closeTabCallback = closeTab;
            renameTabCallback = renameTab;
        }


        public void ShowEditorMessage(string msg, MessageCategory category = MessageCategory.Info, float duration = 3f)
        {
            messageBox?.ShowMessage(msg, duration, category);
        }


        public NodeGraphData SerializeGraphData()
        {
            var data = GraphSerialization.Instance.SerializeGraph(GraphView);
            data.EditorVersion = NodeGraphEditor.EDITOR_VERSION;
            data.name = Name;
            lastHashCode = GraphEditorUtility.Instance.ComputeHash(data);
            return data;
        }

        public void SetActive( bool isActive )
        {
            if( isActive )
            {
                TabButton.AddToClassList( "graph-tab-button--selected" );
                ContentContainer.style.display = DisplayStyle.Flex;
            }
            else
            {
                TabButton.RemoveFromClassList( "graph-tab-button--selected" );
                ContentContainer.style.display = DisplayStyle.None;
            }
        }

        public void Rename( string newName )
        {
            Name = newName;
            TabButton.text = newName;
            GraphView.name = newName;
        }

        private void AddContextMenu()
        {
            TabButton.RegisterCallback<ContextClickEvent>( evt =>
            {
                ShowContextMenu( evt );
                evt.StopPropagation();
            } );
        }

        private void ShowContextMenu( ContextClickEvent evt )
        {
            var menu = new GenericMenu();
            menu.AddItem( new GUIContent( "Close Tab" ), false, () => closeTabCallback?.Invoke( this ) );
            menu.AddItem(new GUIContent("Rename Tab"), false, () => StartInlineRename());
            menu.AddItem(new GUIContent("Close Without Saving"), false, () => CloseWithoutSaving());
            menu.AddItem(new GUIContent("Delete Graph from Disk"), false, () => DeleteGraphFromDisk());


            menu.ShowAsContext();
        }

        private void DeleteGraphFromDisk()
        {
            // Ask user for confirmation
            bool confirmed = EditorUtility.DisplayDialog(
                "Delete Graph",
                $"Are you sure you want to permanently delete the graph asset for '{Name}'?",
                "Delete",
                "Cancel"
            );

            if (!confirmed) return;

            // Request deletion from session manager
            GraphSessionManager.Instance.DeleteTabAndAsset(Name);

            // Close tab visually
            closeTabCallback?.Invoke(this);
        }


        private void CloseWithoutSaving()
        {
            GraphSessionManager.Instance.UnregisterTab(Name);
            closeTabCallback?.Invoke(this);
        }

        private void StartInlineRename()
        {
            TextField renameField = new TextField { value = Name };
            TabButton.Clear();
            TabButton.Add( renameField );
            renameField.Focus();

            renameField.RegisterCallback<FocusOutEvent>( evt => EndInlineRename( renameField ) );
            renameField.RegisterCallback<KeyDownEvent>( evt =>
            {
                if( evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter )
                {
                    EndInlineRename( renameField );
                    evt.StopPropagation();
                }
            } );
        }

        private void EndInlineRename(TextField renameField)
        {
            string newName = renameField.value.Trim();
            if (string.IsNullOrEmpty(newName)) newName = Name;

            TabButton.Clear();
            TabButton.text = newName;

            if (newName != Name)
            {
                string oldName = Name;
                Name = newName;
                TabButton.text = newName;
                GraphView.name = newName;

                renameTabCallback?.Invoke(this, newName);

                GraphSessionManager.Instance.RenameTabAsset(oldName, newName, GraphData);
            }
        }

    }
}