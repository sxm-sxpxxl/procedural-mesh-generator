using UnityEngine;

namespace MeshCreation
{
    public sealed class PlaneMeshCreator : MeshCreator
    {
        protected override MeshResponse CreateMeshResponse()
        {
            int resolution = meshRequest.resolution;
            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;

            CreateVertices(
                vertexGroupsCount: quadVerticesCount,
                excludedVertexGroupsCount: 0,
                getVertexPointByIndex: i => new Vector3
                {
                    x = 1f / resolution * (i % edgeVerticesCount),
                    y = 1f / resolution * (int) (i / edgeVerticesCount)
                }
            );

            SetTriangles(new FaceData[]
            {
                new FaceData(
                    RotationDirection.CW,
                    i => i, 
                    () => meshRequest.isForwardFacing ? Vector3.back : Vector3.forward,
                    i => new Vector2(
                        x: 1f / resolution * (i % edgeVerticesCount),
                        y: 1f / resolution * (int) (i / edgeVerticesCount)
                    )
                )
            });
            
            return MeshResponse;
        }
    }
}
