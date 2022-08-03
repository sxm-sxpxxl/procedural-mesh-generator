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

            Vector2[] uv = CreateUV(vertexData.vertices, v => Vector3.Scale(
                (v - offset) + (0.5f * size),
                new Vector3(1f / size.x, 1f / size.y, 1f / size.z)
            ));
            
            Vector3[] normals = CreateNormals(vertexData.vertices, v => Vector3.forward);
            
            int[] triangles = CreateTriangles(new QuadData[]
            {
                new QuadData(RotationDirection.CW, i => i)
            }, vertexData.excludedVerticesMap, _meshData.isForwardFacing, _meshData.isBackfaceCulling);
            
            return new Mesh
            {
                vertices = vertexData.vertices,
                uv = uv,
                normals = normals,
                triangles = triangles
            };
        }
    }
}
