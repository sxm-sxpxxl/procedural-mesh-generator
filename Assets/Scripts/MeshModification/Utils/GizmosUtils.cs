using UnityEngine;

public static class GizmosUtils
{
    public static void DrawCurve(in Vector3[] points, Transform relativeTransform, Color color, bool isClosed = false)
    {
        Color tempColor = Gizmos.color;
        Gizmos.color = color;

        Vector3 from, to;
        for (int i = 1; i < points.Length; i++)
        {
            from = relativeTransform.TransformPoint(points[i - 1]);
            to = relativeTransform.TransformPoint(points[i]);
            
            Gizmos.DrawLine(from, to);
        }
        
        if (isClosed)
        {
            from = relativeTransform.TransformPoint(points[^1]);
            to = relativeTransform.TransformPoint(points[0]);
            
            Gizmos.DrawLine(from, to);
        }
        
        Gizmos.color = tempColor;
    }

    public static void DrawCircle(int resolution, float radius, float height, Transform relativeTransform, Color color)
    {
        float dt = 1f / resolution;
        var points = new Vector3[resolution];
        
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = new Vector3(
                x: radius * Mathf.Cos(2f * Mathf.PI * i * dt),
                y: height,
                z: radius * Mathf.Sin(2f * Mathf.PI * i * dt)
            );
        }
        
        DrawCurve(points, relativeTransform, color, true);
    }
}
