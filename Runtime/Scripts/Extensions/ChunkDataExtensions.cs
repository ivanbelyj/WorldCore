using UnityEngine;

public static class ChunkDataExtensions
{
    public static Vector2 GetNoiseOffset(
        this ChunkData chunkData,
        WorldGenerationData worldData)
        => new(
            chunkData.ChunkPosition.X * worldData.ChunkSize,
            chunkData.ChunkPosition.Z * worldData.ChunkSize);
}
