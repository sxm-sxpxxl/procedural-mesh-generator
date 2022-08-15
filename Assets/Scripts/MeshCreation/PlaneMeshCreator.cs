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

            VerticesData verticesData = CreateVertices(
                vertexGroupsCount: quadVerticesCount,
                excludedVertexGroupsCount: 0,
                initVertexPoint: (-0.5f * size) + offset,
                getVertexPointByIndex: i => new Vector3
                {
                    x = size.x / resolution * (i % edgeVerticesCount),
                    y = size.y / resolution * (i / edgeVerticesCount)
                }
            );

            int[] triangles = CreateTriangles(new FaceData[]
            {
                new FaceData(
                    RotationDirection.CW,
                    i => i, 
                    () => _meshData.isForwardFacing ? Vector3.back : Vector3.forward,
                    i => new Vector2(
                        x: 1f / resolution * (i % edgeVerticesCount),
                        y: 1f / resolution * (i / edgeVerticesCount)
                    )
                )
            }, verticesData);
            
            return new Mesh
            {
                vertices = verticesData.vertices,
                normals = verticesData.normals,
                uv = verticesData.uv,
                triangles = triangles
            };
        }
    }
}
