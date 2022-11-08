using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Sxm.ProceduralMeshGenerator.Export
{
    internal static class ObjBuilderExtensions
    {
        public static void AppendFaces(
            this StringBuilder builder,
            string keyword,
            IReadOnlyList<int> triangles,
            IReadOnlyList<ObjMeshSaveHandler.VertexData> verticesDataMap
        )
        {
            Assert.IsTrue(triangles.Count % 6 == 0);
            
            var uniqueIndices = new List<int>(capacity: 4);
            for (int i = 0; i < triangles.Count; i += 6)
            {
                builder.AppendFormat("{0} ", keyword);
                
                for (int j = 0; j < 6; j++)
                {
                    int vertexIndex = triangles[i + j];
                    
                    if (uniqueIndices.Contains(vertexIndex) == false)
                    {
                        uniqueIndices.Add(vertexIndex);
                    }
                }
                
                int i0;
                int i1;
                int i2;
                int i3;
                
                // todo: add vertex traversal order definition 
                bool isForward = true;
                if (isForward == false)
                {
                    i0 = 0;
                    i1 = 2;
                    i2 = 3;
                    i3 = 1;
                }
                else
                {
                    i0 = 0;
                    i1 = 1;
                    i2 = 3;
                    i3 = 2;
                }
                
                AppendVertexData(builder, verticesDataMap, uniqueIndices[i0], true);
                AppendVertexData(builder, verticesDataMap, uniqueIndices[i1], true);
                AppendVertexData(builder, verticesDataMap, uniqueIndices[i2], true);
                AppendVertexData(builder, verticesDataMap, uniqueIndices[i3], false);
                
                uniqueIndices.Clear();
                builder.AppendLine();
            }
        }
        
        public static void AppendVector3Array(this StringBuilder builder, string keyword, IReadOnlyList<Vector3> array)
        {
            var culture = new CultureInfo("en-US");
            const string format = "{0} {1:0.000000} {2:0.000000} {3:0.000000}";
            
            for (int i = 0; i < array.Count; i++)
            {
                var element = array[i];
                builder.AppendFormat(culture, format, keyword, element.x, element.y, element.z);
                builder.AppendLine();
            }
        }
        
        public static void AppendVector2Array(this StringBuilder builder, string keyword, IReadOnlyList<Vector2> array)
        {
            var culture = new CultureInfo("en-US");
            const string format = "{0} {1:0.000000} {2:0.000000}";
            
            for (int i = 0; i < array.Count; i++)
            {
                var element = array[i];
                builder.AppendFormat(culture, format, keyword, element.x, element.y);
                builder.AppendLine();
            }
        }
        
        private static void AppendVertexData(
            this StringBuilder builder,
            IReadOnlyList<ObjMeshSaveHandler.VertexData> verticesDataMap,
            int vertexIndex,
            bool whiteSpaceAfter
        )
        {
            int uvIndex = verticesDataMap[vertexIndex].uvIndex;
            int normalIndex = verticesDataMap[vertexIndex].normalIndex;
            
            // builder.AppendFormat("{0}/{1}/{2}", vertexIndex + 1, uvIndex + 1, normalIndex + 1);
            builder.AppendFormat("{0}", vertexIndex + 1, uvIndex + 1, normalIndex + 1);
            if (whiteSpaceAfter)
            {
                builder.Append(" ");
            }
        }
    }
}
