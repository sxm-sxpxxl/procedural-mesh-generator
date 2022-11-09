using System.IO;
using System.Text;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator
{
    public static class PathUtils
    {
        public static string AbsoluteToRelativePath(string absolutePath) =>
            absolutePath.Contains("PackageCache")
                ? PackageAssetAbsoluteToRelativePath(absolutePath) 
                : ProjectAssetAbsoluteToRelativePath(absolutePath);
        
        private static string PackageAssetAbsoluteToRelativePath(string absolutePath)
        {
            string[] directories = absolutePath.Replace('\\', '/').Split('/');
            int projectRootDirectoryIndex = 0;
            string packageName = string.Empty;
            
            for (int i = 0; i < directories.Length; i++)
            {
                if (directories[i] == "Library")
                {
                    //       0     /     1      /  2   /  3
                    // ../Library/PackageCache/com...@/<Root>/..
                    
                    string packageDirectory = directories[i + 2];
                    int lastIndex = packageDirectory.LastIndexOf('@');
                    packageName = packageDirectory.Substring(0, lastIndex);
                    
                    projectRootDirectoryIndex = i + 3;
                    break;
                }
            }
            
            var relativeTo = new StringBuilder();
            for (int i = projectRootDirectoryIndex; i < directories.Length; i++)
            {
                relativeTo.Append(directories[i]);
                
                if (i != directories.Length - 1)
                {
                    relativeTo.Append('/');
                }
            }
            
            return $"Packages/{packageName}/{relativeTo}";
        }
        
        private static string ProjectAssetAbsoluteToRelativePath(string absolutePath) =>
            "Assets/" + Path.GetRelativePath(Application.dataPath, absolutePath).Replace('\\', '/');
    }
}
