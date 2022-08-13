using System;
using System.Linq;
using UnityEngine;

namespace MeshCreation
{
    public abstract class MeshCreator
    {
        public VerticesData VerticesData { get; private set; }

        protected MeshData _meshData;

        protected enum RotationDirection
        {
            CW = 0, // Clockwise
            CCW = 1 // Counterclockwise
        }

        protected readonly struct FaceData
        {
            public readonly RotationDirection traversalOrder;
            public readonly Func<int, int> getActualVertexGroupIndex;
            public readonly int vertexGroupOffset;

            public FaceData(RotationDirection traversalOrder, Func<int, int> getActualVertexGroupIndex, int vertexGroupOffset = 0)
            {
                this.traversalOrder = traversalOrder;
                this.getActualVertexGroupIndex = getActualVertexGroupIndex;
                this.vertexGroupOffset = vertexGroupOffset;
            }
        }
        
        public Mesh CreateMesh(in MeshData data)
        {
            _meshData = data;
            return CreateMesh();
        }

        protected abstract Mesh CreateMesh();

        protected Vector3[] CreateNormals(in Vector3[] vertices, Func<int, Vector3, Vector3> getNormalByVertex)
        {
            var normals = new Vector3[vertices.Length];
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = getNormalByVertex(i, vertices[i]);
            }

            return normals;
        }
        
        protected Vector2[] CreateUV(in Vector3[] vertices, Func<Vector3, Vector2> getUVByVertex)
        {
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < uv.Length; i++)
            {
                uv[i] = getUVByVertex(vertices[i]);
            }

