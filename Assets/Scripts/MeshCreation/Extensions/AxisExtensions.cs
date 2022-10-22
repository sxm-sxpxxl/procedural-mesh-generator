using UnityEngine;
using UnityEngine.Assertions;

public static class AxisExtensions
{
    public static Vector3 AsFor(this Vector2 vec, Plane plane) => plane switch
    {
        Plane.XY => new Vector3(vec.x, vec.y, 0f),
        Plane.YZ => new Vector3(0f, vec.x, vec.y),
        Plane.XZ => new Vector3(vec.x, 0f, vec.y),
        _ => Vector3.zero
    };

    public static Vector3 AsFor(this Vector3 vec, Plane plane, Plane planeWithExcludedCoordinate)
    {
        float excludedCoordinate = vec[(int) planeWithExcludedCoordinate];

        return plane switch
        {
            Plane.XY => new Vector3(vec.x, vec.y, excludedCoordinate),
            Plane.YZ => new Vector3(excludedCoordinate, vec.x, vec.y),
            Plane.XZ => new Vector3(vec.x, -excludedCoordinate, vec.y),
            _ => Vector3.zero
        };
    }
}
