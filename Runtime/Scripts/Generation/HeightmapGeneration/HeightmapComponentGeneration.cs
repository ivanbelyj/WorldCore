using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

// Todo: AI-generated
// Скорее всего нужно рефакторить
public class HeightmapComponentGeneration : GenerationStage
{
    [SerializeField]
    private float baseComponentSize = 128f; // Base size for components (smaller than regions)

    protected override async Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        int chunkRes = worldData.ChunkResolution;
        var noiseOffset = chunkData.GetNoiseOffset(worldData);

        var regionSchemas = configurationProvider.GetRegionSchemas();
        var componentMapping = BuildComponentMapping(regionSchemas);
        chunkData.HeightmapComponentsMapping = componentMapping;

        chunkData.WeightsByHeightmapComponent = await Task.Run(
            () => GenerateComponentWeights(
                chunkData.WeightsByHeightmapRegion,
                componentMapping,
                regionSchemas,
                baseComponentSize,
                worldData.Seed,
                chunkRes,
                chunkRes,
                noiseOffset,
                worldData.WorldScale
            )
        );
        return chunkData;
    }

    private HeightmapRegionZoneGenerationSchema[] BuildComponentMapping(
        List<HeightmapRegionGenerationSchema> regionSchemas)
    {
        var components = new List<HeightmapRegionZoneGenerationSchema>();
        for (int r = 0; r < regionSchemas.Count; r++)
        {
            var zoneSchemas = regionSchemas[r].zoneSchemas;
            for (int z = 0; z < zoneSchemas.Count; z++)
            {
                components.Add(zoneSchemas[z]);
            }
        }
        return components.ToArray();
    }

    private float[][,] GenerateComponentWeights(
        float[][,] regionWeights,
        HeightmapRegionZoneGenerationSchema[] componentMapping,
        List<HeightmapRegionGenerationSchema> regionSchemas,
        float baseComponentSize,
        int seed,
        int width,
        int height,
        Vector2 offset,
        float scaleMultiplier = 1f)
    {
        float componentSize = baseComponentSize / Mathf.Max(scaleMultiplier, 0.01f);
        float[][,] componentWeights = new float[componentMapping.Length][,];
        for (int i = 0; i < componentMapping.Length; i++)
        {
            componentWeights[i] = new float[width, height];
        }

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        // Create noise generators for each component
        var componentNoiseGenerators = new List<FastNoiseLite>();
        for (int i = 0; i < componentMapping.Length; i++)
        {
            FastNoiseLite noise = new FastNoiseLite();
            noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
            noise.SetSeed(seed + i * 100);
            noise.SetFractalType(FastNoiseLite.FractalType.FBm);
            noise.SetFractalOctaves(3);
            noise.SetFractalLacunarity(2f);
            noise.SetFractalGain(0.5f);
            componentNoiseGenerators.Add(noise);
        }

        const float sharpness = 5f; // Controls transition sharpness between components

        // Map components to regions for indexing
        var componentToRegion = new int[componentMapping.Length];
        int componentIndex = 0;
        for (int r = 0; r < regionWeights.Length; r++)
        {
            int zonesInRegion = regionSchemas[r].zoneSchemas.Count;
            for (int z = 0; z < zonesInRegion; z++)
            {
                componentToRegion[componentIndex] = r;
                componentIndex++;
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float noiseX = (x - halfWidth + offset.x) / componentSize;
                float noiseY = (y - halfHeight + offset.y) / componentSize;

                float[] pointComponentWeights = new float[componentMapping.Length];
                float totalComponentWeight = 0f;

                // Compute component weights for ALL regions, masked by region weight
                for (int r = 0; r < regionWeights.Length; r++)
                {
                    float regionWeight = regionWeights[r][y, x];
                    if (regionWeight > 0f) // Only compute for regions with non-zero weight
                    {
                        int zonesInRegion = regionSchemas[r].zoneSchemas.Count;
                        int startComponentIndex = GetStartComponentIndex(r, regionSchemas);

                        // Calculate softmax weights for components in this region
                        float maxComponentVal = 0f;
                        for (int z = 0; z < zonesInRegion; z++)
                        {
                            int c = startComponentIndex + z;
                            float componentVal = To01(componentNoiseGenerators[c].GetNoise(noiseX, noiseY));
                            maxComponentVal = Mathf.Max(maxComponentVal, componentVal);
                        }

                        for (int z = 0; z < zonesInRegion; z++)
                        {
                            int c = startComponentIndex + z;
                            float componentVal = To01(componentNoiseGenerators[c].GetNoise(noiseX, noiseY));
                            float componentWeight = Mathf.Exp(sharpness * (componentVal - maxComponentVal));
                            pointComponentWeights[c] = componentWeight * regionWeight; // Mask with region weight
                            totalComponentWeight += pointComponentWeights[c];
                        }
                    }
                }

                // Normalize weights
                for (int c = 0; c < componentMapping.Length; c++)
                {
                    componentWeights[c][y, x] = (totalComponentWeight > 0f)
                        ? pointComponentWeights[c] / totalComponentWeight
                        : (1f / componentMapping.Length);
                }

                // Debug: Check weight normalization
                float sum = 0f;
                for (int c = 0; c < componentMapping.Length; c++)
                    sum += componentWeights[c][y, x];
                if (Mathf.Abs(sum - 1f) > 0.0001f)
                {
                    Debug.LogWarning($"Component weights sum not 1 at ({x},{y}): {sum}");
                }
            }
        }

        return componentWeights;
    }

    private int GetStartComponentIndex(
        int r,
        List<HeightmapRegionGenerationSchema> regionSchemas)
    {
        int start = 0;
        for (int i = 0; i < r; i++)
        {
            start += regionSchemas[i].zoneSchemas.Count;
        }
        return start;
    }

    private static float To01(float val)
    {
        return (val + 1f) / 2f;
    }
}