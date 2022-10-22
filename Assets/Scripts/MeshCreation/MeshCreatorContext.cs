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
        
        public void DrawDebug(
            Transform relativeTransform,
            float vertexSize = 0.01f,
            Color vertexColor = default,
            bool showVertexLabels = true,
            bool showDuplicatedVertexLabels = true,
            bool showVertexNormals = true,
            float normalsSize = 0.1f
        )
        {
            var meshResponse = _meshCreator.GetMeshResponse();
            
            var vertices = meshResponse.vertices;
            var normals = meshResponse.normals;
            var withBackfaceCulling = meshResponse.withBackfaceCulling;
            
            if (vertices.Length == 0)
            {
                return;
            }
            
            int verticesLength = meshResponse.BackfaceAdjustedVerticesLength;
            var showedVertexGroups = new List<int>(capacity: meshResponse.vertexGroups.Length);

            for (int i = 0; i < verticesLength; i++)
            {
                Vector3 actualVertexPosition = relativeTransform.TransformPoint(vertices[i]);

                Gizmos.color = vertexColor;
                Gizmos.DrawSphere(actualVertexPosition, vertexSize);

                VertexGroup targetGroup = meshResponse.GetGroupByVertexIndex(i);
                if (showVertexLabels && showedVertexGroups.Contains(targetGroup.selfIndex) == false)
                {
                    StringBuilder vertexLabel = new StringBuilder();

                    if (showDuplicatedVertexLabels)
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

                    Handles.Label(actualVertexPosition, vertexLabel.ToString());
                    showedVertexGroups.Add(targetGroup.selfIndex);
                }

                if (normals.Length != 0 && showVertexNormals)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawRay(
                        actualVertexPosition,
                        relativeTransform.TransformDirection(normalsSize * normals[i])
                    );
                    
                    if (withBackfaceCulling == false)
                    {
                        Gizmos.DrawRay(
                            actualVertexPosition,
                            relativeTransform.TransformDirection(normalsSize * normals[i + verticesLength])
                        );
                    }
                }
            }
        }
        
        public Mesh CreateMesh(in BaseMeshRequest request)
        {
            _meshCreator = request switch
            {
                PlaneMeshRequest => new PlaneMeshCreator(request as PlaneMeshRequest),
                CubeMeshRequest => new CubeMeshCreator(request as CubeMeshRequest),
                SphereMeshRequest => new SphereMeshCreator(request as SphereMeshRequest),
                _ => throw new ArgumentOutOfRangeException(nameof(request), "Not expected request value!")
            };

            return _meshCreator.CreateMesh();
        }
    }
}
