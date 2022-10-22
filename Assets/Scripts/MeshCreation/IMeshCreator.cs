using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public interface IMeshCreator
    {
        Mesh CreateMesh();
        MeshResponse GetMeshResponse();
    }
}
