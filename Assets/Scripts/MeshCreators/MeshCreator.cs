using System;
using UnityEngine;

public abstract class MeshCreator
{
    protected MeshData _meshData;

    protected enum RotationDirection
    {
        Clockwise = 0,
        Counterclockwise = 1
    }

    public struct MeshData
    {
        public int resolution;
        public Vector2 size;
        public Vector2 offset;
        public Plane plane;

        public MeshData(int resolution, Vector2 size, Vector2 offset = default, Plane plane = Plane.XZ)
        {
            this.resolution = resolution;
            this.size = size;
            this.offset = offset;
            this.plane = plane;
        }
    }

    public abstract Mesh CreateMesh(MeshData data);

    protected int[] CreateTriangles(int quadCount, Func<int, RotationDirection> getTraversalOrderByIndex, Func<int, int> getActualVertexIndex)
    {
        int quadIndicesCount = GetQuadIndicesByResolution(_meshData.resolution);
        int indicesCount = quadCount * quadIndicesCount;
        var indices = new int[indicesCount];

        for (int i = 0; i < quadCount; i++)
        {
            FillWithQuad(indices, i * quadIndicesCount, getTraversalOrderByIndex(i), getActualVertexIndex);
        }

        return indices;
    }

    protected Vector3[] CreateVertices(int length, Vector3 initVertexPoint, Func<int, Vector3> getVertexPointByIndex)
    {
        var vertices = new Vector3[length];

        vertices[0] = initVertexPoint;
        for (int i = 1; i < vertices.Length; i++)
        {
            vertices[i] = initVertexPoint + getVertexPointByIndex(i);
        }

        return vertices;
    }

    private void FillWithQuad(int[] indices, int startIndex, RotationDirection traversalOrder, Func<int, int> getActualVertexIndex)
    {
        int resolution = _meshData.resolution;
        int quadIndicesCount = GetQuadIndicesByResolution(resolution);

        int triangleVertexIndex = 0, repeatedIndex = 0;
        int firstIndex, secondIndex, thirdIndex, fourthIndex, fifthIndex, sixthIndex;

        for (int i = 0; i < quadIndicesCount; i += 6)
        {
            firstIndex  = ((int) traversalOrder)         + triangleVertexIndex;
            secondIndex = (1 - (int) traversalOrder)     + triangleVertexIndex;
            thirdIndex  = 2                              + triangleVertexIndex;
            fourthIndex = (1 + 2 * (int) traversalOrder) + triangleVertexIndex;
            fifthIndex  = (3 - 2 * (int) traversalOrder) + triangleVertexIndex;
            sixthIndex  = thirdIndex;

            indices[startIndex + i + 0] = getActualVertexIndex(firstIndex);
            indices[startIndex + i + 1] = getActualVertexIndex(secondIndex);
            indices[startIndex + i + 2] = getActualVertexIndex(thirdIndex);
            
            indices[startIndex + i + 3] = getActualVertexIndex(fourthIndex);
            indices[startIndex + i + 4] = getActualVertexIndex(fifthIndex);
            indices[startIndex + i + 5] = getActualVertexIndex(sixthIndex);

            repeatedIndex = (int) Mathf.Repeat(triangleVertexIndex, 2 * (resolution + 1));
            triangleVertexIndex += 2 * (1 + (repeatedIndex / 2 % resolution) / Mathf.Max(resolution - 1, 1));
        }
    }

    private static int GetQuadIndicesByResolution(int resolution) => 6 * resolution * resolution;
}
