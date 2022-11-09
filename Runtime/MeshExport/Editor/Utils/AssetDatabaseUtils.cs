using UnityEditor;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Export.Editor
{
    internal static class AssetDatabaseUtils
    {
        private const string AssetsRootPath = "Assets/";
        
        public static T LoadAssetAtAbsolutePath<T>(string absolutePath) where T : Object
        {
            string relativePath = AbsoluteToRelativePath(absolutePath);
            return AssetDatabase.LoadAssetAtPath<T>(relativePath);
        }
        
        public static string AbsoluteToRelativePath(string absolutePath)
        {
            return AssetsRootPath + absolutePath.Replace(Application.dataPath + "/", string.Empty);
        }
    }
}
