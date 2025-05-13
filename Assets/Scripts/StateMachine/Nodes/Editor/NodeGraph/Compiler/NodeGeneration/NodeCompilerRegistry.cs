using FrameLabs.AI.Nodes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public static class NodeCompilerRegistry
    {
        private static readonly Dictionary<Type, object> compilers = new Dictionary<Type, object>()
        {
            { typeof(ActionNode), new ActionNodeCompiler() },
            { typeof(ExitNode), new ExitNodeCompiler() }
        };


        /// <summary>
        /// Registers a custom compiler for a specific node type. Users can call this during editor initialization.
        /// </summary>
        /// <typeparam name="TNode">The node type to associate with the compiler.</typeparam>
        /// <param name="compiler">The compiler instance.</param>
        /// <param name="allowOverwrite">Whether to overwrite an existing registration. Default is false.</param>
        public static void RegisterCustomCompiler<TNode>(INodeProcessor<TNode> compiler, bool allowOverwrite = false)
            where TNode : Node
        {
            var key = typeof(TNode);
            if (compilers.ContainsKey(key) && !allowOverwrite)
            {
                Debug.LogWarning($"[NodeCompilerRegistry] Compiler for {key.Name} already exists. Skipping registration.");
                return;
            }

            compilers[key] = compiler;
        }

        /// <summary>
        /// Retrieves a compiler for a given node type, if one has been registered.
        /// </summary>
        public static bool TryGetCompiler<TNode>(out INodeProcessor<TNode> compiler) where TNode : Node
        {
            if (compilers.TryGetValue(typeof(TNode), out var instance))
            {
                compiler = instance as INodeProcessor<TNode>;
                return compiler != null;
            }

            compiler = null;
            return false;
        }
    }
}
