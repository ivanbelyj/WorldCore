using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class DebugSpritesBuilder : GenerationStage
{
    private const float OffsetStep = 5f;

    [SerializeField]
    private GameObject mapPrefab;

    [SerializeField]
    private BiomesManager biomesManager;

    [SerializeField]
    private uint biomeIdToDisplayMaskForTest;

    private Color[] regionColors;
    private Color[] regionZoneColors;
    private GameObject noiseMapsParent;

    private readonly struct MapConfig
    {
        public readonly string Name;
        public readonly Func<ChunkData, Color[]> GenerateColorMap;
        public readonly Color DefaultColor;
        public readonly Func<ChunkData, bool> ShouldGenerate;

        public MapConfig(
            string name,
            Func<ChunkData, Color[]> generateColorMap,
            Color defaultColor,
            Func<ChunkData, bool> shouldGenerate = null)
        {
            Name = name;
            GenerateColorMap = generateColorMap;
            DefaultColor = defaultColor;
            ShouldGenerate = shouldGenerate ?? (_ => true);
        }
    }

    public override void Initialize(WorldGenerationData worldGenerationData)
    {
        base.Initialize(worldGenerationData);
        noiseMapsParent = new GameObject("NoiseMaps");
    }

    protected override Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        regionColors = configurationProvider
            .GetRegionSchemas()
            .Select(x => x.debugColor)
            .ToArray();

        regionZoneColors = chunkData.HeightmapComponentsMapping
            ?.Select(x => x.debugColor)
            ?.ToArray() ?? Array.Empty<Color>();

        CreateSpriteMaps(chunkData);
        return Task.FromResult(chunkData);
    }

    private void CreateSpriteMaps(ChunkData chunkData)
    {
        float offset = 0f;
        foreach (var config in GetMapConfigs())
        {
            if (config.ShouldGenerate(chunkData))
            {
                Color[] colorMap = config.GenerateColorMap(chunkData);
                if (colorMap != null)
                {
                    CreateSpriteMap(chunkData, colorMap, config.Name, offset, config.DefaultColor);
                    offset += OffsetStep;
                }
            }
        }
    }

    private IEnumerable<MapConfig> GetMapConfigs()
    {
        return new[]
        {
            new MapConfig(
                "Heightmap",
                chunk => NoiseMapToTextureUtils.NoiseMapToColorMap(
                    chunk.TerrainData.GetHeights(0, 0, worldData.ChunkResolution, worldData.ChunkResolution)),
                Color.white,
                chunk => chunk.TerrainData != null),
            new MapConfig(
                "Temperature",
                chunk => NoiseMapToTextureUtils.NoiseMapToColorMap(chunk.Temperature),
                Color.red,
                chunk => chunk.Temperature != null),
            new MapConfig(
                "Moisture",
                chunk => NoiseMapToTextureUtils.NoiseMapToColorMap(chunk.Moisture),
                Color.cyan,
                chunk => chunk.Moisture != null),
            new MapConfig(
                "Radiation",
                chunk => NoiseMapToTextureUtils.NoiseMapToColorMap(chunk.Radiation),
                Color.yellow,
                chunk => chunk.Radiation != null),
            new MapConfig(
                "Variety",
                chunk => NoiseMapToTextureUtils.NoiseMapToColorMap(chunk.Variety),
                Color.magenta,
                chunk => chunk.Variety != null),
            new MapConfig(
                "Biomes",
                chunk => GenerateBiomesColorMap(chunk.BiomeIds, chunk.Variety),
                Color.white,
                chunk => chunk.BiomeMaskById != null && chunk.BiomeMaskById.Count > 0),
            new MapConfig(
                "BiomeMask",
                chunk => NoiseMapToTextureUtils.NoiseMapToColorMap(chunk.BiomeMaskById[biomeIdToDisplayMaskForTest]),
                Color.white,
                chunk => chunk.BiomeMaskById != null && chunk.BiomeMaskById.ContainsKey(biomeIdToDisplayMaskForTest)),
            new MapConfig(
                "Regions",
                chunk => GenerateCombinedRegionColorMap(chunk.WeightsByHeightmapRegion),
                Color.white,
                chunk => chunk.WeightsByHeightmapRegion != null && chunk.WeightsByHeightmapRegion.Length > 0),
            new MapConfig(
                "RegionZones",
                chunk => GenerateCombinedComponentColorMap(chunk.WeightsByHeightmapComponent),
                Color.white,
                chunk => chunk.WeightsByHeightmapComponent != null && chunk.WeightsByHeightmapComponent.Length > 0)
        };
    }

    private Color[] GenerateCombinedRegionColorMap(float[][,] weightsByWorldRegion)
    {
        if (weightsByWorldRegion == null || weightsByWorldRegion.Length == 0)
            return null;

        return GenerateWeightedColorMap(
            weightsByWorldRegion,
            (width, height) => new Color[width * height],
            GetRegionColor);
    }

    private Color[] GenerateCombinedComponentColorMap(float[][,] weightsByComponent)
    {
        if (weightsByComponent == null || weightsByComponent.Length == 0)
            return null;

        return GenerateWeightedColorMap(
            weightsByComponent,
            (width, height) => new Color[width * height],
            GetComponentColor);
    }

    private Color[] GenerateWeightedColorMap(
        float[][,] weights,
        Func<int, int, Color[]> createColorMap,
        Func<int, Color> getColor)
    {
        int width = weights[0].GetLength(1);
        int height = weights[0].GetLength(0);
        Color[] colorMap = createColorMap(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                float r = 0f, g = 0f, b = 0f;

                for (int i = 0; i < weights.Length; i++)
                {
                    float weight = weights[i][y, x];
                    sum += weight;
                    Color color = getColor(i);
                    r += weight * color.r;
                    g += weight * color.g;
                    b += weight * color.b;
                }

                colorMap[y * width + x] = Mathf.Abs(sum - 1f) > 0.0001f
                    ? Color.white // Highlight normalization errors
                    : new Color(r, g, b);
            }
        }

        return colorMap;
    }

    private Color[] GenerateBiomesColorMap(uint[,] biomesMap, float[,] variety)
    {
        if (biomesMap == null)
            return null;

        int width = biomesMap.GetLength(1);
        int height = biomesMap.GetLength(0);
        Color[] colorMap = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Biome biome = biomesManager.GetBiomeById(biomesMap[y, x]);
                colorMap[y * width + x] = biome.GroupColor;
            }
        }
        return colorMap;
    }

    private void CreateSpriteMap(ChunkData chunkData, Color[] colorMap, string mapName, float zOffset, Color defaultColor = default)
    {
        if (colorMap == null)
        {
            Debug.LogWarning($"Color map for {mapName} is null");
            return;
        }

        Texture2D texture = NoiseMapToTextureUtils.ColorMapToTexture(
            worldData.ChunkResolution, worldData.ChunkResolution, colorMap);

        Vector3 position = worldData.ChunkSize / 100f * new Vector3(
            chunkData.ChunkPosition.X,
            chunkData.ChunkPosition.Z,
            zOffset);

        GameObject spriteGO = Instantiate(mapPrefab, position, Quaternion.identity, noiseMapsParent.transform);
        spriteGO.name = $"Map_{mapName}_{chunkData.ChunkPosition}";

        var noiseRenderer = spriteGO.GetComponent<NoiseMapRenderer>();
        noiseRenderer.RenderMap(worldData.ChunkResolution, worldData.ChunkResolution, colorMap);

        var spriteRenderer = spriteGO.GetComponent<SpriteRenderer>();
        spriteRenderer.color = defaultColor == default ? Color.white : defaultColor;
    }

    private Color GetRegionColor(int regionIndex)
    {
        return regionIndex < regionColors.Length ? regionColors[regionIndex] : Color.gray;
    }

    private Color GetComponentColor(int componentIndex)
    {
        return componentIndex < regionZoneColors.Length ? regionZoneColors[componentIndex] : Color.gray;
    }
}