using UnityEngine;

public class PerlinNoise
{
    private const float C = 1000;

    public static float GetNoise(int seed, float x, float y, float scale = 1f)
    {
        x = (x + seed * C) * scale;
        y = (y + seed * C) * scale;
        return Mathf.Clamp01(Mathf.PerlinNoise(x, y));
    }
}