using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Создает тестовый ландшафт из нескольких чанков для демонстрации системы генерации
/// </summary>
public class ChunkWorld : MonoBehaviour
{
    // Генерирует данные чанков
    [SerializeField]
    private WorldGenerator worldGenerator;

    // Параметры генерации мира
    [SerializeField]
    private WorldGenerationData worldGenerationData;

    [SerializeField]
    [Tooltip("Радиус активных чанков вокруг точек присутствия. 1 - активен только тот чанк, "
        + "в котором находится точка присутствия")]
    private int activeChunksRadius = 1;

    /// <summary>
    /// Позволяет получать данные созданных чанков из стороннего кода
    /// </summary>
    [HideInInspector]
    public Dictionary<ChunkPosition, ChunkData> CreatedChunkByPosition { get; set; }

    /// <summary>
    /// Позиции, на которых чанки активированы
    /// </summary>
    [HideInInspector]
    public HashSet<ChunkPosition> ActiveChunks { get; set; }

    [HideInInspector]
    public event Action<ChunkData> ChunkCreated;

    private Dictionary<uint, ChunkPosition> presencePointById;
    
    private void Awake()
    {
        CreatedChunkByPosition = new Dictionary<ChunkPosition, ChunkData>();
        ActiveChunks = new HashSet<ChunkPosition>();
        presencePointById = new Dictionary<uint, ChunkPosition>();

        worldGenerator.Initialize(worldGenerationData);
    }

    /// <summary>
    /// По мировой координате определяет чанк, в котором находится данная позиция
    /// </summary>
    public ChunkPosition WorldPosToChunkPos(Vector3 pos) {
        int x = GetChunkPos(pos.x);
        int z = GetChunkPos(pos.z);
        return new ChunkPosition(x, z);

        int GetChunkPos(float pos) {
            return Mathf.FloorToInt(pos / worldGenerationData.ChunkSize);
        }
    }

    /// <param name="offset">По умолчанию применяется отступ, перемещающий позицию в центр чанка</param>
    public Vector3 PosInActiveChunkToWorldPos(ChunkPosition chunkPos, Vector3 offset = default) {
        if (!ActiveChunks.Contains(chunkPos)) {
            Debug.LogError("Trying to convert pos in unactive chunk to world pos."
                + " Height can not be defined.");
        }

        if (offset == default) {
            float halfChunk = worldGenerationData.ChunkSize / 2f;
            offset = new Vector3(halfChunk, 0, halfChunk);
        }
        
        ChunkData chunkData = CreatedChunkByPosition[chunkPos];
        Vector3 worldPos = new Vector3(GetWorldPos(chunkPos.X) + offset.x,
            0, GetWorldPos(chunkPos.Z) + offset.z);
        worldPos.y = chunkData.Terrain.SampleHeight(worldPos);

        return worldPos;

        float GetWorldPos(int pos) {
            return pos * worldGenerationData.ChunkSize;
        }
    }

