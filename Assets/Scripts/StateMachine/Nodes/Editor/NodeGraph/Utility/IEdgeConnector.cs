using FrameLabs.Utilities.NodeEditor;
using UnityEngine;

public interface IEdgeConnector
{
    Vector2 GetAnchorWorldPosition();
    void Connect(EdgeElement edge);
}


