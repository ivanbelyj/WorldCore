using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Данные о чанке, передаваемые между этапами генерации. По умолчанию разрешение 
/// дополнительных карт (таких, как карты влажности, температуры, радиации, и т.д.)
/// равно разрешению карты высот
/// </summary>
public class ChunkData
{
    public TerrainData TerrainData { get; set; }
    public ChunkPosition ChunkPosition { get; set; }
    public float[,] Temperature { get; set; }
    public float[,] Moisture { get; set; }
    public float[,] Radiation { get; set; }
    public float[,] Variety { get; set; }

    /// <summary>
    /// Id биомов, сопоставленные каждой точке Terrain'а (размер зависит от разрешения карты высот)
    /// </summary>
    public uint[,] BiomeIds { get; set; }

    /// <summary>
    /// id биомов, которые встретились в данном чанке
    /// </summary>
    public HashSet<uint> ChunkBiomes { get; set; }

    /// <summary>
    /// Маски биомов чанка по id биомов. Используется, например, для текстурирования биомов
    /// </summary>
    public Dictionary<uint, float[,]> BiomeMaskById { get; set; }

    // Необходимо для отображения одной из карт интерполированной маски биома
    // для наглядности при разработке
    // Todo: хранить здесь карты всех биомов
    public float[,] InterpolatedBiomeMask { get; set; }

    public Terrain Terrain { get; set; }

    public ChunkData() {
        
    }
}
