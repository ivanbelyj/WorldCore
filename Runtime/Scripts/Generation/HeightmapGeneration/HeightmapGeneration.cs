using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class HeightmapGeneration : GenerationStage
{
    protected override async Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        var heights = await GetTotalHeightmap(chunkData);
        SetChunkData(chunkData, heights);

        return chunkData;
    }

    private int ChunkResolution => worldData.ChunkResolution;

    private async Task<float[,]> GetTotalHeightmap(ChunkData chunkData)
    {
        var height = ChunkResolution;
        var width = ChunkResolution;

        var totalHeights = new float[height, width];
        var heightmapComponents = await GetHeightmapComponentsAsync(
            chunkData,
            GetHeightmapComponentsNoiseData(chunkData));

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                totalHeights[i, j] = GetTotalHeight(
                    heightmapComponents,
                    chunkData.WeightsByHeightmapComponent,
                    i, j
                );
            }
        }
        return totalHeights;
    }

    private NoiseData[] GetHeightmapComponentsNoiseData(ChunkData chunkData)
    {
        return chunkData
            .HeightmapComponentsMapping
            .Select(x => x.heightmapGenerationSchema.noiseData)
            .ToArray();
    }

    private float GetTotalHeight(
        float[][,] heightmapComponentMaps,
        float[][,] weightMaps,
        int heightmapRow,
        int heightmapCol)
    {
        float total = 0f;
        for (int i = 0; i < heightmapComponentMaps.Length; i++)
        {
            total += weightMaps[i][heightmapRow, heightmapCol] * heightmapComponentMaps[i][heightmapRow, heightmapCol];
        }
        return total;
    }

    private async Task<float[][,]> GetHeightmapComponentsAsync(
        ChunkData chunkData,
        NoiseData[] noiseDataArray)
    {
        var noiseOffset = chunkData.GetNoiseOffset(worldData);

        var tasks = new Task<float[,]>[noiseDataArray.Length];

        for (int i = 0; i < noiseDataArray.Length; i++)
        {
            var noiseData = noiseDataArray[i];
            tasks[i] = GetHeightmapComponent(noiseOffset, noiseData);
        }

        var results = await Task.WhenAll(tasks);

        return results;
    }

    private async Task<float[,]> GetHeightmapComponent(
        Vector2 noiseOffset,
        NoiseData noiseData)
    {
        return await Task.Run(
            () => NoiseMapUtils.GenerateNoiseMap(
                noiseData,
                worldData.Seed,
                ChunkResolution,
                ChunkResolution,
                noiseOffset,
                worldData.WorldScale,
                noiseData.MinValue,
                noiseData.MaxValue));
    }

    private void SetChunkData(ChunkData chunkData, float[,] heights)
    {
        // Применение карты высот и настроек к TerrainData
        Vector3 terrainSize = new(
            worldData.ChunkSize,
            worldData.ChunkHeight / worldData.WorldScale,
            worldData.ChunkSize);

        /// ===================================
        // ВНИМАНИЕ! МИНУТКА ВОЛШЕБСТВА
        // Попробуйте убрать один из этих абсолютно идентичных блоков
        // и посмотрите, как неведомые силы изменят рельеф
        chunkData.TerrainData.size = terrainSize;
        chunkData.TerrainData.heightmapResolution = ChunkResolution;

        chunkData.TerrainData.size = terrainSize;
        chunkData.TerrainData.heightmapResolution = ChunkResolution;
        // Спустя часы поисков проблемы удалось свести к этим волшебным строчкам,
        // но истинные причины навсегда останутся в темных недрах Unity...
        /// ====================================


        chunkData.TerrainData.SetHeights(0, 0, heights);
    }
}
