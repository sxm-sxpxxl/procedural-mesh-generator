using System;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent]
public sealed class PlaneMeshGenerator : MonoBehaviour
{
    [FoldoutGroup("Debug options"), SerializeField] private Color vertexColor = new Color(0f, 0f, 0f, 0.5f);
    [FoldoutGroup("Debug options"), SerializeField] private float vertexSize = 0.01f;
    
    [Title("Common options")]
    [SerializeField, EnumToggleButtons] private Plane plane = Plane.XZ;
    [SerializeField, Indent(1)] private Vector2 size = Vector2.one;
    [SerializeField, Indent(1)] private Vector2 offset = Vector2.zero;
    [SerializeField, Range(1, 32)] private int resolution = 1;

    private MeshFilter _meshFilter;
    private Mesh _mesh;

    private enum Plane
    {
        XY,
        YZ,
        XZ
    }
    
    private void OnDrawGizmos()
    {
        var vertices = GenerateVertices();

        Gizmos.color = vertexColor;
        foreach (var vertex in vertices)
        {
            Gizmos.DrawSphere(vertex, vertexSize);
        }
    }

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    private void Update()
    {
        GenerateMesh();
    }

    private void GenerateMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
        }

        _mesh.Clear();

        _mesh.vertices = GenerateVertices();
        _mesh.triangles = GenerateTriangles(triangleVertexIndex =>
            triangleVertexIndex % 2 == 0 ? triangleVertexIndex / 2 : triangleVertexIndex / 2 + (resolution + 1)
        );
        
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
        points[0] = ConvertTo3D(initialPoint);
        
        for (int i = 1; i < pointsCount; i++)
        {
            float x = size.x / resolution * (i % (resolution + 1));
            float y = size.y / resolution * (i / (resolution + 1));

            points[i] = ConvertTo3D(initialPoint + new Vector2(x, y));
        }

        return points;
    }

    private Vector3 ConvertTo3D(Vector2 vec) => plane switch
    {
        Plane.XY => new Vector3(vec.x, vec.y, 0f),
        Plane.YZ => new Vector3(0f, vec.x, vec.y),
        Plane.XZ => new Vector3(vec.x, 0f, vec.y),
        _ => Vector3.zero
    };
}
