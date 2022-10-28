using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    [BurstCompile]
    public struct CollectTriangleNormalsByVerticesJob : IJob
    {
        private const int VerticesCountInTriangle = 3;
        
        [ReadOnly] public NativeArray<float3> vertices;
        [ReadOnly] public NativeArray<int> triangles;
        
        public NativeArray<UnsafeList<float3>> triangleNormalsByVertices;
        
        public void Execute()
        {
            int triangleIndex, vertexIndex;
            UnsafeList<float3> vertexTriangleNormals;
            
            for (int i = 0; i < triangles.Length; i++)
            {
                triangleIndex = (int) (i / VerticesCountInTriangle);
                vertexIndex = triangles[i];

                vertexTriangleNormals = triangleNormalsByVertices[vertexIndex];
                vertexTriangleNormals.Add(GetTriangleNormalByIndex(triangleIndex));
                triangleNormalsByVertices[vertexIndex] = vertexTriangleNormals;
            }
        }

        private float3 GetTriangleNormalByIndex(int triangleIndex)
        {
            int v1Index = triangles[VerticesCountInTriangle * triangleIndex + 0];
            int v2Index = triangles[VerticesCountInTriangle * triangleIndex + 1];
            int v3Index = triangles[VerticesCountInTriangle * triangleIndex + 2];

            float3 v1 = vertices[v1Index];
            float3 v2 = vertices[v2Index];
            float3 v3 = vertices[v3Index];

            float3 crossProduct = math.cross(v2 - v1, v3 - v1);
            return math.normalize(crossProduct);
        }
    }
}
