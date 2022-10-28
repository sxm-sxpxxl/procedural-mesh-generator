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
            
            var nativeVertices = NativeUtils.CreateNativeArrayFrom<Vector3, float3>(meshData.Vertices, Allocator.TempJob);
            var nativeTriangles = NativeUtils.CreateNativeArrayFrom(meshData.Triangles, Allocator.TempJob);
            
            ApplyOn(nativeVertices).Complete();
            NativeUtils.CopyNativeArrayTo(nativeVertices, meshData.Vertices);
            
            meshData.Bounds = MeshUtils.RecalculateBounds(nativeVertices);
            MeshUtils.RecalculateNormals(meshData.Normals, nativeVertices, nativeTriangles);
            
            nativeVertices.Dispose();
            nativeTriangles.Dispose();
        }
        
        protected abstract JobHandle ApplyOn(NativeArray<float3> vertices);
    }
}
