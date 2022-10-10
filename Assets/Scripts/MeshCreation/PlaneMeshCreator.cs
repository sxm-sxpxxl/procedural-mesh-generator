using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    internal sealed class PlaneMeshCreator : MeshCreator
    {
        protected override MeshResponse HandleRequest()
        {
            int resolution = meshRequest.resolution;
            float distanceBetweenVertices = 1f / resolution;
            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;

            CreateVertices(
                vertexGroupsCount: quadVerticesCount,
                excludedVertexGroupsCount: 0,
                getVertexPointByIndex: i => distanceBetweenVertices * new Vector3(
                    x: i % edgeVerticesCount,
                    y: (int) (i / edgeVerticesCount)
                )
            );
            
            SetTriangles(new FaceData[]
            {
                new FaceData(
                    RotationDirection.CW,
                    i => i,
                    () => meshRequest.isForwardFacing ? Vector3.back : Vector3.forward,
                    i => distanceBetweenVertices * new Vector2(
                        x: i % edgeVerticesCount,
                        y: (int) (i / edgeVerticesCount)
                    )
                )
            });
            
            ScaleAndOffset();
            return LastMeshResponse;
        }
    }
}
