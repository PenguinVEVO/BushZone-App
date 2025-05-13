using FrameLabs.AI.Extension;
using FrameLabs.AI.Nodes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.AI.System
{
    /// <summary>
    /// Manages the behavior tree for the state machine, including loading, resetting, and restarting nodes.
    /// This implementation discovers and clones nodes before initializing them.
    /// </summary>
    internal class NodeTreeController
    {
        // Reference to the state machine that owns this controller
        private readonly StateMachine stateMachine;

        // Queue and discovery set for tracking nodes
        private readonly Dictionary<Node, Node> clonedNodes = new Dictionary<Node, Node>();

        /// <summary>
        /// Constructor for the NodeTreeController. Requires a reference to the parent state machine.
        /// </summary>
        /// <param name="stateMachine">The parent state machine that owns this controller.</param>
        public NodeTreeController(StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// Load the behavior tree and initialize the root node.
        /// </summary>
        public void LoadTree()
        {
            if (stateMachine.treeInstance != null)
            {
                // Set the root node (treeInstance) as the current node
                stateMachine.currentNode = stateMachine.treeInstance;

                if (stateMachine.currentNode is StartNode)
                {
                    // Start the root node after loading the tree
                    stateMachine.currentNode.Start();
                }
                else
                {
                    stateMachine.currentNode = null;
                    Debug.LogWarning("Behavior tree does not have an entry point. Add a StartNode to the tree.");
                }
            }
        }

        /// <summary>
        /// Initializes the tree by discovering, cloning, and initializing all nodes sequentially.
        /// </summary>
        public void InitializeTree()
        {
            if (stateMachine.behaviourTree == null || stateMachine.behaviourTree.RootNode == null)
            {
                Debug.LogError("[NodeTreeController] No valid compiled asset assigned to state machine.");
                return;
            }

            clonedNodes.Clear();

            Node clonedRootNode = DiscoverAndCloneNodes(stateMachine.behaviourTree.RootNode);

            foreach (var clonedNode in clonedNodes.Values)
            {
                clonedNode.Id = Guid.NewGuid().ToString();
                clonedNode.Initialize(stateMachine);
            }

            stateMachine.treeInstance = clonedRootNode;
        }


        /// <summary>
        /// Discovers and clones nodes in the tree, handling circular references by mapping originals to clones.
        /// </summary>
        /// <param name="originalNode">The original node being processed.</param>
        /// <returns>The cloned root node of the tree.</returns>
        private Node DiscoverAndCloneNodes(Node originalNode)
        {
            // If the node has already been cloned, return the clone
            if (clonedNodes.ContainsKey(originalNode))
            {
                return clonedNodes[originalNode];
            }

            // Clone the current node (excluding its childNodes initially)
            Node clonedNode = CreateClone(originalNode);

            // Add the cloned node to the mapping
            clonedNodes[originalNode] = clonedNode;

            // Clear the cloned node's childNodes and repopulate with clones of the original's children
            clonedNode.childNodes.Clear();

            foreach (var childNode in originalNode.childNodes)
            {
                if (childNode != null)
                {
                    Node clonedChild = DiscoverAndCloneNodes(childNode); // Recursively clone the child
                    clonedNode.childNodes.Add(clonedChild);
                }
                else
                {
                    Debug.LogWarning("DiscoverAndCloneNodes: Encountered a null child node.");
                }
            }

            return clonedNode;
        }


        /// <summary>
        /// Creates a clone of a node using Unity's Instantiate method.
        /// Handles node-specific data (e.g., StateExtensions).
        /// </summary>
        /// <param name="originalNode">The original node to clone.</param>
        /// <returns>A new instance of the node.</returns>
        private Node CreateClone(Node originalNode)
        {
            Node clone = ScriptableObject.Instantiate(originalNode);

            // Handle node-specific data here (e.g., state extensions)
            if (clone is ActionNode actionNode && actionNode.stateExtension != null)
            {
                actionNode.stateExtension = ScriptableObject.Instantiate(actionNode.stateExtension);
            }

            return clone;
        }

        /// <summary>
        /// Resets the entire behavior tree by reinitializing the root node.
        /// The current node is reset and reloaded from the original tree structure.
        /// </summary>
        public void ResetTree()
        {
            // Exit the current node (if it exists)
            stateMachine.currentNode?.Exit();

            // Reload the behavior tree by initializing it again
            LoadTree();

            if (stateMachine.currentNode != null)
            {
                // Start the root node after the reset
                stateMachine.currentNode.Start();
                stateMachine.ClearBreadCrumbs();
            }
        }

        /// <summary>
        /// Restarts a specific node by exiting, reinitializing, and starting it.
        /// </summary>
        /// <param name="node">The node to restart.</param>
        /// <returns>True if the node is running after reinitialization, otherwise false.</returns>
        public bool RestartNode(Node node)
        {
            // Exit the node, reinitialize, and then start it
            node.Exit();
            node.Initialize(stateMachine);
            node.Start();

            // Return true if the node is now in the Running state
            return node.State == NodeState.Running;
        }
    }


}