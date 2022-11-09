using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Export
{
    public enum MeshExportFormat
    {
        [InspectorName("Unity Asset (.asset)")]
        Asset,
        [InspectorName("Wavefront (.obj)")]
        Obj
    }
}
