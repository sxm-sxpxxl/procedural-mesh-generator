using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    public sealed class SineMeshModifier : BaseMeshModifier
    {
        [SerializeField, Min(0f)] private float amplitude = 0.5f;
        [SerializeField, Min(0f)] private float frequency = 1f;
        [SerializeField, Min(0f)] private float falloff = 0f;
        [SerializeField, Min(0f)] private float speed = 0f;
    
        private float ScaledTime => speed * Time.time;

        public override void DebugDraw()
        {
            float t = ScaledTime;
            const float dt = DebugDistanceBetweenVertices;
            var points = new Vector3[DebugVerticesResolution];

            float x, y;
            for (int i = 0; i < points.Length; i++)
            {
                x = -0.5f + i * dt;
                y = MathUtils.FalloffSin2PI(amplitude, frequency, falloff, x, t);
            
                points[i] = new Vector3(x, y);
            }

            GizmosUtils.DrawCurve(points, transform, Color.red);
        }

        protected override JobHandle ApplyOn(NativeArray<float3> vertices)
        {
            (float4x4 meshToAxis, float4x4 axisToMesh) = MathUtils.GetFromToTransform(MeshAxis, ModifierAxis);
            
            return new SineModifyJob
            {
                amplitude = amplitude,
                falloff = falloff,
                meshToAxis = meshToAxis,
                axisToMesh = axisToMesh,
                frequency = frequency,
                scaledTime = ScaledTime,
                vertices = vertices
            }.Schedule(vertices.Length, 0);
        }
    }
    
    [BurstCompile]
    internal struct SineModifyJob : IJobParallelFor
    {
        [ReadOnly] public float amplitude;
        [ReadOnly] public float frequency;
        [ReadOnly] public float falloff;
        [ReadOnly] public float scaledTime;
        [ReadOnly] public float4x4 meshToAxis;
        [ReadOnly] public float4x4 axisToMesh;

        public NativeArray<float3> vertices;
        
        public void Execute(int index)
        {
            float4 localAxisVertex = math.mul(meshToAxis, new float4(vertices[index], 1f));
            float offset = MathUtils.FalloffSin2PI(amplitude, frequency, falloff, localAxisVertex.x, scaledTime);
            localAxisVertex.y += offset;

            vertices[index] = math.mul(axisToMesh, localAxisVertex).xyz;
        }
    }
}
