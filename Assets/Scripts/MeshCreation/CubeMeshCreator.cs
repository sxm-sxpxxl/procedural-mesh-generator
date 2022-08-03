using System;
using System.Linq;
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
            int maxVerticesCount = (int) Mathf.Pow(edgeVerticesCount, 3);
            int excludedVerticesCount = (int) Mathf.Pow(resolution - 1, 3);

            VertexData vertexData = CreateVertices(
                verticesCount: maxVerticesCount - excludedVerticesCount,
                excludedVerticesCount: excludedVerticesCount,
                initVertexPoint: (-0.5f * size) + offset,
                getVertexPointByIndex: i => new Vector3
                {
                    x = size.x / resolution * Mathf.Repeat(i / edgeVerticesCount, edgeVerticesCount),
                    y = size.y / resolution * (i % edgeVerticesCount),
                    z = size.z / resolution * (i / quadVerticesCount)
                },
                isVertexValid: i =>
                {
                    int minValidIndex = quadVerticesCount;
                    int maxValidIndex = maxVerticesCount - quadVerticesCount;

                    if (i < minValidIndex || i > maxValidIndex)
                    {
                        return true;
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
                            return true;
                        }
                    }

                    return false;
                }
            );

            int[] triangles = CreateTriangles(new QuadData[]
            {
                new QuadData
                (
                    RotationDirection.CCW,
                    i => i
                ),
                new QuadData
                (
                    RotationDirection.CW,
                    i => i + resolution * quadVerticesCount
                ),
                new QuadData
                (
                    RotationDirection.CCW,
                    i => i * edgeVerticesCount
                ),
                new QuadData
                (
                    RotationDirection.CW,
                    i => i * edgeVerticesCount + resolution
                ),
                new QuadData
                (
                    RotationDirection.CW,
                    i => i * edgeVerticesCount - (i % edgeVerticesCount) * resolution
                ),
                new QuadData
                (
                    RotationDirection.CCW,
                    i =>
                        i * edgeVerticesCount - (i % edgeVerticesCount) * resolution + resolution * edgeVerticesCount
                )
            }, vertexData.excludedVerticesMap);

            return new Mesh
            {
                vertices = vertexData.vertices,
                triangles = triangles
            };
        }
    }
}
