﻿using System;
using UnityEngine;

namespace MeshCreation
{
    public readonly struct MeshData
    {
        public readonly int resolution;
        public readonly Vector3 size;
        public readonly Vector3 offset;
        public readonly bool isForwardFacing;
        public readonly bool isBackfaceCulling;
        public readonly Func<Mesh, Mesh> postProcessCallback;
        
        // todo: refactoring (custom data)
        public readonly float roundness;

        public MeshData(
            int resolution,
            Vector3 size,
            Vector3 offset = default,
            bool isForwardFacing = true,
            bool isBackfaceCulling = true,
            Func<Mesh, Mesh> postProcessCallback = null,
            float roundness = 0f
        )
        {
            this.resolution = resolution;
            this.size = size;
            this.offset = offset;
            this.isForwardFacing = isForwardFacing;
            this.isBackfaceCulling = isBackfaceCulling;
            this.postProcessCallback = postProcessCallback;
            this.roundness = roundness;
        }
    }
}
