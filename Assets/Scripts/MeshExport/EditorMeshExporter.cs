using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Export
{
    // todo: move to editor assembly (?)
    public static class EditorMeshExporter
    {
        public enum MeshExportExtension
        {
            [InspectorName("Unity Asset (.asset)")]
            Asset,
            [InspectorName("Wavefront (.obj)")]
            Obj
        }

        private struct MeshExportExtensionData
        {
            public string label;
            public IMeshSaveHandler saveHandler;
        }
        
        private static readonly Dictionary<MeshExportExtension, MeshExportExtensionData> ExtensionLabels = new()
        {
            { 
                MeshExportExtension.Asset, 
                new MeshExportExtensionData
                {
                    label = "asset",
                    saveHandler = new AssetMeshSaveHandler()
                } 
            },
            {
                MeshExportExtension.Obj,
                new MeshExportExtensionData
                {
                    label = "obj",
                    saveHandler = new ObjMeshSaveHandler()
                }
            }
        };
        
        public static void Export(Mesh mesh, bool isForwardFacing, MeshExportExtension extension)
        {
            var extensionData = ExtensionLabels[extension];
            var path = EditorUtility.SaveFilePanel(
                "Save mesh",
                string.Empty,
                mesh.name + $".{extensionData.label}",
                extensionData.label
            );
            
            if (path.Length == 0)
            {
                Debug.LogWarning("Mesh wasn't saved. Empty save path.");
                return;
            }
            
            Debug.Log($"Save path: {path}");
            
            extensionData.saveHandler.Save(mesh, path);
            AssetDatabase.Refresh();
        }
    }
}
