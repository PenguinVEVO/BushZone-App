/// <summary>
/// Interface for graph elements that can be copied into clipboard data.
/// </summary>
public interface ICopyableElement
{
    /// <summary>
    /// Create a serialized copy of the element for clipboard storage.
    /// </summary>
    /// <param name="copyId">Whether to preserve or generate a new unique ID.</param>
    /// <returns>Serialized data: NodeData or VisualElementData.</returns>
    object CreateCopyData(bool copyId);
}