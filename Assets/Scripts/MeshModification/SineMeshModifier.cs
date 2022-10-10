using UnityEngine;

namespace Sxm.ProceduralMeshGenerator.Modification
{
    public sealed class SineMeshModifier : MeshModifier
    {
        [SerializeField, Min(0f)] private float amplitude = 0.5f;
        [SerializeField, Min(0f)] private float frequency = 1f;
        [SerializeField, Min(0f)] private float falloff = 0f;
        [SerializeField, Min(0f)] private float speed = 0f;
    
        private float ScaledTime => speed * Time.time;
    
        protected override void OnDrawGizmosSelected()
        {
            float t = ScaledTime;
            const float dt = DebugDistanceBetweenVertices;
            var points = new Vector3[DebugVerticesResolution];

            float x, y;
            for (int i = 0; i < points.Length; i++)
            {
                x = -0.5f + i * dt;
                y = MathUtils.FalloffSin2pi(amplitude, frequency, falloff, x, t);
            
                points[i] = new Vector3(x, y);
            }

            GizmosUtils.DrawCurve(points, transform, Color.red);
        }

        public override Vector3[] Modify(in Vector3[] vertices)
        {
            float t = ScaledTime;
            Matrix4x4 meshToAxis = meshTransform.localToWorldMatrix * Axis.worldToLocalMatrix;
            Matrix4x4 axisToMesh = meshToAxis.inverse;

            for (int i = 0; i < vertices.Length; i++)
            {
                var localAxisVertex = meshToAxis.MultiplyPoint3x4(vertices[i]);
                var offset = MathUtils.FalloffSin2pi(amplitude, frequency, falloff, localAxisVertex.x, t);
                localAxisVertex.y += offset;
            
                vertices[i] = axisToMesh.MultiplyPoint3x4(localAxisVertex);
            }
        
            return vertices;
        }
    }
}
