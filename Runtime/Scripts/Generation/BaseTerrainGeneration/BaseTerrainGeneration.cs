using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class BaseTerrainGeneration : GenerationStage
{
    [SerializeField]
    private NoiseData noiseData;

    protected async override Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        // chunkData = await base.ProcessChunk(chunkData);

        int chunkRes = worldData.ChunkResolution;

        // Создание карты шума в виде массива
        var noiseOffset = new Vector2(chunkData.ChunkPosition.X * worldData.ChunkSize,
            chunkData.ChunkPosition.Z * worldData.ChunkSize);

        // Создание матрицы шума, чтобы в дальнейшем назначить Terrain через TerrainData
        float[,] heights = await Task.Run(() => NoiseMapUtils.GenerateNoiseMap(noiseData,
            worldData.Seed, chunkRes, chunkRes, noiseOffset, worldData.WorldScale));

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
