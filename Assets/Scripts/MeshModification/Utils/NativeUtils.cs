using UnityEngine;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    internal static class NativeUtils
    {
        public static NativeArray<T> GetSingleNativeArrayFor<T>(T value, Allocator allocator) where T : struct
        {
            var singleArray = new NativeArray<T>(1, allocator, NativeArrayOptions.UninitializedMemory);
            singleArray[0] = value;
            
            return singleArray;
        }
        
        public static unsafe NativeArray<float3> GetNativeArrayFrom(Vector3[] managedArray, Allocator allocator)
        {
            var nativeArray = new NativeArray<float3>(managedArray.Length, allocator, NativeArrayOptions.UninitializedMemory);

            fixed (void* managedPointer = managedArray)
            {
                UnsafeUtility.MemCpy(
                    NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray),
                    managedPointer,
                    managedArray.Length * (long) UnsafeUtility.SizeOf<float3>()
                );
            }
            
            return nativeArray;
        }

        public static unsafe void SetNativeArrayTo(NativeArray<float3> nativeArray, Vector3[] managedArray)
        {
            fixed (void* managedPointer = managedArray)
            {
                UnsafeUtility.MemCpy(
                    managedPointer, 
                    NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray),
                    nativeArray.Length * (long) UnsafeUtility.SizeOf<float3>()
                );
            }
        }
    }
}
