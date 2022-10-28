using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    public static class MeshUtils
    {
        private const int MaxTrianglesByVertexFor3D = 6;
        
        public static void RecalculateNormals(Vector3[] normals, NativeArray<float3> nativeVertices, NativeArray<int> nativeTriangles)
        {
            var nativeNormals = new NativeArray<float3>(normals.Length, Allocator.TempJob);
            var triangleNormalsByVertices = new NativeArray<UnsafeList<float3>>(nativeVertices.Length, Allocator.TempJob);
            for (int i = 0; i < triangleNormalsByVertices.Length; i++)
            {
                triangleNormalsByVertices[i] = new UnsafeList<float3>(
                    MaxTrianglesByVertexFor3D,
                    Allocator.TempJob,
                    NativeArrayOptions.UninitializedMemory
                );
            }
            
            var jobHandle = new CollectTriangleNormalsByVerticesJob
            {
                vertices = nativeVertices,
                triangles = nativeTriangles,
                triangleNormalsByVertices = triangleNormalsByVertices,
            }.Schedule();
            
            jobHandle = new AveragingTriangleNormalsJob
            {
                triangleNormalsByVertices = triangleNormalsByVertices,
                normals = nativeNormals
            }.Schedule(nativeNormals.Length, 0, jobHandle);
            
            jobHandle.Complete();
            NativeUtils.CopyNativeArrayTo(nativeNormals, normals);
            
            for (int i = 0; i < triangleNormalsByVertices.Length; i++)
            {
                triangleNormalsByVertices[i].Dispose();
            }
            triangleNormalsByVertices.Dispose();
            nativeNormals.Dispose();
        }
        
        public static Bounds RecalculateBounds(NativeArray<float3> nativeVertices)
        {
            var nativeBounds = new NativeReference<bounds>(new bounds(), AllocatorManager.TempJob);
            
            new RecalculateBoundsJob
            {
                bounds = nativeBounds,
                vertices = nativeVertices
            }.Schedule().Complete();
            
            var newBounds = (Bounds) nativeBounds.Value;
            nativeBounds.Dispose();
            
            return newBounds;
        }
    }
}
