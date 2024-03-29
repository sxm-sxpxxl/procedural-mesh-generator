using System;
using System.Collections.Generic;
using UnityEngine;
using Sxm.ProceduralMeshGenerator.Creation;
using Sxm.ProceduralMeshGenerator.Modification;
using Sxm.ProceduralMeshGenerator.Export.Editor;

namespace Sxm.ProceduralMeshGenerator
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent, ExecuteInEditMode]
    public sealed class ProceduralMeshGenerator : MonoBehaviour
    {
        [Serializable]
        public sealed class AppliedMeshModifier
        {
            [SerializeField] private bool isActive = true;
            [SerializeField] private BaseMeshModifier target;

            public bool IsActive => isActive;
            public BaseMeshModifier Target => target;

            public AppliedMeshModifier(BaseMeshModifier target)
            {
                this.target = target;
            }
        }
        
        public event Action OnMeshUpdated = delegate { };
        
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
        
        [SerializeField] private ColliderController.ColliderType colliderType = ColliderController.ColliderType.None;
        [SerializeField, HideInInspector] private ColliderController colliderController = new();
        
        [SerializeField] private List<AppliedMeshModifier> appliedModifiers = new();
        [SerializeField] private int selectedModifierIndex = -1;
        
        [SerializeField] private MeshExportFormat meshExportFormat;
        
        private MeshFilter _meshFilter;
        private InterstitialMeshData _meshData;
        private readonly MeshCreatorContext _meshCreatorContext = new();
        
        private static readonly Dictionary<MeshType, string> MeshTypeNames = new()
        {
            { MeshType.Plane, "Procedural Plane" },
            { MeshType.Cube, "Procedural Cube" },
            { MeshType.Sphere, "Procedural Sphere" }
        };
        
        private MeshFilter MeshFilter => _meshFilter == null ? _meshFilter = GetComponent<MeshFilter>() : _meshFilter;
        
        private AppliedMeshModifier SelectedModifier =>
            selectedModifierIndex >= 0 && selectedModifierIndex < appliedModifiers.Count
                ? appliedModifiers[selectedModifierIndex]
                : null;
        
#if UNITY_EDITOR
        private const string NoneValue = "None";
        
        public string VerticesDebugInfo => _meshData?.VerticesInfo ?? NoneValue;
        
        public string TrianglesDebugInfo => _meshData?.TrianglesInfo ?? NoneValue;
        
        public string BoundsDebugInfo => _meshData?.BoundsInfo ?? NoneValue;
        
        public Mesh MeshForExport => _meshData.MeshInstance;
        
        public MeshExportFormat MeshExportFormat => meshExportFormat;
        
        private void OnDrawGizmos()
        {
            _meshCreatorContext.DrawDebug(transform, debugData);
            
            var selectedModifier = SelectedModifier;
            if (selectedModifier != null && selectedModifier.Target != null && selectedModifier.IsActive)
            {
                selectedModifier.Target.DebugDraw();
            }
        }
#endif
        
        private void Update()
        {
            _meshData = CreateMeshByType();
            ModifyMeshVertices(_meshData);
            
            colliderController.Set(gameObject, colliderType, _meshData);
            MeshFilter.sharedMesh = _meshData.MeshInstance;
            
            OnMeshUpdated.Invoke();
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
