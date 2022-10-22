using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public abstract class BaseMeshRequest
    {
        public readonly string name;
        public readonly int resolution;
        public readonly Vector3 size;
        public readonly Vector3 offset;
        
        public readonly bool isScalingAndOffsetting;
        public readonly bool isForwardFacing;
        public readonly bool isBackfaceCulling;
        
        public readonly Func<MeshResponse, MeshResponse> postProcessCallback;
        
        public BaseMeshRequest(
            string name,
            int resolution,
            Vector3 size,
            Vector3 offset,
            bool isScalingAndOffsetting,
            bool isBackfaceCulling,
            bool isForwardFacing,
            Func<MeshResponse, MeshResponse> postProcessCallback = null
        )
        {
            this.name = name;
            this.resolution = resolution;
            this.size = size;
            this.offset = offset;
            this.isScalingAndOffsetting = isScalingAndOffsetting;
            this.isBackfaceCulling = isBackfaceCulling;
            this.isForwardFacing = isForwardFacing;
            this.postProcessCallback = postProcessCallback;
        }
    }
}
