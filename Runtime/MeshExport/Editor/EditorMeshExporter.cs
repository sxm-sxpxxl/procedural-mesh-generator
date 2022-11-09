using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Sxm.ProceduralMeshGenerator.Export.Editor
{
    public static class EditorMeshExporter
    {
        private struct MeshExportFormatData
        {
            public string extension;
            public IMeshSaveHandler saveHandler;
        }
        
        private static readonly Dictionary<MeshExportFormat, MeshExportFormatData> AvailableFormats = new()
        {
            { 
                MeshExportFormat.Asset, 
                new MeshExportFormatData
                {
                    extension = "asset",
                    saveHandler = new AssetMeshSaveHandler()
                } 
            },
            {
                MeshExportFormat.Obj,
                new MeshExportFormatData
                {
                    extension = "obj",
                    saveHandler = new ObjMeshSaveHandler()
                }
            }
        };
        
        public static void Export(Mesh mesh, MeshExportFormat format)
        {
            bool isFormatAvailable = AvailableFormats.TryGetValue(format, out MeshExportFormatData formatData);
            Assert.IsTrue(isFormatAvailable);
            
            var absolutePath = EditorUtility.SaveFilePanel(
                "Save mesh",
                string.Empty,
                mesh.name + $".{formatData.extension}",
                formatData.extension
            );
            
            if (absolutePath.Length == 0)
            {
                return;
            }
            
            formatData.saveHandler.Save(mesh, absolutePath);
            AssetDatabase.Refresh();
            
            var savedAsset = AssetDatabaseUtils.LoadAssetAtAbsolutePath<Mesh>(absolutePath);
            if (savedAsset != null)
            {
                EditorGUIUtility.PingObject(savedAsset);
            }
        }
    }
}
