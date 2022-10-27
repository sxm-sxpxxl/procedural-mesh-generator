using Sxm.ProceduralMeshGenerator.Creation;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    [DisallowMultipleComponent]
    public abstract class BaseMeshModifier : MonoBehaviour
    {
        protected const int DebugVerticesResolution = 64;
        protected const float DebugDistanceBetweenVertices = 1f / DebugVerticesResolution;
        
        private Transform _meshTransform;
        private Transform _selfTransform;

        protected Transform MeshAxis
        {
            get
            {
                if (_meshTransform == null)
                {
                    _meshTransform = transform.parent;
                }
                
                return _meshTransform;
            }
        }

        protected Transform ModifierAxis
        {
            get
            {
                if (_selfTransform == null)
                {
                    _selfTransform = transform;
                }

                return _selfTransform;
            }
        }
        
        protected abstract void OnDrawGizmosSelected();
        
        public void Modify(InterstitialMeshData meshData)
        {
            if (MeshAxis == null)
            {
                Debug.LogWarning($"Modifier '{gameObject.name}' must be child of '{nameof(ProceduralMeshGenerator)}'");
                return;
            }
            
            var nativeVertices = NativeUtils.GetNativeArrayFrom(meshData.Vertices, Allocator.TempJob);
            var nativeBounds = NativeUtils.GetSingleNativeArrayFor((bounds) meshData.Bounds, Allocator.TempJob);
            
            var jobHandle = ApplyOn(nativeVertices);
            jobHandle = MeshUtils.RecalculateBounds(nativeBounds, nativeVertices, jobHandle);
            
            jobHandle.Complete();
            
            NativeUtils.SetNativeArrayTo(nativeVertices, meshData.Vertices);
            meshData.Bounds = nativeBounds[0];
            
            nativeVertices.Dispose();
            nativeBounds.Dispose();
        }
        
        protected abstract JobHandle ApplyOn(NativeArray<float3> vertices);
    }
}
