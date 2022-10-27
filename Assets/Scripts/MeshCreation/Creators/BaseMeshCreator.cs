using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    internal abstract class BaseMeshCreator<TRequest> : IMeshCreator
        where TRequest : BaseMeshRequest
    {
        private TRequest _baseRequest;
        
        protected InterstitialMeshData meshData;
        
        protected enum RotationDirection
        {
            CW = 0, // Clockwise
            CCW = 1 // Counterclockwise
        }

        protected readonly struct FaceData
        {
            public readonly RotationDirection traversalOrder;
            public readonly Func<int, int> getActualVertexGroupIndex;
            public readonly Func<Vector3> getFaceNormal;
            public readonly Func<int, Vector2> getUV;
            public readonly int vertexGroupOffset;

            public FaceData(
                RotationDirection traversalOrder,
                Func<int, int> getActualVertexGroupIndex,
                Func<Vector3> getFaceNormal,
                Func<int, Vector2> getUV,
                int vertexGroupOffset = 0
            )
            {
                this.traversalOrder = traversalOrder;
                this.getActualVertexGroupIndex = getActualVertexGroupIndex;
                this.getFaceNormal = getFaceNormal;
                this.getUV = getUV;
                this.vertexGroupOffset = vertexGroupOffset;
            }
        }

        public BaseMeshCreator(in TRequest request)
        {
            _baseRequest = request;
        }
        
        public InterstitialMeshData CreateMeshData() =>
            _baseRequest.postProcessCallback?.Invoke(Handle(_baseRequest)) ?? Handle(_baseRequest);
        
        InterstitialMeshData IMeshCreator.GetLastMeshData() => meshData;
        
        protected abstract InterstitialMeshData Handle(TRequest request);

        protected void ScaleAndOffset()
        {
            if (_baseRequest.isScalingAndOffsetting == false)
            {
                return;
            }
            
            var vertices = meshData.Vertices;
            var initialPoint = -0.5f * _baseRequest.size + _baseRequest.offset;
            
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = initialPoint + Vector3.Scale(vertices[i], _baseRequest.size);
            }
            
            meshData.Bounds = new Bounds(_baseRequest.offset, _baseRequest.size);
        }
        
        protected void CreateVertices(
            int vertexGroupsCount,
            int excludedVertexGroupsCount,
            Func<int, Vector3> getVertexPointByIndex,
            Func<int, bool> isVertexGroupExcluded = null,
            Func<int, int> getVertexGroupSizeByIndex = null
        )
        {
            const int excludedGroup = -1;
            
            int groupsCountWithBackface = (_baseRequest.isBackfaceCulling ? 1 : 2) * vertexGroupsCount;
            int allGroupsCount = vertexGroupsCount + excludedVertexGroupsCount;
            int allGroupsCountWithBackface = groupsCountWithBackface + excludedVertexGroupsCount;
            int currentAllGroupsSize = 0;

            var vertexGroups = new VertexGroup[groupsCountWithBackface];
            var excludedVertexGroupsMap = new int[allGroupsCountWithBackface];

            for (int i = 0, vIndex = 0, currentExcludedGroupsCount = 0; i < allGroupsCount; i++)
            {
                if (isVertexGroupExcluded?.Invoke(i) ?? false)
                {
                    currentExcludedGroupsCount++;
                    excludedVertexGroupsMap[i] = excludedGroup;
                    
                    if (_baseRequest.isBackfaceCulling == false)
                    {
                        excludedVertexGroupsMap[i + vertexGroupsCount] = excludedGroup;
                    }
                    
                    continue;
                }
                
                int vertexGroupSize = getVertexGroupSizeByIndex?.Invoke(i) ?? 1;
                Vector3 vertexPosition = getVertexPointByIndex(i);
                
                vertexGroups[vIndex] = new VertexGroup(
                    selfIndex: vIndex,
                    position: vertexPosition,
                    startIndex: i + currentAllGroupsSize,
                    length: vertexGroupSize
                );
                excludedVertexGroupsMap[i] = i - currentExcludedGroupsCount;
                
                if (_baseRequest.isBackfaceCulling == false)
                {
                    vertexGroups[vIndex + vertexGroupsCount] = new VertexGroup(
                        selfIndex: vIndex + vertexGroupsCount,
                        position: vertexPosition,
                        startIndex: i + vertexGroupsCount + currentAllGroupsSize,
                        length: vertexGroupSize
                    );
                    excludedVertexGroupsMap[i + vertexGroupsCount] = (i + vertexGroupsCount) - currentExcludedGroupsCount;
                }
                
                vIndex++;
                currentAllGroupsSize += vertexGroupSize - 1;
            }

            meshData = new InterstitialMeshData(
                _baseRequest.name,
                verticesCount: allGroupsCountWithBackface + currentAllGroupsSize - excludedVertexGroupsCount,
                vertexGroups,
                excludedVertexGroupsMap,
                _baseRequest.isBackfaceCulling
            );
        }
        
        protected void SetTriangles(in FaceData[] faces, int baseEdgeVertexGroupOffset = 0)
        {
            bool isBackfaceCulling = _baseRequest.isBackfaceCulling, isForwardFacing = _baseRequest.isForwardFacing;
            int oneFaceIndicesCount = GetFaceIndicesCountBy(_baseRequest.resolution);
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
                    meshData,
                    actualTraversalOrder,
                    face.getActualVertexGroupIndex,
                    face.getFaceNormal,
                    face.getUV,
                    face.vertexGroupOffset,
                    baseEdgeVertexGroupOffset
                );

                if (isBackfaceCulling == false)
                {
                    SetFace(
                        indices,
                        startIndex: (i + 1) * oneFaceIndicesCount,
                        meshData,
                        (RotationDirection) (1 - (int) actualTraversalOrder),
                        index => face.getActualVertexGroupIndex(index) + meshData.vertexGroups.Length / 2,
                        () => -face.getFaceNormal(),
                        face.getUV,
                        face.vertexGroupOffset,
                        baseEdgeVertexGroupOffset
                    );
                }
            }

            meshData.SetTriangles(indices);
        }

        private void SetFace(
            int[] indices,
            int startIndex,
            in InterstitialMeshData interstitialMeshData,
            RotationDirection traversalOrder,
            Func<int, int> getActualVertexGroupIndex,
            Func<Vector3> getFaceNormal,
            Func<int, Vector2> getUV,
            int vertexGroupOffset,
            int baseEdgeVertexGroupOffset
        )
        {
            int resolution = _baseRequest.resolution;
            int faceIndicesCount = GetFaceIndicesCountBy(resolution);
            int[] excludedVertexGroupsMap = interstitialMeshData.excludedVertexGroupsMap;
            VertexGroup[] vertexGroups = interstitialMeshData.vertexGroups;

            int ConvertToTriangleSpace(int index) => (index % 2 == 0) ? (index / 2) : (index / 2 + (resolution + 1));
            
            // Triangle vertex group indices divided by two Vector3Int.
            Vector3Int tv1Indices = Vector3Int.zero, tv2Indices = Vector3Int.zero;
            // Unique triangle vertex group indices divided by two Vector2Int.
            Vector2Int ut1Indices = Vector2Int.zero, ut2Indices = Vector2Int.zero;
            
            int initIndex = 0, repeatedIndex = 0;
            int minRightEdgeIndex = resolution * (resolution + 1), maxRightEdgeIndex = resolution * (resolution + 2);

            for (int i = 0; i < faceIndicesCount; i += 6)
            {
                tv1Indices[0] = ConvertToTriangleSpace(((int) traversalOrder) + initIndex);
                tv1Indices[1] = ConvertToTriangleSpace((1 - (int) traversalOrder) + initIndex);
                tv1Indices[2] = ConvertToTriangleSpace(2 + initIndex);
                tv2Indices[0] = ConvertToTriangleSpace((1 + 2 * (int) traversalOrder) + initIndex);
                tv2Indices[1] = ConvertToTriangleSpace((3 - 2 * (int) traversalOrder) + initIndex);
                tv2Indices[2] = tv1Indices[2];

                // Conversion to unique indices rule:
                // CCW(1):  ABC | DAC -> 3,0,1 | 4,3,1
                // CW(0):   ABC | BDC -> 0,3,1 | 3,4,1
                ut1Indices[0] = tv1Indices[0];                        // A
                ut1Indices[1] = tv1Indices[1];                        // B
                ut2Indices[0] = tv1Indices[2];                        // C
                ut2Indices[1] = tv2Indices[1 - (int) traversalOrder]; // D
                
                // Major transformations on vertex group indices to determine end vertex indices, and calculation its normals.
                for (int j = 0; j < 4; j++)
                {
                    int uniqueIndex = (j < 2 ? ut1Indices : ut2Indices)[j % 2];

                    int actualVertexGroupIndex = getActualVertexGroupIndex(uniqueIndex);
                    int adjustedForExcludedGroupsIndex = excludedVertexGroupsMap[actualVertexGroupIndex];
                    
                    bool isLieOnLeftEdge = uniqueIndex > 0 && uniqueIndex < resolution;
                    bool isLieOnRightEdge = uniqueIndex > minRightEdgeIndex && uniqueIndex < maxRightEdgeIndex;
                    
                    int overlapCorrectionOffset = 0;
                    if (baseEdgeVertexGroupOffset == vertexGroupOffset && (isLieOnLeftEdge || isLieOnRightEdge))
                    {
                        overlapCorrectionOffset = -baseEdgeVertexGroupOffset;
                    }
                    
                    int excludedGroupsCount = actualVertexGroupIndex - adjustedForExcludedGroupsIndex;
                    int actualVertexIndex = vertexGroups[adjustedForExcludedGroupsIndex][vertexGroupOffset + overlapCorrectionOffset] - excludedGroupsCount;

                    if (j < 2)
                    {
                        ut1Indices[j % 2] = actualVertexIndex;
                    }
                    else
                    {
                        ut2Indices[j % 2] = actualVertexIndex;
                    }

                    interstitialMeshData.Normals[actualVertexIndex] = getFaceNormal();
                    interstitialMeshData.UV[actualVertexIndex] = getUV(uniqueIndex);
                }

                tv1Indices[0] = ut1Indices[0];
                tv1Indices[1] = ut1Indices[1];
                tv1Indices[2] = ut2Indices[0];
                tv2Indices[0] = (traversalOrder == RotationDirection.CW ? ut1Indices : ut2Indices)[1];
                tv2Indices[1] = (traversalOrder == RotationDirection.CW ? ut2Indices : ut1Indices)[1 - (int) traversalOrder];
                tv2Indices[2] = ut2Indices[0];
                
                // Filling triangle indices for the quad.
                for (int j = 0; j < 6; j++)
                {
                    indices[startIndex + i + j] = (j < 3 ? tv1Indices : tv2Indices)[j % 3];
                }
                
                repeatedIndex = (int) Mathf.Repeat(initIndex, 2 * (resolution + 1));
                initIndex += 2 * (1 + (repeatedIndex / 2 % resolution) / Mathf.Max(resolution - 1, 1));
            }
        }

        private static int GetFaceIndicesCountBy(int resolution) => 6 * resolution * resolution;
    }
}
