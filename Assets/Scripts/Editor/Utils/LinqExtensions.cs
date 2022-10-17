using System;
using System.Collections.Generic;

namespace Sxm.ProceduralMeshGenerator
{
    public static class LinqExtensions
    {
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T obj in source)
            {
                action(obj);
            }
            
            return source;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int index = 0;
            foreach (T obj in source)
            {
                action(obj, index++);
            }
            
            return source;
        }
    }
}
