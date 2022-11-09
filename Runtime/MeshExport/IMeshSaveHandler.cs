using System.Threading.Tasks;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Export
{
    public interface IMeshSaveHandler
    {
        void Save(Mesh mesh, string absolutePath);
        Task SaveAsync(Mesh mesh, string absolutePath);
    }
}
