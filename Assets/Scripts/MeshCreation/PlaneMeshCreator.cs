using System;
using System.Linq;
using UnityEngine;

namespace MeshCreation
{
    public sealed class PlaneMeshCreator : MeshCreator
    {
        protected override Mesh CreateMesh()
        {
            int resolution = _meshData.resolution;
            Vector3 size = _meshData.size, offset = _meshData.offset;

            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;

            VertexData vertexData = CreateVertices(
                verticesCount: quadVerticesCount,
                excludedVerticesCount: 0,
                initVertexPoint: (-0.5f * size) + offset,
                getVertexPointByIndex: i => new Vector3
                {
                    x = size.x / resolution * (i % edgeVerticesCount),
                    y = size.y / resolution * (i / edgeVerticesCount)
                }
            );

            Vector2[] uv = CreateUV(vertexData.vertices);
            int[] triangles = CreateTriangles(new QuadData[]
            {
                new QuadData(RotationDirection.CW, i => i)
            }, vertexData.excludedVerticesMap, _meshData.isForwardFacing, _meshData.isBackfaceCulling);

            return new Mesh
            {
                vertices = vertexData.vertices,
                uv = CreateUV(vertexData.vertices),
                triangles = triangles
            };
        }

        private Vector2[] CreateUV(Vector3[] vertices)
        {
            Vector3 size = _meshData.size, offset = _meshData.offset;
            Func<Vector3, Vector2> uvExtractor = vertex => Vector3.Scale(
                (vertex + (0.5f * size) + offset),
                new Vector3(1f / size.x, 1f / size.y, 1f / size.z)
            );

            return vertices.Select(uvExtractor).ToArray();
        }
    }
}
