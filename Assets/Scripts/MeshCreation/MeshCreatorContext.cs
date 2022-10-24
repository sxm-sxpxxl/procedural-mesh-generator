using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public sealed class MeshCreatorContext
    {
        private static readonly float MaxVertexSizeInUnit = 0.01f;
        private static readonly float MaxNormalSizeInUnit = 0.1f;

        private IMeshCreator _meshCreator;
        private DebugData _data;
        
        [Serializable]
        public sealed class DebugData
        {
            [SerializeField] internal bool areVerticesShowed = false;
            [SerializeField] internal Color vertexColor = new Color(0f, 0f, 0f, 0.5f);
            [SerializeField] internal float vertexSize = 0.01f;
            [SerializeField] internal bool isVertexLabelShowed = true;
            [SerializeField] internal Color labelColor = Color.white;
            [SerializeField] internal bool isDuplicatedVerticesShowed = true;
            [SerializeField] internal bool isVertexNormalShowed = true;
            [SerializeField] internal Color normalColor = Color.yellow;
            [SerializeField] internal float normalSize = 0.01f;
            [SerializeField] internal bool isBoundsShowed = false;
            [SerializeField] internal Color boundsColor = Color.green;
        }
        
        public void DrawDebug(Transform relativeTransform, DebugData data)
        {
            _data = data;
            var meshResponse = _meshCreator.GetLastMeshResponse();
            
            if (data.isBoundsShowed)
            {
                Gizmos.color = data.boundsColor;
                Gizmos.DrawWireCube(meshResponse.Bounds.center, meshResponse.Bounds.size);
            }

            var vertices = meshResponse.Vertices;
            var normals = meshResponse.Normals;
            var withBackfaceCulling = meshResponse.withBackfaceCulling;
            
            if (data.areVerticesShowed == false || vertices.Length == 0)
            {
                return;
            }

            float averageBoundsSize = GetAverageSize(meshResponse.Bounds.size);
            int verticesLength = meshResponse.BackfaceAdjustedVerticesLength;
            var showedVertexGroups = new List<int>(capacity: meshResponse.vertexGroups.Length);
            
            for (int i = 0; i < verticesLength; i++)
            {
                Vector3 actualVertexPosition = relativeTransform.TransformPoint(vertices[i]);
                var actualVertexSize = MaxVertexSizeInUnit * averageBoundsSize * data.vertexSize;
                
                DrawVertex(actualVertexPosition, actualVertexSize);
                
                VertexGroup targetGroup = meshResponse.GetGroupByVertexIndex(i);
                if (showedVertexGroups.Contains(targetGroup.selfIndex) == false)
                {
                    bool isLabelDraw = DrawLabelByIndex(
                        targetGroup,
                        i,
                        verticesLength,
                        withBackfaceCulling,
                        actualVertexPosition
                    );
                    
                    if (isLabelDraw)
                    {
                        showedVertexGroups.Add(targetGroup.selfIndex);
                    }
                }
                
                var actualNormalSize = MaxNormalSizeInUnit * averageBoundsSize * data.normalSize;
                DrawNormalByIndex(
                    normals, 
                    i,
                    verticesLength,
                    withBackfaceCulling,
                     relativeTransform,
                    actualVertexPosition,
                    actualNormalSize
                );
            }
        }

        private void DrawVertex(Vector3 position, float size)
        {
            Gizmos.color = _data.vertexColor;
            Gizmos.DrawSphere(position, size);
        }

        private bool DrawLabelByIndex(
            VertexGroup targetGroup,
            int index,
            int backfaceOffset,
            bool withBackfaceCulling,
            Vector3 position
        )
        {
            if (_data.isVertexLabelShowed == false)
            {
                return false;
            }
            
            StringBuilder vertexLabel = new StringBuilder();
            if (_data.isDuplicatedVerticesShowed)
            {
                vertexLabel.Append("V[");

                if (targetGroup.hasSingleVertex)
                {
                    vertexLabel.Append(targetGroup.singleIndex);
                }
                else
                {
                    vertexLabel.AppendJoin(',', targetGroup.indices);
                }

                if (withBackfaceCulling == false)
                {
                    vertexLabel.Append($",{index + backfaceOffset}");
                }

                vertexLabel.Append(']');
            }
            else
            {
                vertexLabel.Append($"V[{targetGroup.selfIndex}]");
            }
            
            var style = new GUIStyle();
            style.normal.textColor = _data.labelColor;
            Handles.Label(position, vertexLabel.ToString(), style);
            
            return true;
        }
        
        private void DrawNormalByIndex(
            Vector3[] normals,
            int index,
            int backfaceOffset,
            bool withBackfaceCulling,
            Transform target,
            Vector3 position,
            float normalSize
        )
        {
            if (_data.isVertexNormalShowed == false || normals.Length == 0)
            {
                return;
            }
            
            Gizmos.color = _data.normalColor;
            Gizmos.DrawRay(
                position,
                target.TransformDirection(normalSize * normals[index])
            );
                    
            if (withBackfaceCulling == false)
            {
                Gizmos.DrawRay(
                    position,
                    target.TransformDirection(normalSize * normals[index + backfaceOffset])
                );
            }
        }

        private static float GetAverageSize(Vector3 size) => (size.x + size.y + size.z) / 3f;
        
        public MeshResponse CreateMesh(in BaseMeshRequest request)
        {
            _meshCreator = request switch
            {
                PlaneMeshRequest => new PlaneMeshCreator(request as PlaneMeshRequest),
                CubeMeshRequest => new CubeMeshCreator(request as CubeMeshRequest),
                SphereMeshRequest => new SphereMeshCreator(request as SphereMeshRequest),
                _ => throw new ArgumentOutOfRangeException(nameof(request), "Not expected request value!")
            };

            return _meshCreator.CreateMeshResponse();
        }
    }
}
