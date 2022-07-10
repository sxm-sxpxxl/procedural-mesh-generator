using UnityEngine;

public static class AxisExtensions
{
    public static Vector3 AsFor(this Vector2 vec, Plane plane) => plane switch
    {
        Plane.XY => new Vector3(vec.x, vec.y, 0f),
        Plane.YZ => new Vector3(0f, vec.x, vec.y),
        Plane.XZ => new Vector3(vec.x, 0f, vec.y),
        _ => Vector3.zero
    };

    public static Vector2 ExtractFor(this Vector3 vec, Plane plane) => plane switch
    {
        Plane.XY => new Vector2(vec.x, vec.y),
        Plane.YZ => new Vector2(vec.y, vec.z),
        Plane.XZ => new Vector2(vec.x, vec.z),
        _ => Vector2.zero
    };
}
