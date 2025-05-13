using FrameLabs.AI.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public class GraphValidator
    {
        public class ValidationResult
        {
            public bool IsValid => Errors.Count == 0;
            public List<string> Errors { get; } = new();
        }

        public static ValidationResult Validate( ParsedGraph parsedGraph )
        {
            var result = new ValidationResult();

            if( parsedGraph == null )
            {
                result.Errors.Add( "ParsedGraph is null." );
                return result;
            }

            if( parsedGraph.RootNode == null )
            {
                result.Errors.Add( "Missing StartNode. Graph must have a valid entry point." );
            }

            foreach( var node in parsedGraph.nodes.Values )
            {
                if( node.RuntimeNode == null )
                {
                    result.Errors.Add( $"Node with ID '{node.ID}' is missing a runtime instance." );
                }

                foreach( var (branchIndex, childId) in node.Children )
                {
                    if( !parsedGraph.nodes.ContainsKey( childId ) )
                    {
                        result.Errors.Add( $"Node '{node.ID}' has a child connection to missing node ID '{childId}'." );
                    }

                    if( branchIndex < 0 )
                    {
                        result.Errors.Add( $"Invalid branch index {branchIndex} in node '{node.ID}'. Must be >= 0." );
                    }

                    if (node.ID == childId)
                    {
                        result.Errors.Add($"Node '{node.ID}' has a self-referencing loop (points to itself).");
                    }
                }
            }

            if( HasCycles( parsedGraph ) )
            {
                result.Errors.Add( "Graph contains one or more cyclic references." );
            }

            return result;
        }

        private static bool HasCycles(ParsedGraph graph)
        {
            foreach (var node in graph.nodes.Values)
            {
                if (HasCycleDFS(node.ID, graph, new Stack<string>()))
                    return true;
            }

            return false;
        }

        private static bool HasCycleDFS(string nodeId, ParsedGraph graph, Stack<string> path)
        {
            if (path.Contains(nodeId))
                return true;

            path.Push(nodeId);

            if (graph.nodes.TryGetValue(nodeId, out var node))
            {
                foreach (var (_, childId) in node.Children)
                {
                    if (HasCycleDFS(childId, graph, path))
                        return true;
                }
            }

            path.Pop();
            return false;
        }


    }

}