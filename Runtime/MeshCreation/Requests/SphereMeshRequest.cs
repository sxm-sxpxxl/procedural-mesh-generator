using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public class SphereMeshRequest : BaseMeshRequest
    {
        public SphereMeshRequest(
            string name,
            int resolution,
            Vector3 size,
            Vector3 offset,
            Func<InterstitialMeshData, InterstitialMeshData> postProcessCallback = null
        ) : base(
            name,
            resolution,
            size,
            offset,
            isScalingAndOffsetting: true,
            isBackfaceCulling: true,
            isForwardFacing: true,
            postProcessCallback: postProcessCallback
        ) { }
    }
}
