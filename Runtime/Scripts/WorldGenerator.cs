using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Предоставляет возможности генерации чанков пользователю
/// </summary>
public class WorldGenerator : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Выводить ли общие сообщения о генерации чанков")]
    private bool showCommonLogMessages;

    [SerializeField]
    [Tooltip("Выводить ли сообщения о проведенных этапах генерации")]
    private bool showStagesLogMessages;

    [SerializeField]
    [Tooltip("Максимальное количество миллисекунд, которое считается допустимым для генерации этапа. "
        + " При превышении во время генерации будет выводиться предупреждение")]
    private float maxNormalMsPerStage = 200;

    [SerializeField]
    private BaseTerrainGeneration baseTerrainGeneration;

    [SerializeField]
    private BiomesGeneration biomesGeneration;

    [SerializeField]
    private BiomesMasksGeneration biomesMasksGeneration;

    [SerializeField]
    private TreesGeneration treesGeneration;

    [SerializeField]
    private DetailsGeneration detailsGeneration;

    // Создает игровые объекты чанков на сцене
    [SerializeField]
    private WorldBuilder worldBuilder;

    [SerializeField]
    private Texturing texturing;

    [SerializeField]
    private DebugSpritesBuilder debugSpritesBuilder;

    private WorldGenerationData worldData;
    private List<IGenerationStage> generationStages;

    /// <summary>
    /// Устанавливает исходные данные о мире перед тем, как генерировать чанки
    /// </summary>
    public void Initialize(WorldGenerationData wordData) {
        this.worldData = wordData;

        AddGenerationStages(worldData.Seed);
    }

    /// <summary>
    /// Добавление объектов, осуществляющих генерацию
    /// </summary>
    private void AddGenerationStages(int seed) {
        generationStages = new List<IGenerationStage>();
        
        generationStages.Add(baseTerrainGeneration);

        generationStages.Add(biomesGeneration);
        generationStages.Add(biomesMasksGeneration);
        generationStages.Add(treesGeneration);
        generationStages.Add(detailsGeneration);

        generationStages.Add(worldBuilder);
        generationStages.Add(texturing);
        generationStages.Add(debugSpritesBuilder);

        if (showCommonLogMessages)
            Debug.Log("Initializing WorldGenerator...");
        float startTime = GetTime();

        foreach(var stage in generationStages) {
            stage.Initialize(worldData);
        }
        if (showCommonLogMessages)
            Debug.Log($"Initialized. Elapsed: { GetTime() - startTime} ms");
    }

    /// <summary>
    /// Генерирует данные чанка, расположенного по заданной позиции
    /// </summary>
    public async Task<ChunkData> CreateChunkAsync(ChunkPosition chunkPos) {
        if (generationStages.Count == 0)
            throw new System.InvalidOperationException(
                "Generation stages must be set before chunk generation");
        
        TerrainData terrainData = CreateInitialTerrainData();
        ChunkData initialChunkData = new ChunkData() {
            ChunkPosition = chunkPos,
            TerrainData = terrainData
        };

        if (showCommonLogMessages)
            Debug.Log($"Creating chunk {chunkPos}...");
        float totalStartTime = GetTime();

        ChunkData lastProcessed = initialChunkData;
        foreach (var stage in generationStages) {
            if (stage.IncludeInGeneration) {
                if (showStagesLogMessages)
                    Debug.Log($"Stage {stage.StageName}");
                
                float startTime = GetTime();

                // lastProcessed = await Task.Run(() => stage.ProcessChunk(lastProcessed));
                lastProcessed = await stage.ProcessChunkAsync(lastProcessed);

                float elapsedMs = GetTime() - startTime;
                if (showStagesLogMessages)
                    Debug.Log($"Stage {stage.StageName} completed. Elapsed: { elapsedMs } ms");

                if (elapsedMs > maxNormalMsPerStage) {
                    Debug.LogWarning($"Max normal time ({maxNormalMsPerStage})" + 
                        $" per generation stage is exceeded ({elapsedMs}) by { stage.StageName }");
                }
            }
        }

        if (showCommonLogMessages)
            Debug.Log($"Chunk {chunkPos} created. Elapsed: { GetTime() - totalStartTime } ms");
            
        return lastProcessed;
    }

    private float GetTime() {
        return Time.realtimeSinceStartup * 1000;
    }

    /// <summary>
    /// Создание изначальных данных ландшафта, которые далее будут обрабатываться
    /// </summary>
    private TerrainData CreateInitialTerrainData() {
        return new TerrainData();
    }
}
