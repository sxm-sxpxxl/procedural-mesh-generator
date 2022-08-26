using System;
using UnityEngine;

namespace MeshCreation
{
    public class MeshRequest
    {
        public readonly int resolution;
        public readonly Vector3 size;
        public readonly Vector3 offset;
        public readonly bool isForwardFacing;
        public readonly bool isBackfaceCulling;
        public readonly Func<MeshResponse, MeshResponse> postProcessCallback;
        public readonly object customData;
        
        public MeshRequest(
            int resolution,
            Vector3 size,
            Vector3 offset = default,
            bool isForwardFacing = true,
            bool isBackfaceCulling = true,
            Func<MeshResponse, MeshResponse> postProcessCallback = null,
            object customData = default
        )
        {
            this.resolution = resolution;
            this.size = size;
            this.offset = offset;
            this.isForwardFacing = isForwardFacing;
            this.isBackfaceCulling = isBackfaceCulling;
            this.postProcessCallback = postProcessCallback;
            this.customData = customData;
        }
    }
}
