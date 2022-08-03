using System;
using UnityEngine;

namespace MeshCreation
{
    public abstract class MeshCreator
    {
        protected MeshData _meshData;

        protected enum RotationDirection
        {
            CW = 0, // Clockwise
            CCW = 1 // Counterclockwise
        }

        protected readonly struct QuadData
        {
            public readonly RotationDirection traversalOrder;
            public readonly Func<int, int> getActualVertexIndex;

            public QuadData(RotationDirection traversalOrder, Func<int, int> getActualVertexIndex)
            {
                this.traversalOrder = traversalOrder;
                this.getActualVertexIndex = getActualVertexIndex;
            }
        }

        protected readonly struct VertexData
        {
            public readonly Vector3[] vertices;
            public readonly int[] excludedVerticesMap;

            public VertexData(Vector3[] vertices, int[] excludedVerticesMap)
            {
                this.vertices = vertices;
                this.excludedVerticesMap = excludedVerticesMap;
            }
        }

        public Mesh CreateMesh(in MeshData data)
        {
            _meshData = data;
            return CreateMesh();
        }

        protected abstract Mesh CreateMesh();

        protected int[] CreateTriangles(
            in QuadData[] quads,
            in int[] excludedVerticesMap,
            bool isForwardFacing = true,
            bool isBackfaceCulling = true
        )
        {
            int quadIndicesCount = GetQuadIndicesCountBy(_meshData.resolution);
            int allIndicesCount = (isBackfaceCulling ? 1 : 2) * quads.Length * quadIndicesCount;

            var indices = new int[allIndicesCount];
            for (int i = 0; i < quads.Length; i++)
            {
                QuadData quad = quads[i];
                RotationDirection actualTraversalOrder = isForwardFacing
                    ? quad.traversalOrder
                    : (RotationDirection) (1 - (int) quad.traversalOrder);

                SetQuad(
                    indices,
                    startIndex: i * quadIndicesCount,
                    excludedVerticesMap,
                    actualTraversalOrder,
                    quad.getActualVertexIndex
                );

                if (isBackfaceCulling == false)
                {
                    SetQuad(
                        indices,
                        startIndex: (i + 1) * quadIndicesCount,
                        excludedVerticesMap,
                        (RotationDirection) (1 - (int) actualTraversalOrder),
                        quad.getActualVertexIndex
                    );
                }
            }

            return indices;
        }

        protected VertexData CreateVertices(
            int verticesCount,
            int excludedVerticesCount,
            Vector3 initVertexPoint,
            Func<int, Vector3> getVertexPointByIndex,
            Func<int, bool> isVertexValid = null
        )
        {
            int allVerticesCount = verticesCount + excludedVerticesCount;

            var vertices = new Vector3[verticesCount];
            var excludedVerticesMap = new int[allVerticesCount];

            vertices[0] = initVertexPoint;
            for (int i = 1, vIndex = 1, invalidCount = 0; i < allVerticesCount; i++)
            {
                if ((isVertexValid?.Invoke(i) ?? true) == false)
                {
                    invalidCount++;
                    excludedVerticesMap[i] = -1;
                    continue;
                }

                vertices[vIndex] = initVertexPoint + getVertexPointByIndex(i);
                excludedVerticesMap[i] = i - invalidCount;
                vIndex++;
            }

            return new VertexData(vertices, excludedVerticesMap);
        }

        private void SetQuad(
            int[] indices,
            int startIndex,
            in int[] excludedVerticesMap,
            RotationDirection traversalOrder,
            Func<int, int> getActualVertexIndex
        )
        {
            int resolution = _meshData.resolution;
            int quadIndicesCount = GetQuadIndicesCountBy(resolution);

            Func<int, int> convertIndex = i => (i % 2 == 0) ? (i / 2) : (i / 2 + (resolution + 1));

            int triangleVertexIndex = 0, repeatedIndex = 0;
            int v1Index, v2Index, v3Index, v4Index, v5Index, v6Index;

            for (int i = 0; i < quadIndicesCount; i += 6)
            {
                v1Index = convertIndex(((int) traversalOrder) + triangleVertexIndex);
                v2Index = convertIndex((1 - (int) traversalOrder) + triangleVertexIndex);
                v3Index = convertIndex(2 + triangleVertexIndex);
                v4Index = convertIndex((1 + 2 * (int) traversalOrder) + triangleVertexIndex);
                v5Index = convertIndex((3 - 2 * (int) traversalOrder) + triangleVertexIndex);
                v6Index = v3Index;

                indices[startIndex + i + 0] = excludedVerticesMap[getActualVertexIndex(v1Index)];
                indices[startIndex + i + 1] = excludedVerticesMap[getActualVertexIndex(v2Index)];
                indices[startIndex + i + 2] = excludedVerticesMap[getActualVertexIndex(v3Index)];

                indices[startIndex + i + 3] = excludedVerticesMap[getActualVertexIndex(v4Index)];
                indices[startIndex + i + 4] = excludedVerticesMap[getActualVertexIndex(v5Index)];
                indices[startIndex + i + 5] = excludedVerticesMap[getActualVertexIndex(v6Index)];

                repeatedIndex = (int) Mathf.Repeat(triangleVertexIndex, 2 * (resolution + 1));
                triangleVertexIndex += 2 * (1 + (repeatedIndex / 2 % resolution) / Mathf.Max(resolution - 1, 1));
            }
        }

        private static int GetQuadIndicesCountBy(int resolution) => 6 * resolution * resolution;
    }
}
