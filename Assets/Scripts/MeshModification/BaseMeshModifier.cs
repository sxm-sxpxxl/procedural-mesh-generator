using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    [DisallowMultipleComponent]
    public abstract class BaseMeshModifier : MonoBehaviour
    {
        protected const int DebugVerticesResolution = 64;
        protected const float DebugDistanceBetweenVertices = 1f / DebugVerticesResolution;
        
        [SerializeField, HideInInspector] protected Transform meshTransform;
        
        private Transform _axis;
        
        protected Transform Axis => _axis ? _axis : transform;
        
        private void Awake()
        {
            _axis = transform;
        }
        
        protected abstract void OnDrawGizmosSelected();
        
        public BaseMeshModifier Init(Transform meshTransform)
        {
            this.meshTransform = meshTransform;
            return this;
        }
        
        public abstract Vector3[] Modify(in Vector3[] vertices);
    }
}
