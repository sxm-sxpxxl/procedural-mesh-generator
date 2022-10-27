using System;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Creation
{
    public sealed class InterstitialMeshData
    {
        internal readonly bool withBackfaceCulling;
        internal readonly int[] excludedVertexGroupsMap;
        internal readonly VertexGroup[] vertexGroups;

        private readonly int[] _vertexGroupIndices;
        private int[] _triangles;
        
        public string MeshName { get; set; }
        public Bounds Bounds { get; set; }
        public Vector3[] Vertices { get; internal set; }
        public Vector3[] Normals { get; internal set; }
        public Vector2[] UV { get; internal set; }
        
        internal int BackfaceAdjustedVerticesLength => withBackfaceCulling ? Vertices.Length : Vertices.Length / 2;
        
        public Mesh MeshInstance => new Mesh
        {
            name = MeshName,
            bounds = Bounds,
            vertices = Vertices,
            normals = Normals,
            uv = UV,
            triangles = _triangles
        };
        
        internal InterstitialMeshData(
            string meshName,
            int verticesCount,
            VertexGroup[] vertexGroups,
            int[] excludedVertexGroupsMap,
            bool withBackfaceCulling
        )
        {
            this.vertexGroups = vertexGroups;
            this.excludedVertexGroupsMap = excludedVertexGroupsMap;
            this.withBackfaceCulling = withBackfaceCulling;

            MeshName = meshName;
            Vertices = GetParametersByVertexGroups(verticesCount, group => group.position);
            Normals = new Vector3[verticesCount];
            UV = new Vector2[verticesCount];
            Bounds = new Bounds(Vector3.zero, Vector3.one);
            
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
