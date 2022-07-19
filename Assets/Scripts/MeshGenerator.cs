using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent, ExecuteAlways]
public sealed class MeshGenerator : SerializedMonoBehaviour
{
    [SerializeField, FoldoutGroup("Debug options"), LabelText("Show vertices")]
    private bool isVerticesShowed = true;
    [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(isVerticesShowed)), LabelText("Show labels")]
    private bool isVertexLabelShowed = true;
    [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(isVerticesShowed)), LabelText("Color")]
    private Color vertexColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(isVerticesShowed)), LabelText("Size")]
    private float vertexSize = 0.01f;
    
    [Title("Generation")]
    [SerializeField]
    private MeshType meshType = MeshType.Plane;
    [SerializeField, Indent, ShowIf(nameof(IsPlaneMeshTypeSelected)), EnumToggleButtons]
    private Plane plane = Plane.XZ;
    [SerializeField, Indent, ShowIf(nameof(IsPlaneMeshTypeSelected)), LabelText("Backface culling")]
    private bool isBackfaceCulling = true;
    [SerializeField, Indent(2), ShowIf(nameof(IsForwardFacingShowed)), LabelText("Forward facing")]
    private bool isForwardFacing = true;
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

    private enum MeshType
    {
        Plane,
        Cube
    }

    private readonly MeshCreatorContext _context = new MeshCreatorContext(new PlaneMeshCreator());

    private MeshFilter MeshFilter => _meshFilter ?? GetComponent<MeshFilter>();
    
    private IEnumerable<Type> GetModifierTypes() => typeof(VertexModifier).Assembly.GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => !x.IsGenericTypeDefinition)
        .Where(x => typeof(VertexModifier).IsAssignableFrom(x));

    private bool IsPlaneMeshTypeSelected => meshType == MeshType.Plane;

    private bool IsForwardFacingShowed => IsPlaneMeshTypeSelected && isBackfaceCulling;

    private void OnDrawGizmos()
    {
        if (isVerticesShowed == false)
        {
            return;
        }
        
        var vertices = MeshFilter.sharedMesh.vertices;
        Gizmos.color = vertexColor;

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(transform.position + vertices[i], vertexSize);

            if (isVertexLabelShowed) 
            {
                Handles.Label(transform.TransformPoint(vertices[i]), $"V[{i}]");
            }
        }
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
        _context.Set(new CubeMeshCreator());
        MeshFilter.sharedMesh = _context.CreateMesh(new MeshCreator.MeshData(resolution, size, offset));
    }

    private void CreatePlane()
    {
        _context.Set(new PlaneMeshCreator());

        var mesh = _context.CreateMesh(new MeshCreator.MeshData(resolution, planeSize.AsFor(Plane.XY), offset, isForwardFacing, isBackfaceCulling));
        var vertices = mesh.vertices;

        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices[i] = (vertices[i] - offset).AsFor(plane, tempIndex: 2) + offset;
        }

        mesh.vertices = vertices;
        MeshFilter.sharedMesh = mesh;
    }
}
