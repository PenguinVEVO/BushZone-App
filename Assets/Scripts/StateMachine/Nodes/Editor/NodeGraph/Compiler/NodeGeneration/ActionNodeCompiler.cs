using FrameLabs.AI.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public class ActionNodeCompiler : INodeProcessor<ActionNode>, IAdditonalProperties<ActionNode>
    {
        public void ProcessNode(ActionNode source, ActionNode target, List<CompilerMessage> messages)
        {
            target.executeOnce = source.executeOnce;

            if (source.stateExtension != null)
            {
                var clone = ScriptableObject.Instantiate(source.stateExtension);
                target.stateExtension = clone;

                messages.Add(new CompilerMessage
                {
                    type = MessageCategory.Info,
                    message = $"[GraphCompiler] Compiling StateExtension: {clone.name}"
                });
            }
        }

        public IEnumerable<ScriptableObject> GetAdditonalProperties(ActionNode target)
        {
            if(target.stateExtension != null)
                yield return target.stateExtension;
        }

    }
}