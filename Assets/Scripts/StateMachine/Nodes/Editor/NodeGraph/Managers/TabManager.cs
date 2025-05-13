using FrameLabs.Utilities.NodeEditor.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace FrameLabs.Utilities.NodeEditor
{
    public class TabManager
    {
        private static readonly TabManager instance = new();

        private VisualElement tabContainer;
        private readonly Dictionary<string, GraphTab> tabs = new();
        private Stack<string> tabHistory = new();
        private GraphTab activeTab;

        private TabManager() { }

        public static TabManager Instance => instance;

        public string GetActiveTabName() => activeTab?.Name;
        public GraphTab GetActiveTab() => activeTab;

        public IEnumerable<GraphTab> GetAllTabs() => tabs.Values;

        public IEnumerable<string> GetTabNames() => tabs.Keys;

        public void Initialize()
        {
            tabContainer = LayerManager.Instance.GetLayer( "Tabs" );

            if( tabContainer == null )
                throw new ArgumentNullException( nameof( tabContainer ) );

            tabContainer.style.flexDirection = FlexDirection.Row;
            tabContainer.style.alignItems = Align.FlexStart;
        }

        public GraphTab GetTabByName( string name )
        {
            return tabs.TryGetValue( name, out var tab ) ? tab : null;
        }

        public bool HasTab( string tabName )
        {
            return tabs.ContainsKey( tabName );
        }

        public GraphTab CreateTab( string tabName, Action<string> onSwitchToTab, Action<GraphTab> onCloseTab )
        {
            if( tabs.ContainsKey( tabName ) )
            {
                GraphTab tab = tabs[ tabName ];
                tab.RegiserCallBacks(onSwitchToTab, onCloseTab, RenameTab);

                return tab;
            }

            var newTab = new GraphTab( tabName, onSwitchToTab, onCloseTab, RenameTab );

            newTab.TabButton.style.flexShrink = 0;
            newTab.TabButton.style.flexGrow = 0;

            tabs.Add( tabName, newTab );
            tabContainer.Add( newTab.TabButton );

            SwitchToTab( tabName, 
                WorkspaceManager.Instance.ClearWorkspace, 
                name => newTab.GraphView );

            return newTab;
        }

        public void CloseTab(GraphTab tab, Action<string> onSwitchToTab, Action onClearGraphView)
        {
            if (tab == null || !tabs.ContainsKey(tab.Name))
                return;

            tabs.Remove(tab.Name);
            tabContainer.Remove(tab.TabButton);

            // Remove from tab history
            tabHistory = new Stack<string>(tabHistory.Where(t => t != tab.Name));

            bool wasActive = (activeTab == tab);
            if (wasActive)
            {
                activeTab = null;

                while (tabHistory.Count > 0)
                {
                    string previousTab = tabHistory.Pop();
                    if (tabs.ContainsKey(previousTab))
                    {
                        activeTab = tabs[previousTab];
                        onSwitchToTab?.Invoke(previousTab);
                        return;
                    }
                }

                onClearGraphView?.Invoke();
            }
        }

        public void SwitchToTab( string tabName, Action onClearGraphView, Func<string, StateGraphView> getGraphView )
        {
            if( !tabs.ContainsKey( tabName ) )
            {
                Debug.LogError( $"Tab '{tabName}' does not exist." );
                return;
            }

            tabHistory = new Stack<string>(tabHistory.Where(t => t != tabName));
            tabHistory.Push(tabName);

            onClearGraphView?.Invoke();

            activeTab = tabs[ tabName ];

            foreach( var tab in tabs.Values )
            {
                tab.SetActive( tab == activeTab );
            }

            var graphView = getGraphView?.Invoke( tabName );
            if( graphView != null )
            {
                graphView.visible = true;
            }            
        }

        public void ClearAllTabs( Action onClearGraphView )
        {
            foreach( var tab in tabs.Values )
            {
                tabContainer.Remove( tab.TabButton );
            }

            tabs.Clear();
            activeTab = null;
            onClearGraphView?.Invoke();
        }

        public void ClearAllTabs()
        {
            foreach( var tab in tabs.Values )
            {
                tabContainer.Remove( tab.TabButton );
            }

            tabs.Clear();
            activeTab = null;
        }

        public List<string> GetTabHistory()
        {
            return tabHistory.Reverse().ToList();
        }

        public void SetTabHistory( IEnumerable<string> history )
        {
            tabHistory.Clear();
            foreach( var tabName in history.Reverse() )
            {
                if( tabs.ContainsKey( tabName ) )
                    tabHistory.Push( tabName );
            }
        }

        private void RenameTab( GraphTab tab, string newName )
        {
            if( tabs.ContainsKey( tab.Name ) )
            {
                tabs.Remove( tab.Name );
                tab.Rename( newName );
                tabs.Add( newName, tab );

                if( activeTab == tab )
                {
                    activeTab = tab;
                }
            }
        }
    }

}