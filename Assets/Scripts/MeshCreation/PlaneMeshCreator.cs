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

            Vector2[] uv = CreateUV(verticesData.vertices, v => Vector3.Scale(
                (v - offset) + (0.5f * size),
                new Vector3(1f / size.x, 1f / size.y, 1f / size.z)
            ));
            
            Vector3[] normals = CreateNormals(verticesData.vertices, (i, _) => {
                int vertexGroupsCount = _meshData.isBackfaceCulling ? verticesData.vertexGroups.Length : verticesData.vertexGroups.Length / 2;
                Vector3 backfaceAdjustedNormal = i >= vertexGroupsCount ? Vector3.forward : Vector3.back;
                return _meshData.isForwardFacing ? backfaceAdjustedNormal : -backfaceAdjustedNormal;
            });
            
            int[] triangles = CreateTriangles(new FaceData[]
            {
                new FaceData(RotationDirection.CW, i => i)
            }, verticesData);
            
            return new Mesh
            {
                vertices = verticesData.vertices,
                uv = uv,
                normals = normals,
                triangles = triangles
            };
        }
    }
}
