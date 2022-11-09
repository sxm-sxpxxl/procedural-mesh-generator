using UnityEngine;
using UnityEngine.Rendering;

namespace Sxm.ProceduralMeshGenerator.Export.Editor
{
    internal static class MeshExtensions
    {
        public static void CopyTo(this Mesh fromMesh, Mesh toMesh)
        {
            toMesh.Clear();
                
            toMesh.name = fromMesh.name;
            toMesh.bounds = fromMesh.bounds;
            toMesh.vertices = fromMesh.vertices;
            toMesh.normals = fromMesh.normals;
            toMesh.uv = fromMesh.uv;
            toMesh.triangles = fromMesh.triangles;
            toMesh.indexFormat = IndexFormat.UInt16;
        }
    }
}
