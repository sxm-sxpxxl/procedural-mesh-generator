using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    public static class MeshUtils
    {
        public static JobHandle RecalculateBounds(NativeArray<bounds> bounds, NativeArray<float3> vertices, JobHandle dependency = default)
        {
            return new RecalculateBoundsJob
            {
                bounds = bounds,
                vertices = vertices
            }.Schedule(dependency);
        }
    }
}
