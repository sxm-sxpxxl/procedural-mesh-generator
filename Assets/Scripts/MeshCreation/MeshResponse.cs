using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public sealed class MeshResponse
    {
        public readonly string meshName;
        public readonly Vector3[] vertices;
        public readonly Vector3[] normals;
        public readonly Vector2[] uv;
        
        internal readonly bool withBackfaceCulling;
        internal readonly int[] excludedVertexGroupsMap;
        internal readonly VertexGroup[] vertexGroups;

        private readonly int[] _vertexGroupIndices;
        private int[] _triangles;

        internal int BackfaceAdjustedVerticesLength => withBackfaceCulling ? vertices.Length : vertices.Length / 2;
        
        public Mesh MeshInstance => new Mesh
        {
            name = meshName,
            vertices = vertices,
            normals = normals,
            uv = uv,
            triangles = _triangles
        };
        
        internal MeshResponse(
            string meshName,
            int verticesCount,
            VertexGroup[] vertexGroups,
            int[] excludedVertexGroupsMap,
            bool withBackfaceCulling
        )
        {
            this.meshName = meshName;
            this.vertexGroups = vertexGroups;
            this.excludedVertexGroupsMap = excludedVertexGroupsMap;
            this.withBackfaceCulling = withBackfaceCulling;

            vertices = GetParametersByVertexGroups(verticesCount, group => group.position);
            normals = new Vector3[verticesCount];
            uv = new Vector2[verticesCount];

            _vertexGroupIndices = GetParametersByVertexGroups(verticesCount, group => group.selfIndex);
        }

        internal void SetTriangles(int[] triangles) => _triangles = triangles;
        
        internal VertexGroup GetGroupByVertexIndex(int index) => vertexGroups[_vertexGroupIndices[index]];

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
