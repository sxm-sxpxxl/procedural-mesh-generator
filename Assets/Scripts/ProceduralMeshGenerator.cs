using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sxm.ProceduralMeshGenerator.Creation;
using Sxm.ProceduralMeshGenerator.Modification;

namespace Sxm.ProceduralMeshGenerator
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent, ExecuteAlways]
    public sealed class ProceduralMeshGenerator : MonoBehaviour
    {
        [SerializeField, FoldoutGroup("Debug options"), LabelText("Show vertices")]
        private bool areVerticesShowed = true;
        [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(areVerticesShowed)), LabelText("Color")]
        private Color vertexColor = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(areVerticesShowed)), LabelText("Size")]
        [Range(0.01f, 1f)]
        private float vertexSize = 0.01f;
        [Space]
        [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(areVerticesShowed)), LabelText("Show labels")]
        private bool isVertexLabelShowed = true;
        [SerializeField, FoldoutGroup("Debug options"), Indent(2), ShowIf(nameof(areVerticesShowed)), LabelText("Show duplicated vertices")]
        private bool isDuplicatedVerticesShowed = true;
        [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(areVerticesShowed)), LabelText("Show normals")]
        private bool isVertexNormalShowed = true;
        [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(areVerticesShowed)), LabelText("Normals size")]
        [Range(0.1f, 2f)]
        private float normalsSize = 1f;

        [Title("Generation")]
        [SerializeField]
        private MeshType meshType = MeshType.Plane;
        [SerializeField, Indent, ShowIf(nameof(IsPlaneMeshTypeSelected)), EnumToggleButtons]
        private Plane plane = Plane.XZ;
        [SerializeField, Indent, ShowIf(nameof(IsPlaneMeshTypeSelected)), LabelText("Backface culling")]
        private bool isBackfaceCulling = true;
        [SerializeField, Indent(2), ShowIf(nameof(IsForwardFacingShowed)), LabelText("Forward facing")]
        private bool isForwardFacing = true;
        [SerializeField, Indent, ShowIf(nameof(IsCubeMeshTypeSelected)), Range(0f, 1f)]
        private float roundness = 0f;
        [Space]
        [SerializeField, HideIf(nameof(IsPlaneMeshTypeSelected))]
        private Vector3 size = Vector3.one;
        [SerializeField, LabelText("Size"), ShowIf(nameof(IsPlaneMeshTypeSelected))]
        private Vector2 planeSize = Vector2.one;
        [SerializeField]
        private Vector3 offset = Vector3.zero;
        [SerializeField, Range(1, 32)]
        private int resolution = 1;
        
        [Header("Modification")]
        [SerializeField] private MeshModifier meshModifier;

        [NonSerialized] private MeshFilter _meshFilter;

        private readonly MeshCreatorContext _context = new MeshCreatorContext(MeshType.Plane);

        private MeshFilter MeshFilter => _meshFilter ? _meshFilter : GetComponent<MeshFilter>();
        
        private bool IsPlaneMeshTypeSelected => meshType == MeshType.Plane;
        
        private bool IsCubeMeshTypeSelected => meshType == MeshType.Cube;
        
        private bool IsForwardFacingShowed => IsPlaneMeshTypeSelected && isBackfaceCulling;

        private static readonly Dictionary<MeshType, string> _meshTypeNames = new Dictionary<MeshType, string>
        {
            { MeshType.Plane, "Procedural Plane" },
            { MeshType.Cube, "Procedural Cube" },
            { MeshType.Sphere, "Procedural Sphere" }
        };
        
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
                normalsSize
            );
        }
        
        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
        }

        private void OnRenderObject()
        {
            GenerateMesh();
            ModifyMesh();
        }

        private void ModifyMesh()
        {
            MeshFilter.sharedMesh.vertices = meshModifier.Modify(MeshFilter.sharedMesh.vertices);
            MeshFilter.sharedMesh.RecalculateBounds();
            MeshFilter.sharedMesh.RecalculateNormals();
        }

        private void GenerateMesh()
        {
            if (meshType == MeshType.Plane) CreatePlane();
            if (meshType == MeshType.Cube) CreateCube();
            if (meshType == MeshType.Sphere) CreateSphere();
        }

        private void CreateSphere()
        {
            MeshFilter.sharedMesh = _context.SetType(MeshType.Sphere).CreateMesh(new MeshRequest(
                _meshTypeNames[MeshType.Sphere],
                resolution,
                size,
                offset
            ));

            isBackfaceCulling = true;
        }

        private void CreateCube()
        {
            MeshFilter.sharedMesh = _context.SetType(MeshType.Cube).CreateMesh(new MeshRequest(
                _meshTypeNames[MeshType.Cube],
                resolution,
                size,
                offset,
                customData: (object) roundness
            ));
            
            isBackfaceCulling = true;
        }

        private void CreatePlane()
        {
            var virtualPlane = Plane.XY;
            MeshFilter.sharedMesh = _context.SetType(MeshType.Plane).CreateMesh(new MeshRequest(
                _meshTypeNames[MeshType.Plane],
                resolution,
                planeSize.AsFor(virtualPlane),
                offset,
                isScalingAndOffsetting: true,
                isForwardFacing,
                isBackfaceCulling,
                meshResponse =>
                {
                    var vertices = meshResponse.vertices;
                    var normals = meshResponse.normals;
                    
                    for (int i = 0; i < vertices.Length; i++)
                    {
                        vertices[i] = (vertices[i] - offset).AsFor(plane, virtualPlane) + offset;
                        normals[i] = normals[i].AsFor(plane, virtualPlane);
                    }
                    
                    return meshResponse;
                }
            ));
        }
    }
}
