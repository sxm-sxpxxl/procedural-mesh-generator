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

        [SerializeField] private List<BaseMeshModifier> modifiers;

        private Transform _transform;
        private MeshFilter _meshFilter;
        private readonly MeshCreatorContext _context = new MeshCreatorContext();
        
        private static readonly Dictionary<MeshType, string> MeshTypeNames = new Dictionary<MeshType, string>
        {
            { MeshType.Plane, "Procedural Plane" },
            { MeshType.Cube, "Procedural Cube" },
            { MeshType.Sphere, "Procedural Sphere" }
        };
        
        private void OnDragGizmos()
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
            GenerateMesh();
            ModifyMesh();
        }
        
        public void AddModifier(BaseMeshModifier modifier)
        {
            modifiers.Add(modifier);
        }
        
        private void GenerateMesh()
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

            GetComponent<MeshFilter>().sharedMesh = _context.CreateMesh(request);
        }

        private void ModifyMesh()
        {
            var meshFilter = GetComponent<MeshFilter>();
            var vertices = meshFilter.sharedMesh.vertices;
            
            for (int i = 0; i < modifiers.Count; i++)
            {
                if (modifiers[i] == null || modifiers[i].IsActive == false)
                {
                    continue;
                }
                
                vertices = modifiers[i].Modify(transform, vertices);
            }
            
            meshFilter.sharedMesh.vertices = vertices;
            meshFilter.sharedMesh.RecalculateBounds();
            meshFilter.sharedMesh.RecalculateNormals();
        }
    }
}
