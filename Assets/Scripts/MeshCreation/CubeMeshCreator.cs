using System;
using UnityEngine;

namespace MeshCreation
{
    public sealed class CubeMeshCreator : MeshCreator
    {
        protected override Mesh CreateMesh()
        {
            int resolution = _meshData.resolution;
            Vector3 size = _meshData.size, offset = _meshData.offset;

            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;
            int excludedVerticesCount = (int) Mathf.Pow(resolution - 1, 3);
            int verticesCount = (int) Mathf.Pow(edgeVerticesCount, 3) - excludedVerticesCount;

            VerticesData verticesData = CreateVertices(
                vertexGroupsCount: verticesCount,
                excludedVertexGroupsCount: excludedVerticesCount,
                initVertexPoint: (-0.5f * size) + offset,
                getVertexPointByIndex: i => new Vector3
                {
                    x = size.x / resolution * Mathf.Repeat(i / edgeVerticesCount, edgeVerticesCount),
                    y = size.y / resolution * (i % edgeVerticesCount),
                    z = size.z / resolution * (i / quadVerticesCount)
                },
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
                        x = (int) diff / quadVerticesCount,
                        y = (int) (diff % quadVerticesCount) / edgeVerticesCount,
                        z = (int) (diff % quadVerticesCount) % edgeVerticesCount
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
                    int xOffset = (int) Mathf.Repeat(i / edgeVerticesCount, edgeVerticesCount);
                    int yOffset = i % edgeVerticesCount;
                    int zOffset = i / quadVerticesCount;

                    bool isXMinOrMax = xOffset == 0 || xOffset == resolution;
                    bool isYMinOrMax = yOffset == 0 || yOffset == resolution;
                    bool isZMinOrMax = zOffset == 0 || zOffset == resolution;

                    return Convert.ToInt32(isXMinOrMax) + Convert.ToInt32(isYMinOrMax) + Convert.ToInt32(isZMinOrMax);
                }
            );

            _vertices = verticesData.vertexGroups;

            int[] triangles = CreateTriangles(new FaceData[]
            {
                // Forward XY face
                new FaceData
                (
                    RotationDirection.CCW,
                    i => i,
                    vertexGroupOffset: (int) Plane.XY
                ),
                // Backward XY face
                new FaceData
                (
                    RotationDirection.CW,
                    i => i + resolution * quadVerticesCount,
                    vertexGroupOffset: (int) Plane.XY
                ),
                // Forward XZ face
                new FaceData
                (
                    RotationDirection.CCW,
                    i => i * edgeVerticesCount,
                    vertexGroupOffset: (int) Plane.XZ
                ),
                // Backward XZ face
                new FaceData
                (
                    RotationDirection.CW,
                    i => i * edgeVerticesCount + resolution,
                    vertexGroupOffset: (int) Plane.XZ
                ),
                // Forward YZ face
                new FaceData
                (
                    RotationDirection.CW,
                    i => i * edgeVerticesCount - (i % edgeVerticesCount) * resolution,
                    vertexGroupOffset: (int) Plane.YZ
                ),
                // Backward YZ face
                new FaceData
                (
                    RotationDirection.CCW,
                    i => (i + resolution) * edgeVerticesCount - (i % edgeVerticesCount) * resolution,
                    vertexGroupOffset: (int) Plane.YZ
                )
            }, verticesData);

            return new Mesh
            {
                vertices = verticesData.Vertices,
                triangles = triangles
            };
        }
    }
}
