using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public sealed class CubeMeshRequest : BaseMeshRequest
    {
        public readonly float roundness;

        public CubeMeshRequest(
            string name,
            int resolution,
            Vector3 size,
            Vector3 offset,
            float roundness,
            bool isScalingAndOffsetting = true,
            Func<MeshResponse, MeshResponse> postProcessCallback = null
        ) : base(
            name,
            resolution,
            size,
            offset,
            isScalingAndOffsetting,
            isBackfaceCulling: true,
            isForwardFacing: true,
            postProcessCallback: postProcessCallback
        )
        {
            this.roundness = roundness;
        }
    }
}
