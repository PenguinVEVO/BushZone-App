using FrameLabs.AI.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.AI.Runtime
{
    /// <summary>
    /// Serialized runtime structure for a compiled behavior tree.
    /// This is the optimized runtime format consumed by the StateMachine system.
    /// </summary>
    public class StateMachineAsset : ScriptableObject
    {
        /// <summary>
        /// The entry point of the tree. This must be a StartNode.
        /// </summary>
        [HideInInspector][SerializeReference] public Node RootNode;

        /// <summary>
        /// All compiled, traversable nodes in the tree.
        /// These are embedded as sub-assets within this asset.
        /// </summary>
        [HideInInspector][SerializeReference] public List<Node> RuntimeNodes = new();

        /// <summary>
        /// Optional metadata name of the tree for identification or analytics.
        /// </summary>
        [HideInInspector][SerializeField] public string TreeName;
    }
}
