using FrameLabs.AI.Nodes;
using FrameLabs.AI.Runtime;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public class CompilerMessage
    {
        public MessageCategory type;
        public string message;
    }

    public class CompilerResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<CompilerMessage> Errors { get; } = new();
    }

    public static class GraphCompiler
    {
        public static (StateMachineAsset, CompilerResult) Compile(ParsedGraph parsedGraph, string treeName, string assetDirectory)
        {
            CompilerResult result = new CompilerResult();
            var compiledNodes = new Dictionary<string, Node>();

            var asset = ScriptableObject.CreateInstance<StateMachineAsset>();
            asset.TreeName = treeName;

            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{assetDirectory}/{treeName}.asset");
            AssetDatabase.CreateAsset(asset, assetPath);

            foreach (var kvp in parsedGraph.nodes)
            {
                var parsedNode = kvp.Value;
                Node nodeInstance = ScriptableObject.CreateInstance(parsedNode.NodeType) as Node;

                if (nodeInstance == null)
                {
                    result.Errors.Add(new CompilerMessage
                    {
                        message = $"[GraphCompiler] Failed to create node instance of type {parsedNode.NodeType}",
                        type = MessageCategory.Error
                    });
                    continue;
                }

                nodeInstance.name = parsedNode.Title;
                compiledNodes[parsedNode.ID] = nodeInstance;

                var sourceNode = parsedNode.View?.NodeData;
                var targetNode = nodeInstance;

                object compilerInstance = null;
                Type sourceType = sourceNode?.GetType();
                Type targetType = targetNode?.GetType();

                if (sourceNode != null && targetNode != null && sourceType == targetType)
                {
                    var tryGetCompilerMethod = typeof(NodeCompilerRegistry)
                        .GetMethod(nameof(NodeCompilerRegistry.TryGetCompiler))
                        ?.MakeGenericMethod(sourceType);

                    var args = new object[] { null };
                    bool found = (bool?)tryGetCompilerMethod?.Invoke(null, args) ?? false;

                    if (found && args[0] is not null)
                    {
                        compilerInstance = args[0];
                        dynamic compiler = compilerInstance;
                        compiler.ProcessNode((dynamic)sourceNode, (dynamic)targetNode, result.Errors);
                    }
                }

                asset.RuntimeNodes.Add(nodeInstance);
                AssetDatabase.AddObjectToAsset(nodeInstance, asset);

                if (compilerInstance != null && sourceType != null)
                {
                    Type additonalProperties = typeof(IAdditonalProperties<>).MakeGenericType(sourceType);
                    if (additonalProperties.IsInstanceOfType(compilerInstance))
                    {
                        var getExtrasMethod = additonalProperties.GetMethod("GetAdditonalProperties");
                        var extraAssets = getExtrasMethod.Invoke(compilerInstance, new object[] { targetNode }) as IEnumerable<ScriptableObject>;

                        foreach (var extra in extraAssets)
                        {
                            if (extra != null)
                                AssetDatabase.AddObjectToAsset(extra, asset);
                        }
                    }
                }
            }

            foreach (var parsedNode in parsedGraph.nodes.Values)
            {
                if (!compiledNodes.TryGetValue(parsedNode.ID, out var parentNode))
                    continue;

                foreach (var (branch, childId) in parsedNode.Children)
                {
                    if (!compiledNodes.TryGetValue(childId, out var childNode))
                    {
                        result.Errors.Add(new CompilerMessage
                        {
                            message = $"[GraphCompiler] Missing child node '{childId}' for parent '{parsedNode.ID}'",
                            type = MessageCategory.Warning
                        });
                        continue;
                    }

                    parentNode.MapExitCodeToChild(branch, childNode);
                }
            }

            if (parsedGraph.RootNode != null && compiledNodes.TryGetValue(parsedGraph.RootNode.ID, out var rootNode))
            {
                asset.RootNode = rootNode;
            }
            else
            {
                result.Errors.Add(new CompilerMessage
                {
                    message = "[GraphCompiler] Could not assign RootNode: missing or invalid.",
                    type = MessageCategory.Error
                });
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return (asset, result);
        }
    }
}
