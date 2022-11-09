using System.IO;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator
{
    public static class PathUtils
    {
        private const string AssetsRootPath = "Assets/";
        
        public static string AbsoluteToRelativePath(string absolutePath) =>
            AssetsRootPath + Path.GetRelativePath(Application.dataPath, absolutePath).Replace('\\', '/');
    }
}
