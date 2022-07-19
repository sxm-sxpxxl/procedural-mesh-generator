using System;
using System.Linq;
using UnityEngine;

public sealed class PlaneMeshCreator : MeshCreator
{
    public override Mesh CreateMesh(MeshData data)
    {
        _meshData = data;

        int resolution = data.resolution;
        Vector3 size = data.size, offset = data.offset;

        var mesh = new Mesh();

        int edgeVerticesCount = resolution + 1;
        int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;

        VertexData vertexData = CreateVertices(
            verticesLength: quadVerticesCount, 
            excludedVerticesCount: 0,
            initVertexPoint: (-0.5f * size) + offset,
            getVertexPointByIndex: i => new Vector3
            {
                x = size.x / resolution * (i % edgeVerticesCount),
                y = size.y / resolution * (i / edgeVerticesCount)
            }
        );

        mesh.vertices = vertexData.vertices;
        mesh.uv = CreateUV(mesh.vertices);
        mesh.triangles = CreateTriangles(new QuadData[]
        { 
            new QuadData
            { 
                traversalOrder = RotationDirection.Clockwise,
                getActualVertexIndex = i => i
            }
        }, vertexData.excludedVerticesMap, data.isForwardFacing, data.isBackfaceCulling);

        return mesh;
    }

    private Vector2[] CreateUV(Vector3[] vertices)
    {
        Vector3 size = _meshData.size, offset = _meshData.offset;
        Func<Vector3, Vector2> uvExtractor = vertex => Vector3.Scale((vertex + (0.5f * size) + offset), new Vector3(1f / size.x, 1f / size.y, 1f / size.z));

        return vertices.Select(uvExtractor).ToArray();
    }
}
