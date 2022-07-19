using System;
using System.Linq;
using UnityEngine;

public sealed class CubeMeshCreator : MeshCreator
{
    public override Mesh CreateMesh(MeshData data)
    {
        _meshData = data;

        int resolution = data.resolution;
        Vector3 size = data.size, offset = data.offset;

        var mesh = new Mesh();
        
        int edgeVerticesCount = resolution + 1;
        int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;
        int maxVerticesCount = (int) Mathf.Pow(edgeVerticesCount, 3);
        int excludedVerticesCount = (int) Mathf.Pow(resolution - 1, 3);

        VertexData vertexData = CreateVertices(
            verticesLength: maxVerticesCount - excludedVerticesCount,
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

                int[] possibleMultipliers = Enumerable.Range(0, resolution - 1).ToArray();

                int firstInvalidIndex = quadVerticesCount + edgeVerticesCount + 1;
                int diff = i - firstInvalidIndex;

                int highOrderMultiplier = (int) diff / quadVerticesCount;
                if (possibleMultipliers.Contains(highOrderMultiplier) == false)
                {
                    return true;
                }

                int middleOrderMultiplier = (int) (diff % quadVerticesCount) / edgeVerticesCount;
                if (possibleMultipliers.Contains(middleOrderMultiplier) == false)
                {
                    return true;
                }

                int lowOrderMultiplier = (int) (diff % quadVerticesCount) % edgeVerticesCount;
                if (possibleMultipliers.Contains(lowOrderMultiplier) == false)
                {
                    return true;
                }

                return false;
            }
        );

        mesh.vertices = vertexData.vertices;
        mesh.triangles = CreateTriangles(new QuadData[]
        {
            new QuadData
            {
                traversalOrder = RotationDirection.Counterclockwise,
                getActualVertexIndex = i => i
            },
            new QuadData
            {
                traversalOrder = RotationDirection.Clockwise,
                getActualVertexIndex = i => i + resolution * quadVerticesCount
            },
            new QuadData
            {
                traversalOrder = RotationDirection.Counterclockwise,
                getActualVertexIndex = i => i * edgeVerticesCount
            },
            new QuadData
            {
                traversalOrder = RotationDirection.Clockwise,
                getActualVertexIndex = i => i * edgeVerticesCount + resolution
            },
            new QuadData
            {
                traversalOrder = RotationDirection.Clockwise,
                getActualVertexIndex = i => i * edgeVerticesCount - (i % edgeVerticesCount) * resolution
            },
            new QuadData
            {
                traversalOrder = RotationDirection.Counterclockwise,
                getActualVertexIndex = i => i * edgeVerticesCount - (i % edgeVerticesCount) * resolution + resolution * edgeVerticesCount
            }
        }, vertexData.excludedVerticesMap);

        return mesh;
    }
}
