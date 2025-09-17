using System;

public static class FloatEx
{
    public static float Precision = 0.0001f;

    public static bool ApproximatelyEquals(this float a, float b)
    {
        return Math.Abs(a - b) <= Precision;
    }
}