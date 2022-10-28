using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    internal static class NativeUtils
    {
        public static unsafe NativeArray<T> CreateNativeArrayFrom<T>(T[] managedArray, Allocator allocator)
            where T : unmanaged
            => CreateNativeArrayFrom<T, T>(managedArray, allocator);
        
        public static unsafe NativeArray<TTo> CreateNativeArrayFrom<TFrom, TTo>(TFrom[] managedArray, Allocator allocator)
            where TFrom : unmanaged
            where TTo : struct
        {
            var nativeArray = new NativeArray<TTo>(managedArray.Length, allocator, NativeArrayOptions.UninitializedMemory);

            fixed (void* managedPointer = managedArray)
            {
                UnsafeUtility.MemCpy(
                    NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray),
                    managedPointer,
                    managedArray.Length * (long) UnsafeUtility.SizeOf<TTo>()
                );
            }
            
            return nativeArray;
        }
        
        public static unsafe void CopyNativeArrayTo<TFrom, TTo>(NativeArray<TFrom> nativeArray, TTo[] managedArray)
            where TFrom : struct
            where TTo : unmanaged
        {
            fixed (void* managedPointer = managedArray)
            {
                UnsafeUtility.MemCpy(
                    managedPointer, 
                    NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(nativeArray),
                    nativeArray.Length * (long) UnsafeUtility.SizeOf<TFrom>()
                );
            }
        }
    }
}
