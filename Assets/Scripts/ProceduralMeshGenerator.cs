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
        
        [SerializeField] private bool areVerticesShowed = false;
        [SerializeField] private Color vertexColor = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField] private float vertexSize = 0.01f;

        [SerializeField] private bool isVertexLabelShowed = true;
        [SerializeField] private bool isDuplicatedVerticesShowed = true;
        [SerializeField] private bool isVertexNormalShowed = true;
        [SerializeField] private float normalSize = 1f;

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

        private Transform _transform;
        private MeshFilter _meshFilter;
        private readonly MeshCreatorContext _context = new MeshCreatorContext();
        
        private static readonly Dictionary<MeshType, string> MeshTypeNames = new Dictionary<MeshType, string>
        {
            { MeshType.Plane, "Procedural Plane" },
            { MeshType.Cube, "Procedural Cube" },
            { MeshType.Sphere, "Procedural Sphere" }
        };

        private MeshFilter MeshFilter => _meshFilter ? _meshFilter : GetComponent<MeshFilter>();
        
        private void OnDrawGizmos()
        {
            if (areVerticesShowed == false)
            {
                return;
            }
            
            _context.DrawDebug(
                transform,
                vertexSize,
                vertexColor,
                isVertexLabelShowed,
                isDuplicatedVerticesShowed,
                isVertexNormalShowed,
                normalSize
            );
        }

        private void Awake()
        {
            _transform = transform;
            _meshFilter = GetComponent<MeshFilter>();
        }

        private void OnRenderObject()
        {
            var meshResponse = GenerateMeshByType();
            ModifyMeshVertices(meshResponse.vertices);

            var meshFilter = MeshFilter;
            meshFilter.sharedMesh = meshResponse.MeshInstance;
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.RecalculateNormals();
        }
        
        public void AddModifier(BaseMeshModifier modifier)
        {
            appliedModifiers.Add(new AppliedMeshModifier(modifier));
        }
        
        private MeshResponse GenerateMeshByType()
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
            
            return _context.CreateMesh(request);
        }

        private void ModifyMeshVertices(Vector3[] vertices)
        {
            for (int i = 0; i < appliedModifiers.Count; i++)
            {
                if (appliedModifiers[i].Target == null || appliedModifiers[i].IsActive == false)
                {
                    continue;
                }

                appliedModifiers[i].Target.Modify(vertices);
            }
        }
    }
}
