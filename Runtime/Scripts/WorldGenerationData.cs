using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Параметры генерации
/// </summary>
[System.Serializable]
public class WorldGenerationData
{
    [Tooltip("Каждому ключу генерации сопоставляется уникальный мир, который можно по нему однозначно восстановить")]
    [SerializeField] private int seed;
    /// <summary>
    /// Каждому ключу генерации сопоставляется уникальный мир,
    /// который можно по нему однозначно восстановить
    /// </summary>
    public int Seed { get => seed; private set => seed = value; }

    [SerializeField] private int chunkSize = 128;
    public int ChunkSize { get => chunkSize; private set => chunkSize = value; }

    /// <summary>
    /// Размерность карты высот и зависимых от нее параметров (разрешение).
    /// Превосходит размер чанка на единицу, т.к. карта высот накладывается
    /// не на "квадраты", из которых состоят чанки, а на вершины сетки
    /// </summary>
    public int ChunkResolution => ChunkSize + 1;
    [SerializeField] private int chunkHeight = 64;
    public int ChunkHeight { get => chunkHeight; private set => chunkHeight = value; }

    [Tooltip("Коэффициент, пропорционально изменяющий масштаб генерации")]
    [SerializeField]
    private float worldScale = 1f;
    /// <summary>
    /// Коэффициент, пропорционально изменяющий масштаб генерации
    /// </summary>
    public float WorldScale { get => worldScale; set => worldScale = value; }
}
