using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    [BurstCompile]
    public struct AveragingTriangleNormalsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<UnsafeList<float3>> triangleNormalsByVertices;
        [WriteOnly] public NativeArray<float3> normals;
        
        public void Execute(int index)
        {
            UnsafeList<float3> vertexTriangleNormals = triangleNormalsByVertices[index];
            
            float3 averageNormal = vertexTriangleNormals[0];
            for (int i = 1; i < vertexTriangleNormals.Length; i++)
            {
                averageNormal += vertexTriangleNormals[i];
            }
            averageNormal /= vertexTriangleNormals.Length;
            
            normals[index] = math.normalize(averageNormal);
        }
    }
}
