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
        var vertices = MeshFilter.sharedMesh.vertices;
        var normals = MeshFilter.sharedMesh.normals;
        
        if (vertices.Length == 0)
        {
            Debug.LogWarning("No vertices.");
            return;
        }
        
        if (areVerticesShowed == false)
        {
            return;
        }
        
        int verticesCount = isBackfaceCulling ? _context.Vertices.Length : _context.Vertices.Length / 2;
        for (int i = 0; i < verticesCount; i++)
        {
            Vector3 currentVertexPosition = transform.TransformPoint(_context.Vertices[i].position);

            Gizmos.color = vertexColor;
            Gizmos.DrawSphere(currentVertexPosition, vertexSize);

            if (isVertexLabelShowed)
            {
                StringBuilder vertexLabel = new StringBuilder();
                
                if (isDuplicatedVerticesShowed)
                {
                    vertexLabel.Append("V[");
                    vertexLabel.AppendJoin(',', _context.Vertices[i].indices);
                
                    if (isBackfaceCulling == false)
                    {
                        vertexLabel.Append($",{i + verticesCount}");
                    }
                
                    vertexLabel.Append(']');
                }
                else
                {
                    vertexLabel.Append($"V[{i}]");
                }
                
                Handles.Label(currentVertexPosition, vertexLabel.ToString());
            }
            
            if (normals.Length == 0)
            {
                // todo: refactoring (too many editor logs)
                // Debug.LogWarning("No vertex normals.");
            }
            else if (isVertexNormalShowed)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(currentVertexPosition, transform.TransformDirection(normalsSize * normals[i]));
                if (isBackfaceCulling == false)
                {
                    Gizmos.DrawRay(currentVertexPosition, transform.TransformDirection(normalsSize * normals[i + verticesCount]));
                }
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
        MeshFilter.sharedMesh = _context.CreateMesh(new MeshData(resolution, size, offset));
        isBackfaceCulling = true;
    }

    private void CreatePlane()
    {
        _context.Set(new PlaneMeshCreator());

        var virtualPlane = Plane.XY;
        var mesh = _context.CreateMesh(new MeshData(resolution, planeSize.AsFor(virtualPlane), offset, isForwardFacing, isBackfaceCulling));
        
        var vertices = mesh.vertices;
        var normals = mesh.normals;
        
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            vertices[i] = (vertices[i] - offset).AsFor(plane, virtualPlane) + offset;
            normals[i] = normals[i].AsFor(plane, virtualPlane);
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        
        MeshFilter.sharedMesh = mesh;
    }
}
