using UnityEngine;

namespace MeshCreation
{
    public readonly struct VertexGroup
    {
        public readonly int selfIndex;
        public readonly int[] indices;
        public readonly Vector3 position;
            
        public VertexGroup(int selfIndex, Vector3 position, int[] indices)
        {
            this.selfIndex = selfIndex;
            this.position = position;
            this.indices = indices;
        }

        public int this[int index] => indices[Mathf.Min(index, indices.Length - 1)];
    }
}
