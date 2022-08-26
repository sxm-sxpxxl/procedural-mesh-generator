using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MeshCreation;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent, ExecuteAlways]
public sealed class MeshGenerator : SerializedMonoBehaviour
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

    [Title("Modification")]
    [SerializeField, LabelText("Enabled")] private bool isModificationEnabled = true;
    [Space]
    [SerializeField, EnableIf(nameof(isModificationEnabled)), TypeFilter(
         filterGetter: nameof(GetModifierTypes),
         DrawValueNormally = false,
         DropdownTitle = "Select a modifier"
    )] private VertexModifier[] modifiers;

    [NonSerialized] private MeshFilter _meshFilter;

    private readonly MeshCreatorContext _context = new MeshCreatorContext(MeshType.Plane);

    private MeshFilter MeshFilter => _meshFilter ? _meshFilter : GetComponent<MeshFilter>();
    
    private IEnumerable<Type> GetModifierTypes() => typeof(VertexModifier).Assembly.GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => !x.IsGenericTypeDefinition)
        .Where(x => typeof(VertexModifier).IsAssignableFrom(x));

    private bool IsPlaneMeshTypeSelected => meshType == MeshType.Plane;

    private bool IsCubeMeshTypeSelected => meshType == MeshType.Cube;

    private bool IsForwardFacingShowed => IsPlaneMeshTypeSelected && isBackfaceCulling;

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
    
    private void OnDrawGizmosSelected()
    {
        modifiers.ForEach(modifier =>
        {
            modifier.Init(plane, offset).OnDrawGizmosSelected(transform);
        });
    }

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void OnRenderObject()
    {
        GenerateMesh();

        if (isModificationEnabled)
        {
            ModifyMesh();
        }
    }

    private void ModifyMesh()
    {
        modifiers.ForEach(modifier =>
        {
            MeshFilter.sharedMesh.vertices = modifier.Init(plane, offset).Modify(MeshFilter.sharedMesh.vertices);
        });

        MeshFilter.sharedMesh.RecalculateBounds();
        MeshFilter.sharedMesh.RecalculateNormals();
    }

    private void GenerateMesh()
    {
        if (meshType == MeshType.Plane) CreatePlane();
        if (meshType == MeshType.Cube) CreateCube();
    }

    private void CreateCube()
    {
        MeshFilter.sharedMesh = _context.Set(MeshType.Cube).CreateMesh(new MeshRequest(
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
        MeshFilter.sharedMesh = _context.Set(MeshType.Plane).CreateMesh(new MeshRequest(
            resolution,
            planeSize.AsFor(virtualPlane),
            offset,
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
