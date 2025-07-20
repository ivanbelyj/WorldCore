using UnityEngine;

/// <summary>
/// Данные для генерации шума
/// </summary>
[System.Serializable]
public struct NoiseData
{
    [SerializeField] private NoiseType noiseType;
    public NoiseType NoiseType => noiseType;

    [SerializeField] private int octaves;
    public int Octaves => octaves;
    [SerializeField] private float persistence;
    public float Persistence => persistence;
    [SerializeField] private float lacunarity;
    public float Lacunarity => lacunarity;

    [SerializeField] private float scale;
    public float Scale { get => scale; }

    [SerializeField] private float redistributionExtent;
    /// <summary>
    /// Степень, в которую будет возводиться сгенерированное значение шума. Значение больше 1
    /// делает значения шума, близкие к 0, ниже, а к 1 - выше.
    /// </summary>
    public float RedistributionExtent {
        get => redistributionExtent;
    }

    [SerializeField] private int seedModifier;
    public int SeedModifier => seedModifier;
}
