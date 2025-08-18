using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public abstract class GenerationStage : MonoBehaviour, IGenerationStage
{
    [SerializeField]
    private bool includeInGeneration = true;
    public bool IncludeInGeneration
    {
        get => includeInGeneration;
        set => includeInGeneration = value;
    }

    private string stageName;

    public string StageName
    {
        get
        {
            if (stageName == null)
            {
                stageName = GetType().Name;
            }
            return stageName;
        }
    }

    protected WorldGenerationData worldData;
    /// <summary>
    /// Для обеспечения детерминированности генерации каждого чанка, для каждого из них
    /// используется собственный объект Random, детерминированно определяемый позицией
    /// и ключом генерации
    /// </summary>
    protected System.Random randomForCurrentChunk;

    protected IWorldGenerationConfigurationProvider configurationProvider;

    public virtual void Initialize(WorldGenerationData worldGenerationData)
    {
        worldData = worldGenerationData;
    }

    public async Task<ChunkData> ProcessChunkAsync(ChunkData chunkData)
    {
        ChunkPosition cPos = chunkData.ChunkPosition;
        int seedForChunk = unchecked(((cPos.X << 16) | cPos.Z) * worldData.Seed);
        randomForCurrentChunk = new System.Random(seedForChunk);

        Random.InitState(seedForChunk * 61);

        return await ProcessChunkImplAsync(chunkData);
    }

    protected abstract Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData);

    protected void Awake()
    {
        configurationProvider = GetComponent<IWorldGenerationConfigurationProvider>();
    }
}
