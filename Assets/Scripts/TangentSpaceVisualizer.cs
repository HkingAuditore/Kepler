using UnityEngine;

public class TangentSpaceVisualizer : MonoBehaviour
{
    public float offset = 0.01f;
    public float scale  = 0.1f;

    private void OnDrawGizmos()
    {
        var filter = GetComponent<MeshFilter>();
        if (filter)
        {
            var mesh = filter.sharedMesh;
            if (mesh) ShowTangentSpace(mesh);
        }
    }

    private void ShowTangentSpace(Mesh mesh)
    {
        var vertices = mesh.vertices;
        var normals  = mesh.normals;
        var tangents = mesh.tangents;
        for (var i = 0; i < vertices.Length; i++)
            ShowTangentSpace(
                             transform.TransformPoint(vertices[i]),
                             transform.TransformDirection(normals[i]),
                             transform.TransformDirection(tangents[i]),
                             tangents[i].w
                            );
    }

    private void ShowTangentSpace(
        Vector3 vertex, Vector3 normal, Vector3 tangent, float binormalSign
    )
    {
        vertex       += normal * offset;
        Gizmos.color =  Color.green;
        Gizmos.DrawLine(vertex, vertex + normal * scale);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(vertex, vertex + tangent * scale);
        var binormal = Vector3.Cross(normal, tangent) * binormalSign;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(vertex, vertex + binormal * scale);
    }
}