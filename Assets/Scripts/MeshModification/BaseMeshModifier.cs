﻿using UnityEngine;

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
        
        // todo #3: change reference pass to mesh transform
        public abstract Vector3[] Modify(in Transform target, in Vector3[] vertices);
    }
}
