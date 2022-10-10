using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    internal sealed class SphereMeshCreator : MeshCreator
    {
        protected override MeshResponse HandleRequest()
        {
            LastMeshResponse = new CubeMeshCreator().GetResponseTo(new MeshRequest(
                meshRequest.name,
                meshRequest.resolution,
                size: Vector3.one,
                offset: Vector3.zero,
                isScalingAndOffsetting: false,
                customData: (object) 0f,
                postProcessCallback: meshResponse =>
                {
                    var vertices = meshResponse.vertices;
                    var normals = meshResponse.normals;
                    
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        var pointOnCube = 2f * vertices[i] - Vector3.one;
                        var sqr = Vector3.Scale(pointOnCube, pointOnCube);

                        var pointOnSphere = new Vector3(
                            x: pointOnCube.x * Mathf.Sqrt(1f - sqr.y / 2f - sqr.z / 2f + sqr.y * sqr.z / 3f),
                            y: pointOnCube.y * Mathf.Sqrt(1f - sqr.x / 2f - sqr.z / 2f + sqr.x * sqr.z / 3f),
                            z: pointOnCube.z * Mathf.Sqrt(1f - sqr.x / 2f - sqr.y / 2f + sqr.x * sqr.y / 3f)
                        );
                        
                        normals[i] = pointOnSphere;
                        vertices[i] = 0.5f * (Vector3.one + pointOnSphere);
                    }
                    
                    return meshResponse;
                }
            ));

            ScaleAndOffset();
            return LastMeshResponse;
        }
    }
}
