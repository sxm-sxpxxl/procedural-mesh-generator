using System;
using System.Diagnostics;
using UnityEngine;
using UnityEditorInternal;
using Sxm.ProceduralMeshGenerator.Creation;

namespace Sxm.ProceduralMeshGenerator
{
    [Serializable]
    public sealed class ColliderController
    {
        public enum ColliderType
        {
            None,
            Bounds,
            Mesh
        }
        
        [SerializeField] private Collider selected;
        
        [Conditional("UNITY_EDITOR")]
        public void Set(GameObject target, ColliderType type, InterstitialMeshData meshData)
        {
            switch (type)
            {
                case ColliderType.None:
                    Reset();
                    break;
                case ColliderType.Bounds:
                {
                    var boxCollider = Select<BoxCollider>(target);
                    boxCollider.center = meshData.Bounds.center;
                    boxCollider.size = meshData.Bounds.size;
                    break;
                }
                case ColliderType.Mesh:
                {
                    var meshCollider = Select<MeshCollider>(target);
                    meshCollider.sharedMesh = meshData.MeshInstance;
                    break;
                }
            }
        }
        
        private T Select<T>(GameObject target) where T : Collider
        {
            if (selected as T != null)
            {
                return selected as T;
            }
            
            Reset();
            
            var newCollider = target.AddComponent<T>();
            ComponentUtility.MoveComponentUp(newCollider);
            selected = newCollider;
            
            return newCollider;
        }
        
        private void Reset()
        {
            if (selected != null)
            {
                UnityEngine.Object.DestroyImmediate(selected);
                selected = null;
            }
        }
    }
}
