using UnityEngine;

/// <summary>
/// Класс, предоставляющий методы, генерирующие и обрабатывающие карту шума
/// </summary>
public static class NoiseMapUtils
{
    public static float[,] GenerateNoiseMap(
        NoiseData noiseData,
        int seed,
        int width,
        int height,
        Vector2 offset,
        float scaleMultiplier = 1f,
        float minValue = 0f,
        float maxValue = 1f)
    {
        const float EPS = 1e-6f;

        float[,] noiseMap = new float[height, width];

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        FastNoiseLite fastNoiseLite;

        switch (noiseData.NoiseType)
        {
            case NoiseType.Simplex:
                fastNoiseLite = Simplex(noiseData);
                break;
            case NoiseType.Perlin:
                fastNoiseLite = Perlin(noiseData);
                break;
            case NoiseType.Ridged:
                fastNoiseLite = Ridged(noiseData);
                break;
            default:
                throw new System.ArgumentException("Unknown noise type");
        }
        int seedMultiplier = noiseData.SeedModifier == 0 ? 1 : noiseData.SeedModifier;
        fastNoiseLite.SetSeed(unchecked(seed * seedMultiplier));

        // Проход по точкам карты высот
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Координаты для получения значения из шума
                float noiseX = (x - halfWidth + offset.x) * noiseData.Scale * scaleMultiplier;
                float noiseY = (y - halfHeight + offset.y) * noiseData.Scale * scaleMultiplier;

                float generatedVal = fastNoiseLite.GetNoise(noiseX, noiseY);

                float noiseVal = To01(generatedVal);

                // Перераспределение
                if (Mathf.Abs(noiseData.RedistributionExtent - 1f) < EPS)
                {
                    // Перераспределение не требуется
                }
                else if (Mathf.Abs(noiseData.RedistributionExtent - 2f) < EPS)
                {
                    noiseVal = noiseVal * noiseVal;
                }
                else if (Mathf.Abs(noiseData.RedistributionExtent - 3f) < EPS)
                {
                    noiseVal = noiseVal * noiseVal * noiseVal;
                }
                else
                {
                    noiseVal = Mathf.Pow(noiseVal, noiseData.RedistributionExtent);
                }

                noiseVal = Remap(noiseVal, 0f, 1f, minValue, maxValue);

                noiseMap[y, x] = noiseVal;
            }
        }

        return noiseMap;
    }

    private static FastNoiseLite Cellular(NoiseData noiseData, int seed)
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        noise.SetSeed(seed);
        noise.SetFractalType(FastNoiseLite.FractalType.PingPong);
        noise.SetFractalOctaves(4);
        noise.SetFractalLacunarity(2);
        noise.SetFractalGain(0.9f);
        noise.SetFractalWeightedStrength(0.7f);
        noise.SetFractalPingPongStrength(3f);

        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Euclidean);
        noise.SetCellularReturnType(FastNoiseLite.CellularReturnType.Distance2Add);
        noise.SetCellularJitter(1f);

        return noise;
    }

    private static FastNoiseLite Simplex(NoiseData noiseData)
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(noiseData.Octaves);
        noise.SetFractalLacunarity(noiseData.Lacunarity);
        noise.SetFractalGain(noiseData.Persistence);
        return noise;
    }

    // Не тестировано
    private static FastNoiseLite Perlin(NoiseData noiseData)
    {
        FastNoiseLite noise = new FastNoiseLite();
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        noise.SetFractalOctaves(noiseData.Octaves);
        noise.SetFractalLacunarity(noiseData.Lacunarity);
        noise.SetFractalGain(noiseData.Persistence);
        return noise;
    }

    private static FastNoiseLite Ridged(NoiseData noiseData)
    {
        var res = Simplex(noiseData);
        res.SetFractalType(FastNoiseLite.FractalType.Ridged);
        return res;
    }

    // [-1; 1] -> [0; 1]
    private static float To01(float val)
    {
        return (val + 1) / 2;
    }

    // For example, [-1; 1] -> [0; 1]
    private static float Remap(
        float value,
        float fromMin,
        float fromMax,
        float toMin,
        float toMax)
    {
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
}
