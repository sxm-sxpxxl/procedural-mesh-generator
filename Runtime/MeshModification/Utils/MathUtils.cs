using UnityEngine;
using Unity.Mathematics;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    internal static class MathUtils
    {
        private const float DoublePI = 2f * math.PI;
    
        public static float FalloffSin2PI(float amplitude, float frequency, float falloff, float argument, float time = 0f)
        {
            float result = amplitude * math.sin(DoublePI * frequency * (argument + time));
            return result * math.exp(-falloff * math.abs(argument));
        }
        
        public static (float4x4, float4x4) GetFromToTransform(Transform from, Transform to)
        {
            Matrix4x4 fromTo = from.localToWorldMatrix * to.worldToLocalMatrix;
            Matrix4x4 toFrom = fromTo.inverse;

            return ((float4x4) fromTo, (float4x4) toFrom);
        }
    }
}
