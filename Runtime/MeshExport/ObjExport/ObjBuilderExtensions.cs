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
            
            for (int i = 0; i < triangles.Count; i += 6)
            {
                builder.AppendFormat("{0} ", keyword);
                
                // quad composition from an unique indices of two triangles
                int v1 = triangles[i + 2];
                int v2 = triangles[i + 1];
                int v3 = triangles[i + 0];
                int v4 = triangles[(i + 3) + 0];
                
                if (v4 == v1 || v4 == v2 || v4 == v3)
                {
                    v4 = v3;
                    v1 = triangles[(i + 3) + 2];
                    v2 = triangles[(i + 3) + 1];
                    v3 = triangles[(i + 3) + 0];
                }
                
                AppendVertexData(builder, verticesDataMap, vertexIndex: v1, whiteSpaceAfter: true);
                AppendVertexData(builder, verticesDataMap, vertexIndex: v2, whiteSpaceAfter: true);
                AppendVertexData(builder, verticesDataMap, vertexIndex: v3, whiteSpaceAfter: true);
                AppendVertexData(builder, verticesDataMap, vertexIndex: v4, whiteSpaceAfter: false);
                
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
            int positionIndex = verticesDataMap[vertexIndex].positionIndex;
            int uvIndex = verticesDataMap[vertexIndex].uvIndex;
            int normalIndex = verticesDataMap[vertexIndex].normalIndex;
            
            builder.AppendFormat("{0}/{1}/{2}", positionIndex + 1, uvIndex + 1, normalIndex + 1);
            if (whiteSpaceAfter)
            {
                builder.Append(" ");
            }
        }
    }
}
