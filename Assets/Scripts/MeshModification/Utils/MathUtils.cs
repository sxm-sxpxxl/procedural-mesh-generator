using UnityEngine;

public static class MathUtils
{
    private const float DoublePI = 2f * Mathf.PI;
    
    public static float FalloffSin2pi(float amplitude, float frequency, float falloff, float argument, float time = 0f)
    {
        float result = amplitude * Mathf.Sin(DoublePI * frequency * (argument + time));
        return result * Mathf.Exp(-falloff * Mathf.Abs(argument));
    }
}
