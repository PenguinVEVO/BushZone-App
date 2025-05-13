using FrameLabs.AI.Nodes;


namespace FrameLabs.AI.System
{
    /// <summary>
    /// Manages the navigation and traversal between nodes in the behavior tree.
    /// Handles finding the next, previous, and parent nodes, and shuffling node priorities.
    /// In this version, navigation is based on dynamic branching in a hierarchical model.
    /// </summary>
    internal class NodeNavigationManager
    {
        // Reference to the state machine this manager belongs to
        private readonly StateMachine stateMachine;

        /// <summary>
        /// Constructor for the NodeNavigationManager. Requires a reference to the parent state machine.
        /// </summary>
        /// <param name="stateMachine">The parent state machine that owns this manager.</param>
        public NodeNavigationManager( StateMachine stateMachine )
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// Retrieves the next node in the current branch of the behavior tree.
        /// This method now accounts for hierarchical decision-making based on node states.
        /// </summary>
        /// <returns>The next node in the branch, or null if no next node is found.</returns>
        public Node GetNextNode()
        {
            if( stateMachine.currentNode != null )
            {
                return GetNextSiblingOrMoveUp( stateMachine.currentNode );
            }

            return null;
        }

        /// <summary>
        /// Retrieves the next sibling node of the current node, or moves up the hierarchy to find the next node.
        /// </summary>
        /// <param name="currentNode">The current node in the behavior tree.</param>
        /// <returns>The next node to execute, or null if no next node is found.</returns>
        private Node GetNextSiblingOrMoveUp( Node currentNode )
        {
            Node parentNode = FindParentNode( stateMachine.treeInstance, currentNode );

            if( parentNode != null )
            {
                int currentIndex = parentNode.childNodes.IndexOf( currentNode );

                // Check if there is a next sibling
                if( currentIndex < parentNode.childNodes.Count - 1 )
                {
                    return parentNode.childNodes[ currentIndex + 1 ];  // Return the next sibling node
                }
                else
                {
                    // No next sibling, move up the hierarchy
                    return GetNextSiblingOrMoveUp( parentNode );
                }
            }

            return null;  // Reached the top of the tree with no next node
        }

        /// <summary>
        /// Moves the current node to the next node in the current branch.
        /// If the next node is null, it resets the tree or moves up in the hierarchy.
        /// </summary>
        public void MoveToNextNodeInBranch()
        {
            Node nextNode = GetNextNode();

            if( nextNode != null )
            {
                stateMachine.currentNode = nextNode;

                // Call Start() on the next node if it is initialized but hasn't started
                if( nextNode.State == NodeState.Initialized )
                {
                    nextNode.Start();
                }

                nextNode.Execute(); // Execute the next node after starting
            }
            else
            {
                // If no next node is found, reset the tree (or move up in the hierarchy)
                stateMachine.ResetTree();
            }
        }

        /// <summary>
        /// Finds the previous node in the current branch. Useful for backtracking or returning to a parent node.
        /// </summary>
        /// <returns>The previous node, or null if none is found.</returns>
        public Node GetPreviousNode()
        {
            if( stateMachine.currentNode != null )
            {
                return FindParentNode( stateMachine.treeInstance, stateMachine.currentNode );
            }

            return null;
        }

        /// <summary>
        /// Finds the parent node of a given child node within the behavior tree.
        /// </summary>
        /// <param name="parentNode">The node to start the search from (usually the root node).</param>
        /// <param name="childNode">The child node whose parent is being searched for.</param>
        /// <returns>The parent node if found, otherwise null.</returns>
        public Node FindParentNode( Node parentNode, Node childNode )
        {
            // Check if the parent node contains the child node
            if( parentNode.childNodes.Contains( childNode ) )
                return parentNode;

            for (int i = 0; i < parentNode.childNodes.Count; i++ )
            {
                Node child = parentNode.childNodes[ i ];    

                Node result = FindParentNode(child, childNode);

                if (result != null)
                    return result;
            }

            // Return null if the parent node was not found
            return null;
        }

        /// <summary>
        /// Shuffles the priority of a node by swapping it with the next node in the parent's child list.
        /// This method can be modified to account for hierarchical prioritization.
        /// </summary>
        /// <param name="node">The node to shuffle.</param>
        /// <returns>True if the node was successfully shuffled, otherwise false.</returns>
        public bool ShuffleNodePriority( Node node )
        {
            Node parentNode = FindParentNode( stateMachine.treeInstance, node );

            if( parentNode != null )
            {
                int currentIndex = parentNode.childNodes.IndexOf( node );

                if( currentIndex < parentNode.childNodes.Count - 1 )
                {
                    Node nextNode = parentNode.childNodes[ currentIndex + 1 ];
                    parentNode.childNodes[ currentIndex ] = nextNode;
                    parentNode.childNodes[ currentIndex + 1 ] = node;
                    node.SetPriority( nextNode.Priority + 1 );

                    // Ensure the next node gets properly started if swapped in
                    if( nextNode.State == NodeState.Initialized )
                    {
                        nextNode.Start();
                    }

                    return true;
                }
            }

            return false;
        }
    }

}