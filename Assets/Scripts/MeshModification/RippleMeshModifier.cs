using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    public sealed class RippleMeshModifier : BaseMeshModifier
    {
        [SerializeField, Min(0f)] private float amplitude = 0.5f;
        [SerializeField, Min(0f)] private float frequency = 1f;
        [SerializeField, Min(0f)] private float falloff = 0f;
        [Space]
        [SerializeField, Min(0f)] private float innerRadius = 0f;
        [SerializeField, Min(0f)] private float outerRadius = 1f;
        [SerializeField, Min(0f)] private float speed = 0f;

        private float ScaledTime => speed * Time.time;
        
        private void OnValidate()
        {
            if (innerRadius > outerRadius)
            {
                innerRadius = outerRadius;
            }
        }
        
        protected override void OnDrawGizmosSelected()
        {
            float t = ScaledTime;
            float dt = (outerRadius - innerRadius) / DebugVerticesResolution;

            var verticesCount = DebugVerticesResolution + 1;
            var vertices = new Vector3[verticesCount];
            var symmetricalVertices = new Vector3[verticesCount];

            float x, y;
            for (int i = 0; i < verticesCount; i++)
            {
                x = innerRadius + i * dt;
                y = MathUtils.FalloffSin2PI(amplitude, frequency, falloff, x, t);
                
                vertices[i] = new Vector3(x, y);
                symmetricalVertices[i] = new Vector3(-x, y);
            }

            GizmosUtils.DrawCurve(vertices, transform, Color.red);
            GizmosUtils.DrawCurve(symmetricalVertices, transform, Color.red);

            float innerHeight = MathUtils.FalloffSin2PI(amplitude, frequency, falloff, innerRadius, t);
            float outerHeight = vertices[^1].y;
            
            GizmosUtils.DrawCircle(DebugVerticesResolution, innerRadius, innerHeight, transform, Color.red);
            GizmosUtils.DrawCircle(DebugVerticesResolution, outerRadius, outerHeight, transform, Color.red);
        }

        public override void Modify(in Vector3[] vertices)
        {
            (float4x4 meshToAxis, float4x4 axisToMesh) = MathUtils.GetFromToTransform(meshTransform, Axis);
            var nativeVertices = NativeUtils.GetNativeArrayFrom(vertices, Allocator.TempJob);
            
            new RippleModifyJob
            {
                amplitude = amplitude,
                frequency = frequency,
                falloff = falloff,
                scaledTime = ScaledTime,
                innerRadius = innerRadius,
                outerRadius = outerRadius,
                meshToAxis = meshToAxis,
                axisToMesh = axisToMesh,
                vertices = nativeVertices
            }.Schedule(nativeVertices.Length, 0).Complete();

            NativeUtils.SetNativeArrayTo(nativeVertices, vertices);
            nativeVertices.Dispose();
        }
    }
    
    [BurstCompile(CompileSynchronously = true)]
    internal struct RippleModifyJob : IJobParallelFor
    {
        [ReadOnly] public float amplitude;
        [ReadOnly] public float frequency;
        [ReadOnly] public float falloff;
        [ReadOnly] public float scaledTime;
        [ReadOnly] public float innerRadius;
        [ReadOnly] public float outerRadius;
        [ReadOnly] public float4x4 meshToAxis;
        [ReadOnly] public float4x4 axisToMesh;

        public NativeArray<float3> vertices;
        
        public void Execute(int index)
        {
            float4 localAxisVertex = math.mul(meshToAxis, new float4(vertices[index], 1f));

            float distanceToCenter = math.length(localAxisVertex.xz);
            float clampedDistance = math.clamp(distanceToCenter, innerRadius, outerRadius);

            float offset = MathUtils.FalloffSin2PI(amplitude, frequency, falloff, clampedDistance, scaledTime);
            localAxisVertex.y += offset;

            vertices[index] = math.mul(axisToMesh, localAxisVertex).xyz;
        }
    }
}
