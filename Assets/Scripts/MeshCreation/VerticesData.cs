using System.Linq;
using UnityEngine;

namespace MeshCreation
{
    public readonly struct VerticesData
    {
        public readonly Vector3[] vertices;
        public readonly int[] vertexGroupIndices;
        public readonly VertexGroup[] vertexGroups;
        public readonly int[] excludedVertexGroupsMap;

        public VerticesData(VertexGroup[] vertexGroups, int[] excludedVertexGroupsMap)
        {
            this.vertexGroups = vertexGroups;
            this.excludedVertexGroupsMap = excludedVertexGroupsMap;
            vertices = vertexGroups.SelectMany(v => v.indices, (v, _) => v.position).ToArray();
            vertexGroupIndices = vertexGroups.SelectMany(v => v.indices, (v, _) => v.selfIndex).ToArray();
        }

        public VertexGroup GetGroupByVertexIndex(int index) => vertexGroups[vertexGroupIndices[index]];
    }
}
