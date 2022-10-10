using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public sealed class MeshRequest
    {
        public readonly string name;
        public readonly int resolution;
        public readonly Vector3 size;
        public readonly Vector3 offset;
        
        public readonly bool isScalingAndOffsetting;
        public readonly bool isForwardFacing;
        public readonly bool isBackfaceCulling;
        
        public readonly Func<MeshResponse, MeshResponse> postProcessCallback;
        public readonly object customData;
        
        public MeshRequest(
            string name,
            int resolution,
            Vector3 size,
            Vector3 offset = default,
            bool isScalingAndOffsetting = true,
            bool isForwardFacing = true,
            bool isBackfaceCulling = true,
            Func<MeshResponse, MeshResponse> postProcessCallback = null,
            object customData = default
        )
        {
            this.name = name;
            this.resolution = resolution;
            this.size = size;
            this.offset = offset;
            this.isScalingAndOffsetting = isScalingAndOffsetting;
            this.isForwardFacing = isForwardFacing;
            this.isBackfaceCulling = isBackfaceCulling;
            this.postProcessCallback = postProcessCallback;
            this.customData = customData;
        }
    }
}
