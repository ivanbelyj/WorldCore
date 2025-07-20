using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Точка присутствия, вокруг которой активируются чанки в определенном радиусе.
/// В начале игры инициирует создание области чанков 
/// </summary>
public class PresentInChunk : MonoBehaviour
{
    [SerializeField]
    private ChunkWorld chunkWorld;

    [SerializeField]
    [Tooltip("Позиция чанка, в котором изначально находится точка присутствия")]
    private ChunkPosition initialChunkPosition;

    /// <summary>
    /// Последняя позиция присутствия, для которой были созданы / скрыты чанки
    /// </summary>
    private ChunkPosition lastChunkPosition;

    private bool updatingPointsOfPresence = false;

    private uint presencePointId;

    private static uint lastPresencePointId = 0;
    public static uint NewPresencePointId()
    {
        return ++lastPresencePointId;
    }

    private async void Start()
    {
        updatingPointsOfPresence = true;
        presencePointId = NewPresencePointId();
        if (chunkWorld == null)
        {
            chunkWorld = FindAnyObjectByType<ChunkWorld>();
        }
        await chunkWorld.AddPresencePointAsync(presencePointId, initialChunkPosition);
        updatingPointsOfPresence = false;

        transform.position = chunkWorld.PosInActiveChunkToWorldPos(initialChunkPosition);
    }


    private void OnDestroy()
    {
        chunkWorld.RemovePresencePoint(presencePointId);
    }

    private ChunkPosition GetCurrentChunkPosition()
    {
        // return initialChunkPosition;
        return chunkWorld.WorldPosToChunkPos(transform.position);
    }

    private async void Update()
    {
        ChunkPosition currentPos = GetCurrentChunkPosition();
        if (currentPos != lastChunkPosition && !updatingPointsOfPresence)
        {
            // Debug.Log($"Old pos: {lastChunkPosition}; newPos: {currentPos}. Need to update presence point");
            updatingPointsOfPresence = true;
            await chunkWorld.UpdatePresencePoint(presencePointId, currentPos);
            updatingPointsOfPresence = false;
        }
        lastChunkPosition = currentPos;
    }
}
