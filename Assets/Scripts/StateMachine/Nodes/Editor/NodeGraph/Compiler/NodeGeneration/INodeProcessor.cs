using FrameLabs.AI.Nodes;
using System.Collections.Generic;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public interface INodeProcessor<TNode> where TNode : Node
    {
        void ProcessNode(TNode source, TNode target, List<CompilerMessage> messages);
    }
}
