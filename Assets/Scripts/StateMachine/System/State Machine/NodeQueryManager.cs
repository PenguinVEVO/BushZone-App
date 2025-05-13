using FrameLabs.AI.Extension;
using FrameLabs.AI.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.AI.System
{
    /// <summary>
    /// Manages queries related to nodes within the state machine's behavior tree.
    /// Includes methods for retrieving all nodes, finding specific nodes, and searching for nodes by type.
    /// </summary>
    internal class NodeQueryManager
    {
        // Reference to the state machine that owns this manager
        private readonly StateMachine stateMachine;
        private Dictionary<Node, StateMachine> nodeStateMachineMap = new Dictionary<Node, StateMachine>();

        private List<Node> cachedNodes = new List<Node>();


        /// <summary>
        /// Constructor for the NodeQueryManager. Requires a reference to the parent state machine.
        /// </summary>
        /// <param name="stateMachine">The parent state machine that owns this manager.</param>
        public NodeQueryManager(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        internal void GenerateMachineLookUpTable()
        {
            for (int i=0; i < StateMachine.allStateMachines.Count; i++ )
            {
                StateMachine sm = StateMachine.allStateMachines[ i ];

                List<Node> allNodes = GetAllNodes( sm.treeInstance );

                for (int j =0; j < allNodes.Count; j++ )
                {
                    Node node = allNodes[ j ];

                    nodeStateMachineMap[ node ] = sm;
                }
            }            
        }

        /// <summary>
        /// Retrieves all nodes in the behavior tree starting from the root node.
        /// </summary>
        /// <param name="rootNode">The root node from which to start retrieving all nodes.</param>
        /// <returns>A list of all nodes in the behavior tree.</returns>
        internal List<Node> GetAllNodes(Node rootNode)
        {
            // If cached nodes are available, return them immediately.
            if (cachedNodes != null && cachedNodes.Count > 0)
            {
                return cachedNodes;
            }

            // Return an empty list if the root node is null
            if (rootNode == null)
            {
                return new List<Node>();
            }

            List<Node> allNodes = new List<Node>();
            HashSet<Node> visitedNodes = new HashSet<Node>();
            Stack<Node> stack = new Stack<Node>();

            stack.Push(rootNode);

            while (stack.Count > 0)
            {
                Node currentNode = stack.Pop();

                // Skip already visited nodes
                if (visitedNodes.Contains(currentNode)) continue;

                // Mark the current node as visited
                visitedNodes.Add(currentNode);
                allNodes.Add(currentNode);

                // Add child nodes to the stack
                for (int i = currentNode.childNodes.Count - 1; i >= 0; i--)
                {
                    Node childNode = currentNode.childNodes[i];
                    if (childNode != null)
                    {
                        stack.Push(childNode);
                    }
                }
            }

            // Cache the result for future use
            cachedNodes = allNodes;

            return cachedNodes;
        }


        internal void ClearNodeCache()
        {
            cachedNodes = null;
        }

        internal int GetNodeCount()
        {
            return cachedNodes.Count;
        }

        /// <summary>
        /// Finds the state machine that contains a specific node within its behavior tree.
        /// </summary>
        /// <param name="node">The node to find within the state machine's behavior tree.</param>
        /// <returns>The state machine that contains the specified node, or null if not found.</returns>
        public StateMachine GetStateMachineByNode(Node node)
        {
           
            if (nodeStateMachineMap.TryGetValue(node, out var stateMachine))
            {
                return stateMachine;
            }

            Debug.LogWarning( $"Node not found in any state machine: {node.Id}" );
            return null;
        }

        /// <summary>
        /// Iteratively searches for a specific node within a tree using a stack for depth-first traversal.
        /// </summary>
        /// <param name="parentNode">The node to start the search from.</param>
        /// <param name="targetNode">The node to search for.</param>
        /// <returns>The target node if found, otherwise null.</returns>
        private static Node FindNodeInTree(Node parentNode, Node targetNode)
        {
            // Early exit if the root is null or already matches the target
            if (parentNode == null || parentNode == targetNode)
            {
                return parentNode;
            }

            // Use a stack to iterate through the tree (depth-first)
            Stack<Node> stack = new();
            stack.Push(parentNode);

            while (stack.Count > 0)
            {
                Node currentNode = stack.Pop();

                // Return the current node if it matches the target
                if (currentNode == targetNode)
                {
                    return currentNode;
                }

                // Push child nodes onto the stack in reverse order to maintain left-to-right traversal
                for (int i = currentNode.childNodes.Count - 1; i >= 0; i--)
                {
                    if (currentNode.childNodes[i] != null)
                    {
                        stack.Push(currentNode.childNodes[i]);
                    }
                }
            }

            return null; // Target node not found
        }

        /// <summary>
        /// Searches for the first node of a specified type within the behavior tree.
        /// </summary>
        /// <typeparam name="T">The type of node to search for.</typeparam>
        /// <returns>The first node of type <typeparamref name="T"/> if found, otherwise null.</returns>
        public T FindNodeByType<T>() where T : Node
        {
            List<Node> allNodes = GetAllNodes(stateMachine.treeInstance);

            // Iterate through all nodes using a for loop to find the node of the specified type
            for (int i = 0; i < allNodes.Count; i++)
            {
                if (allNodes[i] is T specificNode)
                {
                    return specificNode;
                }
            }

            return null; // Node of type T not found
        }


        /// <summary>
        /// Tracks a node in the state machine's breadcrumb trails.
        /// </summary>
        public void TrackNodeInBreadcrumbs(Node node)
        {
            if (!stateMachine.visitedNodes.Contains(node.Id))
            {
                stateMachine.breadCrumbTrails.Enqueue(node);  // Add to queue
                stateMachine.visitedNodes.Add(node.Id);       // Mark this node as visited
            }
        }

        public bool QueryBreadcrumbs(Node node)
        {
            return stateMachine.breadCrumbTrails.Contains(node);
        }

        /// <summary>
        /// Recursively searches for a specific node within a behavior tree.
        /// </summary>
        /// <param name="parentNode">The starting node of the search.</param>
        /// <param name="targetNode">The node to search for.</param>
        /// <returns>The target node if found, otherwise null.</returns>
        internal Node FindNode(Node parentNode, Node targetNode)
        {
            // If the current node is the target node, return it
            if (parentNode == targetNode)
            {
                return parentNode;
            }

            // Recursively search through the child nodes using a for loop
            for (int i = 0; i < parentNode.childNodes.Count; i++)
            {
                Node childNode = parentNode.childNodes[i];
                Node foundNode = FindNode(childNode, targetNode);
                if (foundNode != null)
                {
                    return foundNode;
                }
            }

            return null; // Target node not found
        }


        /// <summary>
        /// Retrieves the first ActionNode associated with a specific action type.
        /// </summary>
        /// <typeparam name="T">The type of action to search for.</typeparam>
        /// <returns>The first ExecuteActionNode associated with the action type, or null if not found.</returns>
        public ActionNode GetNodeByExtension<T>() where T : StateExtension
        {
            List<Node> allNodes = GetAllNodes(stateMachine.treeInstance);

            // Use a for loop to avoid the overhead of foreach
            for (int i = 0; i < allNodes.Count; i++)
            {
                Node node = allNodes[i];

                if (node is ActionNode actionNode && actionNode.stateExtension is T)
                {
                    return actionNode;
                }
            }

            return null; // No matching action node found
        }


    }

}