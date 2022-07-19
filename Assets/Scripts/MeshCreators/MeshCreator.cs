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
        public Vector3 size;
        public Vector3 offset;
        public bool isForwardFacing;
        public bool isBackfaceCulling;

        public MeshData(int resolution, Vector3 size, Vector3 offset = default, bool isForwardFacing = true, bool isBackfaceCulling = true)
        {
            this.resolution = resolution;
            this.size = size;
            this.offset = offset;
            this.isForwardFacing = isForwardFacing;
            this.isBackfaceCulling = isBackfaceCulling;
        }
    }

    protected struct QuadData
    {
        public RotationDirection traversalOrder;
        public Func<int, int> getActualVertexIndex;
    }

    protected struct VertexData
    {
        public Vector3[] vertices;
        public int[] excludedVerticesMap;
    }

    public abstract Mesh CreateMesh(MeshData data);

    protected int[] CreateTriangles(QuadData[] quadDatas, int[] excludedVerticesMap, bool isForwardFacing = true, bool isBackfaceCulling = true)
    {
        int quadIndicesCount = GetQuadIndicesByResolution(_meshData.resolution);
        int indicesCount = (isBackfaceCulling ? 1 : 2) * quadDatas.Length * quadIndicesCount;

        var indices = new int[indicesCount];
        for (int i = 0; i < quadDatas.Length; i++)
        {
            QuadData data = quadDatas[i];
            RotationDirection actualTraversalOrder = isForwardFacing ? data.traversalOrder : (RotationDirection) (1 - (int) data.traversalOrder);

            FillWithQuad(
                indices,
                startIndex: i * quadIndicesCount,
                excludedVerticesMap,
                traversalOrder: actualTraversalOrder,
                getActualVertexIndex: data.getActualVertexIndex
            );

            if (isBackfaceCulling == false)
            {
                FillWithQuad(
                    indices,
                    startIndex: (i + 1) * quadIndicesCount,
                    excludedVerticesMap,
                    traversalOrder: (RotationDirection) (1 - (int) actualTraversalOrder),
                    getActualVertexIndex: data.getActualVertexIndex
                );
            }
        }

        return indices;
    }

    protected VertexData CreateVertices(
        int verticesLength,
        int excludedVerticesCount,
        Vector3 initVertexPoint,
        Func<int, Vector3> getVertexPointByIndex,
        Func<int, bool> isVertexValid = null
    )
    {
        int allVerticesCount = verticesLength + excludedVerticesCount;

        var vertices = new Vector3[verticesLength];
        var excludedVerticesMap = new int[allVerticesCount];

        vertices[0] = initVertexPoint;
        for (int i = 1, j = 1, invalidCount = 0; i < allVerticesCount; i++)
        {
            if ((isVertexValid?.Invoke(i) ?? true) == false)
            {
                invalidCount++;
                excludedVerticesMap[i] = -1;
                continue;
            }

            vertices[j] = initVertexPoint + getVertexPointByIndex(i);
            excludedVerticesMap[i] = i - invalidCount;
            j++;
        }

        return new VertexData { vertices = vertices, excludedVerticesMap = excludedVerticesMap };
    }

    private void FillWithQuad(int[] indices, int startIndex, int[] vertexIndices, RotationDirection traversalOrder, Func<int, int> getActualVertexIndex)
    {
        int resolution = _meshData.resolution;
        int quadIndicesCount = GetQuadIndicesByResolution(resolution);

        int triangleVertexIndex = 0, repeatedIndex = 0;
        int firstIndex, secondIndex, thirdIndex, fourthIndex, fifthIndex, sixthIndex;

        Func<int, int> convertIndex = i => (i % 2 == 0) ? (i / 2) : (i / 2 + (resolution + 1));

        for (int i = 0; i < quadIndicesCount; i += 6)
        {
            firstIndex  = convertIndex(((int) traversalOrder)         + triangleVertexIndex);
            secondIndex = convertIndex((1 - (int) traversalOrder)     + triangleVertexIndex);
            thirdIndex  = convertIndex(2                              + triangleVertexIndex);
            fourthIndex = convertIndex((1 + 2 * (int) traversalOrder) + triangleVertexIndex);
            fifthIndex  = convertIndex((3 - 2 * (int) traversalOrder) + triangleVertexIndex);
            sixthIndex  = thirdIndex;

            indices[startIndex + i + 0] = vertexIndices[getActualVertexIndex(firstIndex)];
            indices[startIndex + i + 1] = vertexIndices[getActualVertexIndex(secondIndex)];
            indices[startIndex + i + 2] = vertexIndices[getActualVertexIndex(thirdIndex)];
            
            indices[startIndex + i + 3] = vertexIndices[getActualVertexIndex(fourthIndex)];
            indices[startIndex + i + 4] = vertexIndices[getActualVertexIndex(fifthIndex)];
            indices[startIndex + i + 5] = vertexIndices[getActualVertexIndex(sixthIndex)];

            repeatedIndex = (int) Mathf.Repeat(triangleVertexIndex, 2 * (resolution + 1));
            triangleVertexIndex += 2 * (1 + (repeatedIndex / 2 % resolution) / Mathf.Max(resolution - 1, 1));
        }
    }

    private static int GetQuadIndicesByResolution(int resolution) => 6 * resolution * resolution;
}
