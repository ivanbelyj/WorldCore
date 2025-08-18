using System.Threading.Tasks;
using UnityEngine;

public class HeightmapRegionGeneration : GenerationStage
{
    [SerializeField]
    private float baseCellSize = 1024f;

    protected override async Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        int chunkRes = worldData.ChunkResolution;

        var noiseOffset = chunkData.GetNoiseOffset(worldData);

        var regionSchemas = configurationProvider.GetRegionSchemas();

        chunkData.WeightsByHeightmapRegion = await Task.Run(
            () => NoiseHeightmapRegionUtils.GenerateMapWithWeights(
                regionSchemas.Count,
                baseCellSize,
                worldData.Seed,
                chunkRes,
                chunkRes,
                noiseOffset,
                worldData.WorldScale
            )
        );
        return chunkData;
    }
}
