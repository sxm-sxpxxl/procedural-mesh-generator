using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public interface IMeshCreator
    {
        MeshResponse CreateMeshResponse();
        MeshResponse GetLastMeshResponse();
    }
}
