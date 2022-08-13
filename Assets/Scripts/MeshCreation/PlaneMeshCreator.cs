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

            _vertices = verticesData.vertexGroups;

            Vector2[] uv = CreateUV(verticesData.Vertices, v => Vector3.Scale(
                (v - offset) + (0.5f * size),
                new Vector3(1f / size.x, 1f / size.y, 1f / size.z)
            ));
            
            Vector3[] normals = CreateNormals(verticesData.Vertices, (i, v) => {
                int verticesCount = _meshData.isBackfaceCulling ? verticesData.vertexGroups.Length : verticesData.vertexGroups.Length / 2;
                return i >= verticesCount ? Vector3.back : Vector3.forward;
            });
            
            int[] triangles = CreateTriangles(new FaceData[]
            {
                new FaceData(RotationDirection.CW, i => i)
            }, verticesData, _meshData.isForwardFacing, _meshData.isBackfaceCulling);
            
            return new Mesh
            {
                vertices = verticesData.Vertices,
                uv = uv,
                normals = normals,
                triangles = triangles
            };
        }
    }
}
