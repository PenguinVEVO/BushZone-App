using FrameLabs.Utilities.NodeEditor.Layer;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class ContextMenuAction
    {
        public string Label;
        public Action Callback;
        public bool IsSeparatorBefore;
        public Texture Icon;

        public ContextMenuAction(string label, Action callback, bool isSeparatorBefore = false, Texture icon = null)
        {
            Label = label;
            Callback = callback;
            IsSeparatorBefore = isSeparatorBefore;
            Icon = icon;
        }
    }

    public class ToolbarManager
    {
        private static ToolbarManager instance = new();
        
        private readonly Dictionary<string, Toolbar> toolStrips = new Dictionary<string, Toolbar>();


        public static ToolbarManager Instance => instance;

        private ToolbarManager() { }

        public Toolbar CreateToolBar(string Name, string Layer = "Toolbar")
        {
            if( toolStrips.ContainsKey( Name ) )
                return toolStrips[ Name ];

            VisualElement layer = LayerManager.Instance.GetLayer(Layer);

            if ( layer == null) 
            {
                Debug.LogError( $"{Layer} , not found unable to create tool bar" );
                return null;
            }

            Toolbar toolbar = new()
            {
                name = Name
            };

            layer.Add( toolbar );

            toolStrips.Add( Name, toolbar );

            return toolbar;
        }

        public Button CreateContextMenuButton(string text, List<ContextMenuAction> actions)
        {
            Button button = new Button { text = text };

            button.clicked += () =>
            {
                var menu = new GenericMenu();
                string lastRoot = null;

                foreach (var action in actions)
                {
                    string currentRoot = GetMenuPathRoot(action.Label);

                    if (action.IsSeparatorBefore && currentRoot != lastRoot)
                    {
                        menu.AddItem(new GUIContent(" "), false, () => { });
                    }

                    var content = action.Icon != null
                        ? new GUIContent(action.Label, action.Icon)
                        : new GUIContent(action.Label);

                    menu.AddItem(content, false, () => action.Callback?.Invoke());
                    lastRoot = currentRoot;
                }

                menu.DropDown(button.worldBound);
            };

            return button;
        }

        private string GetMenuPathRoot(string label)
        {
            int lastSlash = label.LastIndexOf('/');
            return lastSlash >= 0 ? label.Substring(0, lastSlash + 1) : "";
        }


        public Toolbar GetToolbar( string Name ) 
        {
            if( toolStrips.ContainsKey( Name ) )
            {
                return toolStrips[ Name ];
            }

            return null;
        }

        public void Clear()
        {
            toolStrips.Clear();
        }

        public Button CreateToolElement( string text, Action OnClick )
        {
            Button button = new Button( OnClick )
            {
                text = text
            };

            return button;           
        }        
    }

}