    private void SetChunkActive(ChunkPosition chunkPosition, bool isActive) {
        CreatedChunkByPosition[chunkPosition].Terrain.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Генерирует чанк и создает его на сцене
    /// </summary>
    // private async Task GenerateAndCreateChunkGO(ChunkPosition pos) {
    //     Debug.Log($"GenerateAndCreateChunkGO {pos}");
    //     ChunkData chunkData = await worldGenerator.CreateChunkAsync(pos);
    //     CreatedChunkByPosition.Add(pos, chunkData);
    //     Debug.Log($"GenerateAndCreateChunkGO {pos} completed.");
        
    //     ChunkCreated?.Invoke(chunkData);
    // }

    // В данный момент система точек присутствия (скрытие дальних чанков,
    // уничтожение слишком далеких, и т.д.) работает ограниченно и в дальнейшем будет переписана
    #region Old Points Of Presence
    public async Task AddPresencePointAsync(uint pointId, ChunkPosition chunkPos) {
        presencePointById.Add(pointId, chunkPos);
        for (int x = chunkPos.X - activeChunksRadius + 1; x <= chunkPos.X + activeChunksRadius - 1; x++) {
            for (int z = chunkPos.Z - activeChunksRadius + 1; z <= chunkPos.Z + activeChunksRadius - 1; z++) {
                ChunkPosition pos = new ChunkPosition(x, z);
                // Если чанк еще не сгенерирован
                if (!CreatedChunkByPosition.ContainsKey(pos)) {
                    ChunkData chunkData = await worldGenerator.CreateChunkAsync(pos);
                    CreatedChunkByPosition.Add(pos, chunkData);
                    
                    ChunkCreated?.Invoke(chunkData);
                }
                // Если создан, но неактивен
                else if (!ActiveChunks.Contains(pos)) {
                    SetChunkActive(pos, true);
                }
                ActiveChunks.Add(pos);
            }
        }
    }

    public void RemovePresencePoint(uint pointId) {
        ChunkPosition chunkPos = presencePointById[pointId];
        presencePointById.Remove(pointId);

        // Todo: deactivate chunks:
        return;

        HashSet<ChunkPosition> chunksToDeactivate = new HashSet<ChunkPosition>();
        for (int x = chunkPos.X - activeChunksRadius + 1; x <= chunkPos.X + activeChunksRadius - 1; x++) {
            for (int z = chunkPos.Z - activeChunksRadius + 1; z <= chunkPos.Z + activeChunksRadius - 1; z++) {
                ChunkPosition pos = new ChunkPosition(x, z);
                // Если чанк создан и активен
                if (CreatedChunkByPosition.ContainsKey(pos) && ActiveChunks.Contains(pos)
                    // Метод RemovePointOfPresence может быть вызван при уничтожении GameObject
                    // с компонентом PresentInChunk, а уничтожение может происходить в том числе
                    // при закрытии сцены. При закрытии сцены Terrain могут быть уже уничтожены,
                    // поэтому необходимо проверять, не уничтожены ли они, чтобы не пытаться
                    // отключать уничтоженный объект
                    && CreatedChunkByPosition[pos].Terrain != null
                    // Помимо всего прочего, деактивируются чанки, далекие от точек присутствия
                    && presencePointById.Values.All(p => Vector3.Distance(new Vector3(p.X, 0, p.Z),
                        new Vector3(pos.X, 0, pos.Z)) >= activeChunksRadius)) {                    
                    chunksToDeactivate.Add(pos);
                }
            }
        }
        foreach (ChunkPosition pos in chunksToDeactivate) {
            // CreatedChunkByPosition.Remove(pos);
            SetChunkActive(pos, false);
            ActiveChunks.Remove(pos);
            // Destroy(CreatedChunkByPosition[pos].Terrain.gameObject);
        }
    }

    public async Task UpdatePresencePoint(uint pointId, ChunkPosition newPos) {
        RemovePresencePoint(pointId);
        await AddPresencePointAsync(pointId, newPos);
    }

    #endregion

    // Todo:
    #region Points Of Presence
    // public async Task AddPresencePoint(uint presencePointId, ChunkPosition pos) {
    //     presencePointById.Add(presencePointId, pos);
    //     Debug.Log("AddPresencePoint");
    //     DebugLogChunksAround();
    //     await ActivateAroundOfPresencePointsAsync();
    // }

    // public void RemovePresencePoint(uint presencePointId) {
    //     presencePointById.Remove(presencePointId);
    //     Debug.Log("RemovePresencePoint");
    //     DebugLogChunksAround();
    //     DeactivateOutOfPresencePointsAsync();
    // }

    // public async Task UpdatePresencePoint(uint presencePointId, ChunkPosition newPos) {
    //     Debug.Log("UpdatePresencePoint");
    //     presencePointById[presencePointId] = newPos;
    //     DeactivateOutOfPresencePointsAsync();
    //     DebugLogChunksAround();
    //     await ActivateAroundOfPresencePointsAsync();
    // }

    // private async Task ActivateChunk(ChunkPosition pos) {
    //     ActiveChunks.Add(pos);
    //     // Если чанк еще не сгенерирован
    //     if (!CreatedChunkByPosition.ContainsKey(pos)) {
    //         ChunkData chunkData = await worldGenerator.CreateChunkAsync(pos);
    //         CreatedChunkByPosition.Add(pos, chunkData);
            
    //         ChunkCreated?.Invoke(chunkData);
    //     }
    //     // Если создан, но неактивен
    //     else if (!ActiveChunks.Contains(pos)) {
    //         SetChunkActive(pos, true);
    //     }
    // }
    
    // private void DeactivateChunk(ChunkPosition pos) {
    //     ActiveChunks.Remove(pos);
    //     SetChunkActive(pos, false);
    // }

    // private HashSet<ChunkPosition> ChunksAroundPresencePoints() {
    //     HashSet<ChunkPosition> res = new HashSet<ChunkPosition>();
    //     foreach ((uint id, ChunkPosition pos) in presencePointById) {
    //         for (int x = pos.X - activeChunksRadius + 1;
    //                 x <= pos.X + activeChunksRadius - 1; x++) {
    //             for (int z = pos.Z - activeChunksRadius + 1;
    //                 z <= pos.Z + activeChunksRadius - 1; z++) {
    //                 res.Add(new ChunkPosition(x, z));
    //             }
    //         }
    //     }
    //     return res;
    // }

    // private void DebugLogChunksAround() {
    //     string str = "";
    //     foreach (ChunkPosition pos in ChunksAroundPresencePoints()) {
    //         str += pos + ", ";
    //     }
    //     Debug.Log("ChunksAroundPresencePoints: " + str);
    // }

    // private void DeactivateOutOfPresencePointsAsync() {
    //     var aroundPointsOfPresence = ChunksAroundPresencePoints();

    //     var outOfRadius = ActiveChunks.Except(aroundPointsOfPresence);
    //     foreach (var chunk in outOfRadius) {
    //         DeactivateChunk(chunk);
    //     }
    // }

    // private async Task ActivateAroundOfPresencePointsAsync() {
    //     var aroundPointsOfPresence = ChunksAroundPresencePoints();

    //     foreach (var chunk in aroundPointsOfPresence) {
    //         if (!ActiveChunks.Contains(chunk)) {
    //             await ActivateChunk(chunk);
    //         }
    //     }
    // }

    #endregion
}
