using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine.UIElements;

namespace Sxm.ProceduralMeshGenerator.Editor
{
    public static class AssetDatabaseUtils
    {
        public static StyleSheet GetRelativeStyle(string stylesRelativePath = "", [CallerFilePath] string absoluteScriptFilePath = "")
        {
            var relativeScriptFilePath = PathUtils.AbsoluteToRelativePath(absoluteScriptFilePath);

            if (string.IsNullOrEmpty(stylesRelativePath) == false)
            {
                string scriptFileName = Path.GetFileName(relativeScriptFilePath);
                string parentRelativePath = PathUtils.AbsoluteToRelativePath(Directory.GetParent(relativeScriptFilePath)?.FullName);
                
                relativeScriptFilePath = $"{parentRelativePath}/{stylesRelativePath}/{scriptFileName}";
            }
            
            var ussPath = Path.ChangeExtension(relativeScriptFilePath, "uss");
            var ussAsset = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
            
            if (ussAsset == null)
            {
                throw new ArgumentException($"Style asset on path '{ussPath}' wasn't found.");
            }
            
            return ussAsset;
        }
    }
}
