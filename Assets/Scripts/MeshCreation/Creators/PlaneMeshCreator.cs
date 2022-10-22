using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    internal sealed class PlaneMeshCreator : BaseMeshCreator<PlaneMeshRequest>
    {
        public PlaneMeshCreator(in PlaneMeshRequest request) : base(in request) { }
        
        protected override MeshResponse Handle(PlaneMeshRequest request)
        {
            int resolution = request.resolution;
            float distanceBetweenVertices = 1f / resolution;
            int edgeVerticesCount = resolution + 1;
            int quadVerticesCount = edgeVerticesCount * edgeVerticesCount;

            CreateVertices(
                vertexGroupsCount: quadVerticesCount,
                excludedVertexGroupsCount: 0,
                getVertexPointByIndex: i => distanceBetweenVertices * new Vector3(
                    x: i % edgeVerticesCount,
                    y: (int) (i / edgeVerticesCount)
                )
            );
            
            SetTriangles(new FaceData[]
            {
                new FaceData(
                    RotationDirection.CW,
                    i => i,
                    () => request.isForwardFacing ? Vector3.back : Vector3.forward,
                    i => distanceBetweenVertices * new Vector2(
                        x: i % edgeVerticesCount,
                        y: (int) (i / edgeVerticesCount)
                    )
                )
            });
            
            ScaleAndOffset();
            TransformTo(request.axis, request.offset);
            
            return response;
        }

        private void TransformTo(Plane axis, Vector3 offset)
        {
            var vertices = response.vertices;
            var normals = response.normals;
                    
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = (vertices[i] - offset).AsFor(axis, PlaneMeshRequest.VirtualAxis) + offset;
                normals[i] = normals[i].AsFor(axis, PlaneMeshRequest.VirtualAxis);
            }
        }
    }
}
