using System;
using System.Linq;
using UnityEngine;

public sealed class PlaneMeshCreator : MeshCreator
{
    public override Mesh CreateMesh(MeshData data)
    {
        _meshData = data;

        int resolution = data.resolution;
        Vector2 size = data.size;
        Vector2 offset = data.offset;
        Plane plane = data.plane;

        var mesh = new Mesh();

        int verticesCount = (resolution + 1) * (resolution + 1);
        mesh.vertices = CreateVertices(verticesCount,
            (-0.5f * new Vector2(size.x, size.y) + offset).AsFor(plane),
            i => (new Vector2
                {
                    x = size.x / resolution * (i % (resolution + 1)),
                    y = size.y / resolution * (i / (resolution + 1))
                }
            ).AsFor(plane)
        );
        mesh.uv = CreateUV(mesh.vertices);
        mesh.triangles = CreateTriangles(1, i => RotationDirection.Clockwise, triangleVertexIndex =>
            triangleVertexIndex % 2 == 0 ? triangleVertexIndex / 2 : triangleVertexIndex / 2 + (resolution + 1)
        );

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }

    private Vector2[] CreateUV(Vector3[] vertices)
    {
        Vector2 size = _meshData.size, offset = _meshData.offset;
        Func<Vector2, Vector2> uvExtractor = vertex => (vertex + (0.5f * size) + offset) / size;
        
        return vertices.Select(x => uvExtractor.Invoke(x.ExtractFor(_meshData.plane))).ToArray();
    }
}
