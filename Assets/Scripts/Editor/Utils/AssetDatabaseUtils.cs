using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Sxm.ProceduralMeshGenerator
{
    public static class AssetDatabaseUtils
    {
        public static string GetAssetPathFor(string scriptName, string searchFolderPath = default)
        {
            var searchInFolders = string.IsNullOrEmpty(searchFolderPath) == false ? new string[] { searchFolderPath } : null;
            var guids = AssetDatabase.FindAssets($"t:Script {scriptName}", searchInFolders);

            if (guids.Length != 1)
            {
                Debug.LogError($"Asset '{scriptName}.cs' was not found or more than one was found!");
                return string.Empty;
            }
            
            return AssetDatabase.GUIDToAssetPath(guids[0]);
        }

        public static StyleSheet GetRootStylesFor(string scriptName)
        {
            var relativePath = GetAssetPathFor(scriptName, "Assets/Scripts");
            var ussPath = Path.ChangeExtension(relativePath, "uss");
            
            return AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
        }
    }
}
