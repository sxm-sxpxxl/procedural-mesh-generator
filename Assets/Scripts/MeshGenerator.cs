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
    [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(isVerticesShowed)), LabelText("Color")]
    private Color vertexColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField, FoldoutGroup("Debug options"), Indent, ShowIf(nameof(isVerticesShowed)), LabelText("Size")]
    private float vertexSize = 0.01f;
    
    [Title("Generation")]
    [SerializeField] private MeshType meshType = MeshType.Plane;
    [SerializeField, Indent, ShowIf(nameof(IsPlaneMeshTypeSelected)), EnumToggleButtons] private Plane plane = Plane.XZ;
    [Space]
    [SerializeField] private Vector2 size = Vector2.one;
    [SerializeField] private Vector2 offset = Vector2.zero;
    [SerializeField, Range(1, 64)] private int resolution = 1;

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
        Plane
    }

    private readonly MeshCreatorContext _context = new MeshCreatorContext(new PlaneMeshCreator());

    private MeshFilter MeshFilter => _meshFilter ?? GetComponent<MeshFilter>();
    
    private IEnumerable<Type> GetModifierTypes() => typeof(VertexModifier).Assembly.GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => !x.IsGenericTypeDefinition)
        .Where(x => typeof(VertexModifier).IsAssignableFrom(x));

    private bool IsPlaneMeshTypeSelected => meshType == MeshType.Plane;
    
    private void OnDrawGizmos()
    {
        if (isVerticesShowed == false)
        {
            return;
        }
        
        var target = MeshFilter.sharedMesh.vertices;
        
        Gizmos.color = vertexColor;
        target.ForEach(vertex => Gizmos.DrawSphere(transform.position + vertex, vertexSize));
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
        MeshFilter.sharedMesh = _context.CreateMesh(new MeshCreator.MeshData(resolution, size, offset, plane));
    }
}
