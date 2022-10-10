using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    public sealed class RippleMeshModifier : MeshModifier
    {
        [SerializeField, Min(0f)] private float amplitude = 0.5f;
        [SerializeField, Min(0f)] private float frequency = 1f;
        [SerializeField, Min(0f)] private float falloff = 0f;
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
                y = MathUtils.FalloffSin2pi(amplitude, frequency, falloff, x, t);
                
                vertices[i] = new Vector3(x, y);
                symmetricalVertices[i] = new Vector3(-x, y);
            }

            GizmosUtils.DrawCurve(vertices, transform, Color.red);
            GizmosUtils.DrawCurve(symmetricalVertices, transform, Color.red);

            float innerHeight = MathUtils.FalloffSin2pi(amplitude, frequency, falloff, innerRadius, t);
            float outerHeight = vertices[^1].y;
            
            GizmosUtils.DrawCircle(DebugVerticesResolution, innerRadius, innerHeight, transform, Color.red);
            GizmosUtils.DrawCircle(DebugVerticesResolution, outerRadius, outerHeight, transform, Color.red);
        }

        public override Vector3[] Modify(in Vector3[] vertices)
        {
            float t = ScaledTime;
            Matrix4x4 meshToAxis = meshTransform.localToWorldMatrix * Axis.worldToLocalMatrix;
            Matrix4x4 axisToMesh = meshToAxis.inverse;

            for (int i = 0; i < vertices.Length; i++)
            {
                var localAxisVertex = meshToAxis.MultiplyPoint3x4(vertices[i]);
                
                float distanceToCenter = new Vector3(localAxisVertex.x, 0f, localAxisVertex.z).magnitude;
                var clampedDistance = Mathf.Clamp(distanceToCenter, innerRadius, outerRadius);

                var offset = MathUtils.FalloffSin2pi(amplitude, frequency, falloff, clampedDistance, t);
                localAxisVertex.y += offset;
                
                vertices[i] = axisToMesh.MultiplyPoint3x4(localAxisVertex);
            }
            
            return vertices;
        }
    }
}
