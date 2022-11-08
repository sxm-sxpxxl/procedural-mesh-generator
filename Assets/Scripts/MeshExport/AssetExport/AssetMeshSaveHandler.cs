using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Export
{
    public sealed class AssetMeshSaveHandler : IMeshSaveHandler
    {
        public void Save(Mesh mesh, string absolutePath)
        {
            if (absolutePath.Contains(Application.dataPath) == false)
            {
                Debug.LogWarning("Mesh wasn't saved as unity asset. Save path must be inside the project.");
                return;
            }
            
            string relativePath = "Assets/" + absolutePath.Replace(Application.dataPath + "/", string.Empty);
            var existedMeshAsset = AssetDatabase.LoadAssetAtPath<Mesh>(relativePath);
            
            if (existedMeshAsset != null)
            {
                mesh.CopyTo(existedMeshAsset);
                AssetDatabase.SaveAssetIfDirty(existedMeshAsset);
            }
            else
            {
                AssetDatabase.CreateAsset(mesh, relativePath);
            }
        }

        public Task SaveAsync(Mesh mesh, string absolutePath)
        {
            return new Task(() => Save(mesh, absolutePath));
        }
    }
}
