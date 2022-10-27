using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public sealed class PlaneMeshRequest : BaseMeshRequest
    {
        internal static readonly Plane VirtualAxis = Plane.XY;
        
        public readonly Plane axis;
        
        public PlaneMeshRequest(
            string name,
            int resolution,
            Vector2 size,
            Plane axis,
            Vector3 offset,
            bool isScalingAndOffsetting = true,
            bool isBackfaceCulling = true,
            bool isForwardFacing = true,
            Func<InterstitialMeshData, InterstitialMeshData> postProcessCallback = null
        ) : base(
            name,
            resolution,
            size.AsFor(VirtualAxis),
            offset,
            isScalingAndOffsetting,
            isBackfaceCulling,
            isForwardFacing,
            postProcessCallback
        )
        {
            this.axis = axis;
        }
    }
}
