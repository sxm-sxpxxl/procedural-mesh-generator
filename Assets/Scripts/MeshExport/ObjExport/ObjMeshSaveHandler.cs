using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Export
{
    public sealed class ObjMeshSaveHandler : IMeshSaveHandler
    {
        internal struct VertexData
        {
            public int positionIndex;
            public int normalIndex;
            public int uvIndex;
        }
        
        public void Save(Mesh mesh, string absolutePath)
        {
            string objContent = BuildContent(mesh);
            File.WriteAllText(absolutePath, objContent);
        }
        
        public Task SaveAsync(Mesh mesh, string absolutePath)
        {
            string objContent = BuildContent(mesh);
            return File.WriteAllTextAsync(absolutePath, objContent);
        }
        
        private string BuildContent(Mesh mesh)
        {
            var uniqueVerticesData = new VertexData[mesh.vertices.Length];
            
            var uniquePositions = MakeUnique(
                mesh.vertices,
                (vertexIndex, positionIndex) => uniqueVerticesData[vertexIndex].positionIndex = positionIndex
            );
            var uniqueNormals = MakeUnique(
                mesh.normals,
                (vertexIndex, normalIndex) => uniqueVerticesData[vertexIndex].normalIndex = normalIndex
            );
            var uniqueUV = MakeUnique(
                mesh.uv, 
                (vertexIndex, uvIndex) => uniqueVerticesData[vertexIndex].uvIndex = uvIndex
            );
            
            TransformToDefaultAxis(uniquePositions);
            TransformToDefaultAxis(uniqueNormals);
            
            var content = new StringBuilder();
            
            content.AppendLine($"o {mesh.name.ToLower().Replace(' ', '-')}");
            content.AppendVector3Array("v", uniquePositions);
            content.AppendVector2Array("vt", uniqueUV);
            content.AppendVector3Array("vn", uniqueNormals);
            content.AppendFaces("f", mesh.triangles, uniqueVerticesData);
            
            return content.ToString();
        }
        
        private static void TransformToDefaultAxis(Vector3[] vertices)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].y *= -1f;
                vertices[i] = Quaternion.Euler(0f, 0f, 180f) * vertices[i];
            }
        }
        
        private static T[] MakeUnique<T>(
            IReadOnlyList<T> elements,
            Action<int, int> vertexDataSetCallback
        ) where T : struct
        {
            var uniqueElements = new List<Tuple<int, T>>(capacity: elements.Count);
            
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                var uniqueFound = uniqueElements.Find(x => Equals(x.Item2, element));
                int uniqueVertexDataElementIndex;
                
                if (uniqueFound == null)
                {
                    uniqueVertexDataElementIndex = uniqueElements.Count;
                    uniqueElements.Add(new Tuple<int, T>(uniqueVertexDataElementIndex, elements[i]));
                }
                else
                {
                    uniqueVertexDataElementIndex = uniqueFound.Item1;
                }
                
                vertexDataSetCallback.Invoke(i, uniqueVertexDataElementIndex);
            }
        
            return uniqueElements.Select(x => x.Item2).ToArray();
        }
    }
}
