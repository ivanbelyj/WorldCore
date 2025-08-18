using System;
using System.Collections.Generic;
using UnityEngine;

// Todo: AI-generated
// Скорее всего нужно рефакторить
public static class NoiseHeightmapRegionUtils
{
    public static float[][,] GenerateMapWithWeights(
        int numRegions,
        float baseCellSize,
        int seed,
        int width,
        int height,
        Vector2 offset,
        float scaleMultiplier = 1f)
    {
        // Larger scaleMultiplier -> smaller regions (more detailed)
        float cellSize = baseCellSize / Mathf.Max(scaleMultiplier, 0.01f);
        float[][,] weights = InitializeWeightsArray(numRegions, width, height);

        // Generate Perlin noise for each region
        List<FastNoiseLite> noiseGenerators = CreateNoiseGenerators(numRegions, seed);
        float[,] rawNoiseMaps = GenerateRawNoiseMaps(noiseGenerators, width, height, offset, cellSize);

        // Compute normalized weights with Native region bias near (0,0)
        ComputeWeights(weights, rawNoiseMaps, offset, width, height, cellSize, numRegions);

        return weights;
    }

    private static float[][,] InitializeWeightsArray(int numRegions, int width, int height)
    {
        float[][,] weights = new float[numRegions][,];
        for (int r = 0; r < numRegions; r++)
        {
            weights[r] = new float[width, height];
        }
        return weights;
    }

    private static List<FastNoiseLite> CreateNoiseGenerators(int numRegions, int seed)
    {
        List<FastNoiseLite> noiseGenerators = new List<FastNoiseLite>();
        for (int i = 0; i < numRegions; i++)
        {
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetSeed(seed + i * 1000); // Unique seed per region
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(2);
            noise.SetFractalLacunarity(2f);
            noise.SetFractalGain(0.5f);
            noiseGenerators.Add(noise);
        }
        return noiseGenerators;
    }

    private static float[,] GenerateRawNoiseMaps(List<FastNoiseLite> noiseGenerators, int width, int height, Vector2 offset, float cellSize)
    {
        float[,] rawNoiseMaps = new float[noiseGenerators.Count, width * height];
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseX = (x - halfWidth + offset.x) / cellSize;
                float noiseY = (y - halfHeight + offset.y) / cellSize;

                for (int r = 0; r < noiseGenerators.Count; r++)
                {
                    float noiseVal = noiseGenerators[r].GetNoise(noiseX, noiseY);
                    rawNoiseMaps[r, y * width + x] = To01(noiseVal); // Map [-1,1] to [0,1]
                }
            }
        }
        return rawNoiseMaps;
    }

    private static void ComputeWeights(float[][,] weights, float[,] rawNoiseMaps, Vector2 offset, int width, int height, float cellSize, int numRegions)
    {
        float nativeRadius = cellSize; // Radius for Native region dominance near (0,0)
        const float sharpness = 100f; // Controls transition sharpness (higher = narrower transitions)

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                float[] pointWeights = new float[numRegions];
                float totalWeight = 0f;

                // Softmax-like weighting for sharp but smooth transitions
                float maxNoiseVal = 0f;
                for (int r = 0; r < numRegions; r++)
                {
                    maxNoiseVal = Mathf.Max(maxNoiseVal, rawNoiseMaps[r, index]);
                }

                for (int r = 0; r < numRegions; r++)
                {
                    float noiseVal = rawNoiseMaps[r, index];
                    // Emphasize the region with the highest noise value
                    pointWeights[r] = Mathf.Exp(sharpness * (noiseVal - maxNoiseVal));
                    totalWeight += pointWeights[r];
                }

                // Apply Native region bias near (0,0)
                Vector2 globalPos = offset + new Vector2(x, y);
                float distToOrigin = globalPos.magnitude;
                if (distToOrigin < nativeRadius)
                {
                    float nativeFactor = 1f - (distToOrigin / nativeRadius); // Linear falloff
                    pointWeights[0] += nativeFactor * totalWeight; // Boost Native region
                    totalWeight += nativeFactor * totalWeight;
                }

                // Normalize weights to ensure sum = 1
                for (int r = 0; r < numRegions; r++)
                {
                    weights[r][y, x] = pointWeights[r] / totalWeight;
                }
            }
        }
    }

    // Map [-1,1] to [0,1]
    private static float To01(float val)
    {
        return (val + 1f) / 2f;
    }
}
