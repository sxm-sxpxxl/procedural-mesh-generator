using System;
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

        public VerticesData(int verticesCount, VertexGroup[] vertexGroups, int[] excludedVertexGroupsMap) : this()
        {
            this.vertexGroups = vertexGroups;
            this.excludedVertexGroupsMap = excludedVertexGroupsMap;
            vertices = GetParametersByVertexGroups(verticesCount, group => group.position);
            vertexGroupIndices = GetParametersByVertexGroups(verticesCount, group => group.selfIndex);
        }
        
        public VertexGroup GetGroupByVertexIndex(int index) => vertexGroups[vertexGroupIndices[index]];

        private T[] GetParametersByVertexGroups<T>(int verticesCount, Func<VertexGroup, T> getParameter) where T : struct
        {
            var parameters = new T[verticesCount];
            
            for (int i = 0, pIndex = 0; i < vertexGroups.Length; i++)
            {
                VertexGroup group = vertexGroups[i];
                
                T parameter = getParameter.Invoke(group);
                int indicesCount = group.hasSingleVertex ? 1 : group.indices.Length;
                
                for (int j = 0; j < indicesCount; j++)
                {
                    parameters[pIndex++] = parameter;
                }
            }

            return parameters;
        }
    }
}
