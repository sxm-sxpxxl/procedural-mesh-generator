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
        public struct VertexData
        {
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
            var uniqueVertices = mesh.vertices;
            var verticesData = new VertexData[uniqueVertices.Length];

            for (int i = 0; i < verticesData.Length; i++)
            {
                // todo: refactoring
                uniqueVertices[i].y *= -1f;
            }
            
            var uniqueUV = MakeUnique(
                mesh.uv, 
                (vertexIndex, uvIndex) => verticesData[vertexIndex].uvIndex = uvIndex
            );
            var uniqueNormals = MakeUnique(
                mesh.normals,
                (vertexIndex, normalIndex) =>
                {
                    verticesData[vertexIndex].normalIndex = normalIndex;
                }
            );
            
            var content = new StringBuilder();
            
            content.AppendLine($"o {mesh.name.ToLower().Replace(' ', '-')}");
            content.AppendVector3Array("v", uniqueVertices);
            content.AppendVector2Array("vt", uniqueUV);
            content.AppendVector3Array("vn", uniqueNormals);
            content.AppendFaces("f", mesh.triangles, verticesData);

            return content.ToString();
        }
        
        // todo: fix setting index outside normal array bounds
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
                int uniqueVertexElementIndex;
                
                if (uniqueFound == null)
                {
                    uniqueElements.Add(new Tuple<int, T>(i, elements[i]));
                    uniqueVertexElementIndex = i;
                }
                else
                {
                    uniqueVertexElementIndex = uniqueFound.Item1;
                }
                
                vertexDataSetCallback.Invoke(i, uniqueVertexElementIndex);
            }
        
            return uniqueElements.Select(x => x.Item2).ToArray();
        }
    }
}
