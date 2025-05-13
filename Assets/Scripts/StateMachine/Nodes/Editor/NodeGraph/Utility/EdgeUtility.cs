

namespace FrameLabs.Utilities.NodeEditor
{
    public static class EdgeConntectorExtension
    {
        /// <summary>
        /// Invokes OnEdgeCreated if the connector is a PortElement with a NodeView parent.
        /// </summary>
        public static void TryNotifyCreated(this IEdgeConnector connector, EdgeElement edge)
        {
            if (connector is PortElement port)
            {
                port.ParentView?.OnEdgeCreated(edge);
            }
        }

        /// <summary>
        /// Invokes OnEdgeRemoved if the connector is a PortElement with a NodeView parent.
        /// </summary>
        public static void TryNotifyRemoved(this IEdgeConnector connector, EdgeElement edge)
        {
            if (connector is PortElement port)
            {
                port.ParentView?.OnEdgeRemoved(edge);
            }
        }
    }
}