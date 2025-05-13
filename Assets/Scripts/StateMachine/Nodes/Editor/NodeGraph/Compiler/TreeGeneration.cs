using FrameLabs.AI.Nodes;
using System.Linq;
using FrameLabs.AI.Runtime;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public static class TreeGeneration
    {
        public class TreeGenerationResult
        {
            public StateMachineAsset CompiledAsset;
            public GraphValidator.ValidationResult ValidationResult;
            public bool Success => ValidationResult != null && ValidationResult.IsValid && CompiledAsset != null && CompiledAsset.RootNode != null;
        }

        public static TreeGenerationResult Generate(string treeName, string assetDirectory, StateGraphView graphView)
        {
            var result = new TreeGenerationResult();

            var parser = new GraphParser();
            var parsedNodes = parser.Parse(graphView);

            if (parsedNodes == null)
            {                
                graphView.OwningTab.ShowEditorMessage("[TreePipeline] Parsing failed. Aborting generation.", MessageCategory.Error);
                return result;
            }

            var parsedGraph = new ParsedGraph
            {
                nodes = parsedNodes,
                RootNode = parsedNodes.Values.FirstOrDefault(n => n.RuntimeNode is StartNode)
            };

            result.ValidationResult = GraphValidator.Validate(parsedGraph);
            if (!result.ValidationResult.IsValid)
            {
                foreach (var error in result.ValidationResult.Errors)
                    graphView.OwningTab.ShowEditorMessage($"[GraphValidator] {error}", MessageCategory.Error);

                graphView.OwningTab.ShowEditorMessage("[TreePipeline] Validation failed. Aborting generation.", MessageCategory.Warning);
                return result;
            }

            CompilerResult compilerResult;
            StateMachineAsset newSMA = null;
            (newSMA, compilerResult) = GraphCompiler.Compile(parsedGraph, treeName, assetDirectory);

            result.CompiledAsset = newSMA;

            for (int i = 0; i < compilerResult.Errors.Count; i++)
                graphView.OwningTab.ShowEditorMessage(compilerResult.Errors[i].message, compilerResult.Errors[i].type, 5);

            if (newSMA == null || newSMA.RootNode == null)
            {
                graphView.OwningTab.ShowEditorMessage("[TreePipeline] Compilation failed.", MessageCategory.Error);
            }

            return result;
        }
    }

}