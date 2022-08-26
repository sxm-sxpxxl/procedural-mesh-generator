using System;
using UnityEngine;

namespace MeshCreation
{
    public sealed class CubeMeshCreator : MeshCreator
    {
        private struct VertexData
        {
            public Vector3 position;
            public Vector3 normal;

            public VertexData(Vector3 position, Vector3 normal)
            {
                this.position = position;
                this.normal = normal;
            }
        }
        
        protected override MeshResponse CreateMeshResponse()
        {
            int resolution = meshRequest.resolution;
            Vector3 size = meshRequest.size, offset = meshRequest.offset;
            float roundness = (float) meshRequest.customData;

            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;
            int excludedVerticesCount = (int) Mathf.Pow(resolution - 1, 3);
            int verticesCount = (int) Mathf.Pow(edgeVerticesCount, 3) - excludedVerticesCount;

            CreateVertices(
                vertexGroupsCount: verticesCount,
                excludedVertexGroupsCount: excludedVerticesCount,
                getVertexPointByIndex: i => GetVertexDataBy(i).position,
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

                    var currentMultipliers = new Vector3Int
                    {
                        x = diff / quadVerticesCount,
                        y = (diff % quadVerticesCount) / edgeVerticesCount,
                        z = (diff % quadVerticesCount) % edgeVerticesCount
                    };

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
            },
                baseEdgeVertexGroupOffset: (int) Plane.XY,
                getCustomNormalVertex: roundness > 0f ? i => GetVertexDataBy(i).normal : null
            );
            
            return MeshResponse;
        }

        private VertexData GetVertexDataBy(int index)
        {
            int resolution = meshRequest.resolution;
            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;
            float distanceBetweenVertices = 1f / resolution;
            
            var withoutRoundnessPoint = new Vector3
            {
                x = distanceBetweenVertices * Mathf.Repeat((int) (index / edgeVerticesCount), edgeVerticesCount),
                y = distanceBetweenVertices * (index % edgeVerticesCount),
                z = distanceBetweenVertices * (int) (index / quadVerticesCount)
            };
            
            float roundness = (float) meshRequest.customData;
            float halfRoundness = 0.5f * roundness;
            float maxBorder = 1f - halfRoundness;
            
            var innerPoint = new Vector3
            {
                x = Mathf.Clamp(withoutRoundnessPoint.x, halfRoundness, maxBorder),
                y = Mathf.Clamp(withoutRoundnessPoint.y, halfRoundness, maxBorder),
                z = Mathf.Clamp(withoutRoundnessPoint.z, halfRoundness, maxBorder)
            };
            
            var normal = (withoutRoundnessPoint - innerPoint).normalized;
            var actualPosition = innerPoint + normal * halfRoundness;
            
            return new VertexData(actualPosition, normal);
        }
    }
}
