using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    internal sealed class CubeMeshCreator : BaseMeshCreator<CubeMeshRequest>
    {
        public CubeMeshCreator(in CubeMeshRequest request) : base(in request) { }
        
        protected override InterstitialMeshData Handle(CubeMeshRequest request)
        {
            int resolution = request.resolution;
            float distanceBetweenVertices = 1f / resolution;

            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;
            int excludedVerticesCount = (int) Mathf.Pow(resolution - 1, 3);
            int verticesCount = (int) Mathf.Pow(edgeVerticesCount, 3) - excludedVerticesCount;

            CreateVertices(
                vertexGroupsCount: verticesCount,
                excludedVertexGroupsCount: excludedVerticesCount,
                getVertexPointByIndex: i => distanceBetweenVertices * new Vector3(
                    x: Mathf.Repeat((int) (i / edgeVerticesCount), edgeVerticesCount),
                    y: (i % edgeVerticesCount),
                    z: (int) (i / quadVerticesCount)
                ),
                isVertexGroupExcluded: i =>
                {
                    int minValidIndex = quadVerticesCount;
                    int maxValidIndex = verticesCount - quadVerticesCount + excludedVerticesCount;

                    if (i < minValidIndex || i > maxValidIndex)
                    {
                        return false;
                    }

                    int firstInvalidIndex = quadVerticesCount + edgeVerticesCount + 1;
                    int diff = i - firstInvalidIndex;

                    var currentMultipliers = new Vector3Int(
                        x: diff / quadVerticesCount,
                        y: (diff % quadVerticesCount) / edgeVerticesCount,
                        z: (diff % quadVerticesCount) % edgeVerticesCount
                    );

                    for (int j = 0; j < 3; j++)
                    {
                        if (currentMultipliers[j] < 0 || currentMultipliers[j] >= resolution - 1)
                        {
                            return false;
                        }
                    }

                    return true;
                },
                getVertexGroupSizeByIndex: i =>
                {
                    int xOffset = (int) Mathf.Repeat((int) (i / edgeVerticesCount), edgeVerticesCount);
                    int yOffset = i % edgeVerticesCount;
                    int zOffset = i / quadVerticesCount;

                    bool isXMinOrMax = xOffset == 0 || xOffset == resolution;
                    bool isYMinOrMax = yOffset == 0 || yOffset == resolution;
                    bool isZMinOrMax = zOffset == 0 || zOffset == resolution;

                    return Convert.ToInt32(isXMinOrMax) + Convert.ToInt32(isYMinOrMax) + Convert.ToInt32(isZMinOrMax);
                }
            );
            
            SetTriangles(new FaceData[]
            {
                // Forward XY face
                new FaceData
                (
                    RotationDirection.CCW,
                    i => i,
                    () => Vector3.back,
                    i => new Vector2(
                        x: 1f / resolution * (int) (i / edgeVerticesCount),
                        y: 1f / resolution * (i % edgeVerticesCount)
                    ),
                    vertexGroupOffset: (int) Plane.XY
                ),
                // Backward XY face
                new FaceData
                (
                    RotationDirection.CW,
                    i => i + resolution * quadVerticesCount,
                    () => Vector3.forward,
                    i => new Vector2(
                        x: 1f - 1f / resolution * (int) (i / edgeVerticesCount),
                        y: 1f / resolution * (i % edgeVerticesCount)
                    ),
                    vertexGroupOffset: (int) Plane.XY
                ),
                // Forward XZ face
                new FaceData
                (
                    RotationDirection.CCW,
                    i => i * edgeVerticesCount,
                    () => Vector3.down,
                    i => new Vector2(
                        x: 1f / resolution * (i % edgeVerticesCount),
                        y: 1f - 1f / resolution * (int) (i / edgeVerticesCount)
                    ),
                    vertexGroupOffset: (int) Plane.XZ
                ),
                // Backward XZ face
                new FaceData
                (
                    RotationDirection.CW,
                    i => i * edgeVerticesCount + resolution,
                    () => Vector3.up,
                    i => new Vector2(
                        x: 1f / resolution * (i % edgeVerticesCount),
                        y: 1f / resolution * (int) (i / edgeVerticesCount)
                    ),
                    vertexGroupOffset: (int) Plane.XZ
                ),
                // Forward YZ face
                new FaceData
                (
                    RotationDirection.CW,
                    i => i * edgeVerticesCount - (i % edgeVerticesCount) * resolution,
                    () => Vector3.left,
                    i => new Vector2(
                        x: 1f - 1f / resolution * (int) (i / edgeVerticesCount),
                        y: 1f / resolution * (i % edgeVerticesCount)
                    ),
                    vertexGroupOffset: (int) Plane.YZ
                ),
                // Backward YZ face
                new FaceData
                (
                    RotationDirection.CCW,
                    i => (i + resolution) * edgeVerticesCount - (i % edgeVerticesCount) * resolution,
                    () => Vector3.right,
                    i => new Vector2(
                        x: 1f / resolution * (int) (i / edgeVerticesCount),
                        y: 1f / resolution * (i % edgeVerticesCount)
                    ),
                    vertexGroupOffset: (int) Plane.YZ
                )
            }, baseEdgeVertexGroupOffset: (int) Plane.XY);
            
            RoundCube(request.roundness);
            ScaleAndOffset();
            
            return meshData;
        }

        private void RoundCube(float roundness)
        {
            if (roundness <= 0)
            {
                return;
            }
            
            var vertices = meshData.Vertices;
            var normals = meshData.Normals;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                var cubePoint = vertices[i];
                
                float halfRoundness = 0.5f * roundness;
                float maxBorder = 1f - halfRoundness;

                var innerCubePoint = new Vector3(
                    x: Mathf.Clamp(cubePoint.x, halfRoundness, maxBorder),
                    y: Mathf.Clamp(cubePoint.y, halfRoundness, maxBorder),
                    z: Mathf.Clamp(cubePoint.z, halfRoundness, maxBorder)
                );
                
                normals[i] = (cubePoint - innerCubePoint).normalized;
                vertices[i] = innerCubePoint + normals[i] * halfRoundness;
            }
        }
    }
}
