using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    // todo #1: fix incorrect creating of the plane when isForwardFacing changing
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
            Func<MeshResponse, MeshResponse> postProcessCallback = null
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
