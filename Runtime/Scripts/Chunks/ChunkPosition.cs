using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Позиция чанка в бесконечном мире относительно начала мира (чанк (0, 0))
/// </summary>
public struct ChunkPosition : IEquatable<ChunkPosition>
{

    public int X { get; private set; }
    public int Z { get; private set; }
    public ChunkPosition(int x, int z) {
        X = x;
        Z = z;
    }
    public override int GetHashCode()
    {
        return (X, Z).GetHashCode();
    }

    public ChunkPosition Top => new ChunkPosition(X, Z + 1);
    public ChunkPosition Right => new ChunkPosition(X + 1, Z);
    public ChunkPosition Bottom => new ChunkPosition(X, Z - 1);
    public ChunkPosition Left => new ChunkPosition(X - 1, Z);

    public override string ToString()
    {
        return $"({X}, {Z})";
    }

    public override bool Equals(object other) {
        if (other is ChunkPosition chunkPos) {
            return Equals(chunkPos);
        } else
            return false;
    }

    public bool Equals(ChunkPosition other)
    {
        return this.X == other.X && this.Z == other.Z;
    }

    public static bool operator==(ChunkPosition pos1, ChunkPosition pos2) {
        return pos1.Equals(pos2);
    }

    public static bool operator!=(ChunkPosition pos1, ChunkPosition pos2) {
        return !pos1.Equals(pos2);
    }
}
