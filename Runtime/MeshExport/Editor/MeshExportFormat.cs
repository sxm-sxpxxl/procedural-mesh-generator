using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Export.Editor
{
    public enum MeshExportFormat
    {
        [InspectorName("Unity Asset (.asset)")]
        Asset,
        [InspectorName("Wavefront (.obj)")]
        Obj
    }
}
