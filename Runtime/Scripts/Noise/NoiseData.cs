using UnityEngine;

/// <summary>
/// Данные для генерации шума
/// </summary>
[System.Serializable]
public struct NoiseData
{
    public NoiseType noiseType;
    public NoiseType NoiseType => noiseType;

    public int octaves;
    public int Octaves => octaves;
    public float persistence;
    public float Persistence => persistence;
    public float lacunarity;
    public float Lacunarity => lacunarity;

    public float scale;
    public float Scale { get => scale; }

    public float redistributionExtent;
    /// <summary>
    /// Степень, в которую будет возводиться сгенерированное значение шума. Значение больше 1
    /// делает значения шума, близкие к 0, ниже, а к 1 - выше.
    /// </summary>
    public float RedistributionExtent
    {
        get => redistributionExtent;
    }

    public float minValue;
    public float MinValue => minValue;

    public float maxValue;
    public float MaxValue => maxValue;

    public int seedModifier;
    public int SeedModifier => seedModifier;
}
