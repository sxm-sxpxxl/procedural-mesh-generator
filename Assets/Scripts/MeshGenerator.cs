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
        var sharedMesh = MeshFilter.sharedMesh;
        var vertices = sharedMesh.vertices;
        var normals = sharedMesh.normals;
        
        if (vertices.Length == 0)
        {
            Debug.LogWarning("No vertices.");
            return;
        }
        
        if (areVerticesShowed == false)
        {
            return;
        }
        
        var verticesData = _context.VerticesData;
        int verticesCount = isBackfaceCulling ? verticesData.vertices.Length : verticesData.vertices.Length / 2;
        var showedVertexGroups = new List<int>(capacity: verticesData.vertexGroups.Length);
        
        for (int i = 0; i < verticesCount; i++)
        {
            Vector3 actualVertexPosition = transform.TransformPoint(vertices[i]);

            Gizmos.color = vertexColor;
            Gizmos.DrawSphere(actualVertexPosition, vertexSize);
            
            VertexGroup targetGroup = verticesData.GetGroupByVertexIndex(i);
            if (isVertexLabelShowed && showedVertexGroups.Contains(targetGroup.selfIndex) == false)
            {
                StringBuilder vertexLabel = new StringBuilder();
                
                if (isDuplicatedVerticesShowed)
                {
                    vertexLabel.Append("V[");
                    
                    if (targetGroup.hasSingleVertex)
                    {
                        vertexLabel.Append(targetGroup.singleIndex);
                    }
                    else
                    {
                        vertexLabel.AppendJoin(',', targetGroup.indices);
                    }

                    if (isBackfaceCulling == false)
                    {
                        vertexLabel.Append($",{i + verticesCount}");
                    }
                
                    vertexLabel.Append(']');
                }
                else
                {
                    vertexLabel.Append($"V[{targetGroup.selfIndex}]");
                }
                
                Handles.Label(actualVertexPosition, vertexLabel.ToString());
                showedVertexGroups.Add(targetGroup.selfIndex);
            }
            
            if (normals.Length == 0)
            {
                // todo: refactoring (too many editor logs)
                // Debug.LogWarning("No vertex normals.");
            }
            else if (isVertexNormalShowed)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(actualVertexPosition, transform.TransformDirection(normalsSize * normals[i]));
                if (isBackfaceCulling == false)
                {
                    Gizmos.DrawRay(actualVertexPosition, transform.TransformDirection(normalsSize * normals[i + verticesCount]));
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
        MeshFilter.sharedMesh = _context.CreateMesh(new MeshData(
            resolution,
            planeSize.AsFor(virtualPlane),
            offset,
            isForwardFacing,
            isBackfaceCulling,
            mesh =>
            {
                var vertices = mesh.vertices;
                var normals = mesh.normals;
                
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i] = (vertices[i] - offset).AsFor(plane, virtualPlane) + offset;
                    normals[i] = normals[i].AsFor(plane, virtualPlane);
                }

                mesh.vertices = vertices;
                mesh.normals = normals;

                return mesh;
            }
        ));
    }
}
