using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public interface IMeshCreator
    {
        InterstitialMeshData CreateMeshData();
        InterstitialMeshData GetLastMeshData();
    }
}