            return uv;
        }

        protected VerticesData CreateVertices(
            int vertexGroupsCount,
            int excludedVertexGroupsCount,
            Vector3 initVertexPoint,
            Func<int, Vector3> getVertexPointByIndex,
            Func<int, bool> isVertexGroupExcluded = null,
            Func<int, int> getVertexGroupSizeByIndex = null
        )
        {
            const int excludedGroup = -1;
            
            int groupsCountWithBackface = (_meshData.isBackfaceCulling ? 1 : 2) * vertexGroupsCount;
            int allGroupsCount = vertexGroupsCount + excludedVertexGroupsCount;
            int allGroupsCountWithBackface = groupsCountWithBackface + excludedVertexGroupsCount;
            int edgeVerticesCount = _meshData.resolution + 1, currentAllGroupsSize = 0;

            var vertexGroups = new VertexGroup[groupsCountWithBackface];
            var excludedVertexGroupsMap = new int[allGroupsCountWithBackface];
            
            for (int i = 0, vIndex = 0, currentExcludedGroupsCount = 0; i < allGroupsCount; i++)
            {
                if (isVertexGroupExcluded?.Invoke(i) ?? false)
                {
                    currentExcludedGroupsCount++;
                    excludedVertexGroupsMap[i] = excludedGroup;
                    
                    if (_meshData.isBackfaceCulling == false)
                    {
                        excludedVertexGroupsMap[i + vertexGroupsCount] = excludedGroup;
                    }
                    
                    continue;
                }
                
                int vertexGroupSize = getVertexGroupSizeByIndex?.Invoke(i) ?? 1;
                vertexGroups[vIndex] = new VertexGroup(
                    selfIndex: vIndex,
                    position: initVertexPoint + getVertexPointByIndex(i),
                    indices: Enumerable.Range(i + currentAllGroupsSize, vertexGroupSize).ToArray()
                );
                excludedVertexGroupsMap[i] = i - currentExcludedGroupsCount;
                
                if (_meshData.isBackfaceCulling == false)
                {
                    vertexGroups[vIndex + vertexGroupsCount] = vertexGroups[vIndex];
                    excludedVertexGroupsMap[i + vertexGroupsCount] = (i + vertexGroupsCount) - currentExcludedGroupsCount;
                }
                
                vIndex++;
                currentAllGroupsSize += vertexGroupSize - 1;
            }

            VerticesData = new VerticesData(vertexGroups, excludedVertexGroupsMap);
            return VerticesData;
        }
        
        protected int[] CreateTriangles(
            in FaceData[] faces,
            VerticesData verticesData,
            bool isForwardFacing = true,
            bool isBackfaceCulling = true
        )
        {
            int oneFaceIndicesCount = GetFaceIndicesCountBy(_meshData.resolution);
            int allFacesIndicesCount = (isBackfaceCulling ? 1 : 2) * faces.Length * oneFaceIndicesCount;
            var indices = new int[allFacesIndicesCount];
            
            for (int i = 0; i < faces.Length; i++)
            {
                FaceData face = faces[i];
                RotationDirection actualTraversalOrder = isForwardFacing
                    ? face.traversalOrder
                    : (RotationDirection) (1 - (int) face.traversalOrder);

                SetFace(
                    indices,
                    startIndex: i * oneFaceIndicesCount,
                    verticesData,
                    actualTraversalOrder,
                    face.getActualVertexGroupIndex,
                    face.vertexGroupOffset
                );

                if (isBackfaceCulling == false)
                {
                    SetFace(
                        indices,
                        startIndex: (i + 1) * oneFaceIndicesCount,
                        verticesData,
                        (RotationDirection) (1 - (int) actualTraversalOrder),
                        i => face.getActualVertexGroupIndex(i) + verticesData.vertexGroups.Length,
                        face.vertexGroupOffset
                    );
                }
            }

            return indices;
        }

        private void SetFace(
            int[] indices,
            int startIndex,
            in VerticesData verticesData,
            RotationDirection traversalOrder,
            Func<int, int> getActualVertexGroupIndex,
            int vertexGroupOffset
        )
        {
            int resolution = _meshData.resolution;
            int faceIndicesCount = GetFaceIndicesCountBy(resolution);
            int[] excludedVertexGroupsMap = verticesData.excludedVertexGroupsMap;
            VertexGroup[] vertexGroups = verticesData.vertexGroups;
            
            Func<int, int> fromTriangleSpace = i => (i % 2 == 0) ? (i / 2) : (i / 2 + (resolution + 1));
            Vector3Int leftIndices = Vector3Int.zero, rightIndices = Vector3Int.zero;
            int initIndex = 0, repeatedIndex = 0, actualVertexGroupIndex, adjustedForExcludedGroupsIndex, excludedGroupsCount;
            
            for (int i = 0; i < faceIndicesCount; i += 6)
            {
                leftIndices [0] = fromTriangleSpace(((int) traversalOrder) + initIndex);
                leftIndices [1] = fromTriangleSpace((1 - (int) traversalOrder) + initIndex);
                leftIndices [2] = fromTriangleSpace(2 + initIndex);
                rightIndices[0] = fromTriangleSpace((1 + 2 * (int) traversalOrder) + initIndex);
                rightIndices[1] = fromTriangleSpace((3 - 2 * (int) traversalOrder) + initIndex);
                rightIndices[2] = leftIndices[2];
                
                for (int j = 0; j < 6; j++)
                {
                    Vector3Int targetIndices = j < 3 ? leftIndices : rightIndices;
                    
                    actualVertexGroupIndex = getActualVertexGroupIndex(targetIndices[j % 3]);
                    adjustedForExcludedGroupsIndex = excludedVertexGroupsMap[actualVertexGroupIndex];
                    excludedGroupsCount = actualVertexGroupIndex - adjustedForExcludedGroupsIndex;
                    
                    indices[startIndex + i + j] = vertexGroups[adjustedForExcludedGroupsIndex][vertexGroupOffset] - excludedGroupsCount;
                }
                
                repeatedIndex = (int) Mathf.Repeat(initIndex, 2 * (resolution + 1));
                initIndex += 2 * (1 + (repeatedIndex / 2 % resolution) / Mathf.Max(resolution - 1, 1));
            }
        }

        private static int GetFaceIndicesCountBy(int resolution) => 6 * resolution * resolution;
    }
}
