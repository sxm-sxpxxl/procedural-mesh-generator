using System.Collections.Generic;
using UnityEngine;
using Sxm.ProceduralMeshGenerator.Creation;
using Sxm.ProceduralMeshGenerator.Modification;

namespace Sxm.ProceduralMeshGenerator
{
    [RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshRenderer))]
    [DisallowMultipleComponent]
    // todo: ExecuteAlways?
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
        [SerializeField] private Plane plane = Plane.XZ;
        [SerializeField] private bool isBackfaceCulling = true;
        [SerializeField] private bool isForwardFacing = true;
        [SerializeField] private float roundness = 0f;

        [SerializeField] private Vector2 size2d = Vector2.one;
        [SerializeField] private Vector3 size3d = Vector3.one;
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private int resolution = 1;

        [SerializeField] private List<MeshModifier> modifiers;

        public void AddModifier(MeshModifier modifier)
        {
            modifiers.Add(modifier);
        }
    }
}
