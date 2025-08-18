using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BaseTerrainGeneration : GenerationStage
{
    public enum BaseTerrainGenerationSource { NoiseData, HeightmapGenerationSchema }

    [SerializeField]
    private BaseTerrainGenerationSource generationSource;

    [SerializeField]
    private NoiseData noiseData;

    [SerializeField]
    private HeightmapGenerationSchema heightmapGenerationSchema;

    protected async override Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        var noiseData = generationSource switch
        {
            BaseTerrainGenerationSource.NoiseData => this.noiseData,
            BaseTerrainGenerationSource.HeightmapGenerationSchema => heightmapGenerationSchema.noiseData,
            _ => throw new InvalidOperationException()
        };

        int chunkRes = worldData.ChunkResolution;

        // Создание карты шума в виде массива
        var noiseOffset = chunkData.GetNoiseOffset(worldData);

        // Создание матрицы шума, чтобы в дальнейшем назначить Terrain через TerrainData
        float[,] heights = await Task.Run(
            () => NoiseMapUtils.GenerateNoiseMap(
                noiseData,
                worldData.Seed,
                chunkRes,
                chunkRes,
                noiseOffset,
                worldData.WorldScale,
                noiseData.MinValue,
                noiseData.MaxValue));

        // Применение карты высот и настроек к TerrainData
        Vector3 terrainSize = new Vector3(worldData.ChunkSize,
            worldData.ChunkHeight / worldData.WorldScale, worldData.ChunkSize);

        /// ===================================
        // ВНИМАНИЕ! МИНУТКА ВОЛШЕБСТВА
        // Попробуйте убрать один из этих абсолютно идентичных блоков
        // и посмотрите, как неведомые силы изменят рельеф
        chunkData.TerrainData.size = terrainSize;
        chunkData.TerrainData.heightmapResolution = chunkRes;

        chunkData.TerrainData.size = terrainSize;
        chunkData.TerrainData.heightmapResolution = chunkRes;
        // Спустя часы поисков проблемы удалось свести к этим волшебным строчкам,
        // но истинные причины навсегда останутся в темных недрах Unity...
        /// ====================================


        chunkData.TerrainData.SetHeights(0, 0, heights);

        return chunkData;
    }
}
