using System;
using System.Collections.Generic;
using UnityEngine;
using Sxm.ProceduralMeshGenerator.Creation;
using Sxm.ProceduralMeshGenerator.Modification;

namespace Sxm.ProceduralMeshGenerator
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent, ExecuteAlways]
    public sealed class ProceduralMeshGenerator : MonoBehaviour
    {
        [Serializable]
        public sealed class AppliedMeshModifier
        {
            [SerializeField] private bool isActive = true;
            [SerializeField] private BaseMeshModifier target = null;

            public bool IsActive => isActive;
            public BaseMeshModifier Target => target;

            public AppliedMeshModifier(BaseMeshModifier target)
            {
                this.target = target;
            }
        }
        
        [SerializeField] private MeshCreatorContext.DebugData debugData;
        [SerializeField] private MeshType meshType = MeshType.Plane;
        [SerializeField] private Plane planeAxis = Plane.XZ;
        [SerializeField] private bool isBackfaceCulling = true;
        [SerializeField] private bool isForwardFacing = true;
        [SerializeField] private float roundness = 0f;
        
        [SerializeField] private Vector2 size2d = Vector2.one;
        [SerializeField] private Vector3 size3d = Vector3.one;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private int resolution = 1;
        
        [SerializeField] private List<AppliedMeshModifier> appliedModifiers;
        [SerializeField] private int selectedModifierIndex = -1;
        
        private MeshFilter _meshFilter;
        private readonly MeshCreatorContext _meshCreatorContext = new MeshCreatorContext();
        
        private static readonly Dictionary<MeshType, string> MeshTypeNames = new Dictionary<MeshType, string>
        {
            { MeshType.Plane, "Procedural Plane" },
            { MeshType.Cube, "Procedural Cube" },
            { MeshType.Sphere, "Procedural Sphere" }
        };

        private AppliedMeshModifier SelectedModifier =>
            selectedModifierIndex >= 0 && selectedModifierIndex < appliedModifiers.Count
                ? appliedModifiers[selectedModifierIndex]
                : null;
        private MeshFilter MeshFilter => _meshFilter ? _meshFilter : GetComponent<MeshFilter>();
        
        private void OnDrawGizmos()
        {
            _meshCreatorContext.DrawDebug(transform, debugData);
            
            var selectedModifier = SelectedModifier;
            if (selectedModifier != null && selectedModifier.Target != null && selectedModifier.IsActive)
            {
                selectedModifier.Target.DebugDraw();
            }
        }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
        }
        
        private void OnRenderObject()
        {
            var meshData = CreateMeshByType();
            ModifyMeshVertices(meshData);
            
            var meshFilter = MeshFilter;
            meshFilter.sharedMesh = meshData.MeshInstance;
        }
        
        public void AddModifier(BaseMeshModifier modifier)
        {
            appliedModifiers.Add(new AppliedMeshModifier(modifier));
        }
        
        private InterstitialMeshData CreateMeshByType()
        {
            BaseMeshRequest request = meshType switch
            {
                MeshType.Plane => new PlaneMeshRequest(
                    MeshTypeNames[meshType],
                    resolution,
                    size2d,
                    planeAxis,
                    offset,
                    isScalingAndOffsetting: true,
                    isBackfaceCulling,
                    isForwardFacing
                ),
                MeshType.Cube => new CubeMeshRequest(
                    MeshTypeNames[meshType],
                    resolution,
                    size3d,
                    offset,
                    roundness
                ),
                MeshType.Sphere => new SphereMeshRequest(
                    MeshTypeNames[meshType],
                    resolution,
                    size3d,
                    offset
                ),
                _ => throw new ArgumentOutOfRangeException(nameof(meshType), "Not expected mesh type!")
            };
            
            return _meshCreatorContext.CreateMeshData(request);
        }

        private void ModifyMeshVertices(InterstitialMeshData meshData)
        {
            for (int i = 0; i < appliedModifiers.Count; i++)
            {
                if (appliedModifiers[i].Target == null || appliedModifiers[i].IsActive == false)
                {
                    continue;
                }

                appliedModifiers[i].Target.Modify(meshData);
            }
        }
    }
}
