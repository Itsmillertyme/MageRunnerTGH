using UnityEngine;

public static class UtilityTools
{
    public static float RandomVarianceFloat()
    {
        float variance = Random.Range(-0.1f, 0.1f);
        return variance;
    }

    public static float RandomVarianceFloat(float value)
    {
        float variance = Random.Range(-value, value);
        return variance;
    }
}