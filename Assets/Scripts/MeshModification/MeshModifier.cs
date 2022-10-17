using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    [DisallowMultipleComponent]
    public abstract class MeshModifier : MonoBehaviour
    {
        protected const int DebugVerticesResolution = 64;
        protected const float DebugDistanceBetweenVertices = 1f / DebugVerticesResolution;
    
        [SerializeField, HideInInspector] protected Transform meshTransform;
        [SerializeField, HideInInspector] protected bool isActive = true;
    
        private Transform _axis;
        protected Transform Axis => _axis ? _axis : transform;

        protected virtual void OnDrawGizmosSelected() { }

        private void Awake()
        {
            _axis = transform;
        }

        public abstract Vector3[] Modify(in Vector3[] vertices);
    }
}
