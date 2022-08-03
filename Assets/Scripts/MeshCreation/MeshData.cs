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

        public MeshData(
            int resolution,
            Vector3 size,
            Vector3 offset = default,
            bool isForwardFacing = true,
            bool isBackfaceCulling = true
        )
        {
            this.resolution = resolution;
            this.size = size;
            this.offset = offset;
            this.isForwardFacing = isForwardFacing;
            this.isBackfaceCulling = isBackfaceCulling;
        }
    }
}
