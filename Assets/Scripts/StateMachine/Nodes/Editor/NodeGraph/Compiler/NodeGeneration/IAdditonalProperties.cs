using FrameLabs.AI.Nodes;
using System.Collections.Generic;
using UnityEngine;

namespace FrameLabs.Utilities.NodeEditor.Generation
{
    public interface IAdditonalProperties<TNode> where TNode : Node
    {
        IEnumerable<ScriptableObject> GetAdditonalProperties(TNode target);
    }
}