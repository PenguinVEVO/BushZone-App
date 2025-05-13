using FrameLabs.AI.Nodes;
using System.Collections.Generic;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public class ExitNodeCompiler : INodeProcessor<ExitNode>
    {
        public void ProcessNode(ExitNode source, ExitNode target, List<CompilerMessage> messages)
        {
            target.executeOnce = source.executeOnce;
            target.RestartTree = source.RestartTree;
        }
    }
}
