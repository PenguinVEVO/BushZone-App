using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class BoxColliderVisualBounds : MonoBehaviour
{
    public Color wireFrame = Color.white;
    public Color mesh = Color.white;


    public VisualizationMode visualizationMode = VisualizationMode.Wireframe;
    public enum VisualizationMode { Wireframe, Mesh, Both }

    private BoxCollider boxCollider;

    private void OnValidate()
    {
        // Get the attached BoxCollider
        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }
    }

    private void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            // Calculate the scaled center based on GameObject's scale and BoxCollider's center
            Vector3 scaledCenter = Vector3.Scale(boxCollider.center, transform.lossyScale);

            // Calculate the rotated center based on GameObject's rotation and scaled center
            Vector3 rotatedCenter = transform.rotation * scaledCenter;

            // Calculate the scaled size based on GameObject's scale and BoxCollider's size
            Vector3 scaledSize = Vector3.Scale(boxCollider.size, transform.lossyScale);

            // Calculate the Gizmos matrix using the rotated center and scaled size
            Matrix4x4 cubeTransform = Matrix4x4.TRS(
                transform.position + rotatedCenter,
                transform.rotation,
                scaledSize
            );

            Gizmos.matrix = cubeTransform;

            if (visualizationMode == VisualizationMode.Wireframe)
            {
                // Set the color of the Gizmo
                Gizmos.color = wireFrame;

                // Draw a wireframe cube centered at (0, 0, 0)
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
            else if (visualizationMode == VisualizationMode.Mesh)
            {
                // Set the color of the Gizmo
                Gizmos.color = mesh;

                // Draw a wireframe cube centered at (0, 0, 0)
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
            }
            else if (visualizationMode == VisualizationMode.Both)
            {
                // Set the color of the Gizmo
                Gizmos.color = mesh;

                // Draw a wireframe cube centered at (0, 0, 0)
                Gizmos.DrawCube(Vector3.zero, Vector3.one);

                // Set the color of the Gizmo
                Gizmos.color = wireFrame;

                // Draw a wireframe cube centered at (0, 0, 0)
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }

            // Reset the Gizmos matrix to prevent affecting other Gizmos drawings
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
}
