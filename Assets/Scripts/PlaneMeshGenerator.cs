using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent]
public sealed class PlaneMeshGenerator : SerializedMonoBehaviour
{
    [FoldoutGroup("Debug options"), SerializeField] private Color vertexColor = new Color(0f, 0f, 0f, 0.5f);
    [FoldoutGroup("Debug options"), SerializeField] private float vertexSize = 0.01f;
    
    [Title("Generation")]
    [SerializeField, EnumToggleButtons] private Plane plane = Plane.XZ;
    [SerializeField, Indent(1)] private Vector2 size = Vector2.one;
    [SerializeField, Indent(1)] private Vector2 offset = Vector2.zero;
    [SerializeField, Range(1, 64)] private int resolution = 1;

    [Title("Modification")]
    [SerializeField, TypeFilter(
         filterGetter: nameof(GetModifierTypes),
         DrawValueNormally = false,
         DropdownTitle = "Select a modifier"
     )] private VertexModifier[] modifiers;

    [NonSerialized] private MeshFilter _meshFilter;
    [NonSerialized] private Mesh _mesh;
    [NonSerialized] private Vector3[] _vertices;
    
    private IEnumerable<Type> GetModifierTypes() => typeof(VertexModifier).Assembly.GetTypes()
        .Where(x => !x.IsAbstract)
        .Where(x => !x.IsGenericTypeDefinition)
        .Where(x => typeof(VertexModifier).IsAssignableFrom(x));
    
    private void OnDrawGizmos()
    {
        var target = _vertices ?? GenerateVertices();
        
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

    private void Update()
    {
        GenerateMesh();
        ModifyMesh();
    }

    private void ModifyMesh()
    {
        modifiers.ForEach(modifier =>
        {
            _vertices = modifier.Init(plane, offset).Modify(_vertices);
        });

        _mesh.vertices = _vertices;
        
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
    }

    private void GenerateMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
        }

        _mesh.Clear();

        _mesh.vertices = _vertices = GenerateVertices();
        _mesh.uv = _vertices.Select(x => x.ExtractFor(plane) - (-0.5f * new Vector2(size.x, size.y) + offset)).ToArray();
        _mesh.triangles = GenerateTriangles(triangleVertexIndex =>
            triangleVertexIndex % 2 == 0 ? triangleVertexIndex / 2 : triangleVertexIndex / 2 + (resolution + 1)
        );

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

        _meshFilter.mesh = _mesh;
    }

    private int[] GenerateTriangles(Func<int, int> getActualVertexIndex)
    {
        int indicesCount = 6 * resolution * resolution;
        var indices = new int[indicesCount];

        int triangleVertexIndex = 0, repeatedIndex = 0;
        for (int i = 0; i < indicesCount; i += 6)
        {
            indices[i + 0] = getActualVertexIndex(triangleVertexIndex + 0);
            indices[i + 1] = getActualVertexIndex(triangleVertexIndex + 1);
            indices[i + 2] = getActualVertexIndex(triangleVertexIndex + 2);
            
            indices[i + 3] = getActualVertexIndex(triangleVertexIndex + 1);
            indices[i + 4] = getActualVertexIndex(triangleVertexIndex + 3);
            indices[i + 5] = getActualVertexIndex(triangleVertexIndex + 2);

            repeatedIndex = (int) Mathf.Repeat(triangleVertexIndex, 2 * (resolution + 1));
            triangleVertexIndex += 2 * (1 + (repeatedIndex / 2 % resolution) / Mathf.Max(resolution - 1, 1));
        }
        
        return indices;
    }

    private Vector3[] GenerateVertices()
    {
        int pointsCount = (resolution + 1) * (resolution + 1);
        var points = new Vector3[pointsCount];

        Vector2 initialPoint = -0.5f * new Vector2(size.x, size.y) + offset;
        points[0] = initialPoint.AsFor(plane);
        
        for (int i = 1; i < pointsCount; i++)
        {
            float x = size.x / resolution * (i % (resolution + 1));
            float y = size.y / resolution * (i / (resolution + 1));

            points[i] = (initialPoint + new Vector2(x, y)).AsFor(plane);
        }

        return points;
    }
}
