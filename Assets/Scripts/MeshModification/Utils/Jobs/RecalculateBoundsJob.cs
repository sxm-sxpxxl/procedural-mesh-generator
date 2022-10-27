using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    [Serializable]
    public struct bounds
    {
        public float3 center;
        public float3 size;
        
        public bounds(float3 min, float3 max)
        {
            size = max - min;
            center = min + 0.5f * size;
        }

        public static implicit operator Bounds(bounds b) => new Bounds(b.center, b.size);
        
        public static implicit operator bounds(Bounds b) => new bounds(b.min, b.max);
    }
    
    [BurstCompile]
    public struct RecalculateBoundsJob : IJob
    {
        [ReadOnly] public NativeArray<float3> vertices;
        [WriteOnly] public NativeArray<bounds> bounds;
        
        public void Execute()
        {
            float3 min, max;
            min = max = vertices[0];

            for (int i = 1; i < vertices.Length; i++)
            {
                var v = vertices[i];
                
                for (int j = 0; j < 3; j++)
                {
                    if (v[j] < min[j])
                    {
                        min[j] = v[j];
                    }
                
                    if (v[j] > max[j])
                    {
                        max[j] = v[j];
                    }
                }
            }
            
            bounds[0] = new bounds(min, max);
        }
    }
}
