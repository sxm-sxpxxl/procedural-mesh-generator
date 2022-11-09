using System.Linq;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    internal readonly struct VertexGroup
    {
        public readonly int selfIndex;
        public readonly bool hasSingleVertex;
        public readonly int singleIndex;
        public readonly int[] indices;
        public readonly Vector3 position;
            
        public VertexGroup(int selfIndex, Vector3 position, int startIndex, int length)
        {
            this.selfIndex = selfIndex;
            this.position = position;
            hasSingleVertex = length == 1;
            singleIndex = hasSingleVertex ? startIndex : -1;
            indices = hasSingleVertex ? null : Enumerable.Range(startIndex, length).ToArray();
        }

        public int this[int index] => hasSingleVertex ? singleIndex : indices[Mathf.Min(index, indices.Length - 1)];
    }
}
