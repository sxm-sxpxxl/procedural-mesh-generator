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
        private IMeshCreator _meshCreator;
        
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
            
            int verticesLength = meshResponse.BackfaceAdjustedVerticesLength;
            var showedVertexGroups = new List<int>(capacity: meshResponse.vertexGroups.Length);
            
            for (int i = 0; i < verticesLength; i++)
            {
                Vector3 actualVertexPosition = relativeTransform.TransformPoint(vertices[i]);

                Gizmos.color = data.vertexColor;
                Gizmos.DrawSphere(actualVertexPosition, data.vertexSize);

                VertexGroup targetGroup = meshResponse.GetGroupByVertexIndex(i);
                if (data.isVertexLabelShowed && showedVertexGroups.Contains(targetGroup.selfIndex) == false)
                {
                    StringBuilder vertexLabel = new StringBuilder();

                    if (data.isDuplicatedVerticesShowed)
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
                            vertexLabel.Append($",{i + verticesLength}");
                        }

                        vertexLabel.Append(']');
                    }
                    else
                    {
                        vertexLabel.Append($"V[{targetGroup.selfIndex}]");
                    }
                    
                    var style = new GUIStyle();
                    style.normal.textColor = data.labelColor;
                    Handles.Label(actualVertexPosition, vertexLabel.ToString(), style);
                    
                    showedVertexGroups.Add(targetGroup.selfIndex);
                }
                
                if (normals.Length != 0 && data.isVertexNormalShowed)
                {
                    Gizmos.color = data.normalColor;
                    Gizmos.DrawRay(
                        actualVertexPosition,
                        relativeTransform.TransformDirection(data.normalSize * normals[i])
                    );
                    
                    if (withBackfaceCulling == false)
                    {
                        Gizmos.DrawRay(
                            actualVertexPosition,
                            relativeTransform.TransformDirection(data.normalSize * normals[i + verticesLength])
                        );
                    }
                }
            }
        }
        
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
