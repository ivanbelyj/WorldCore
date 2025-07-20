using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Имеет доступ к списку существующих биомов и классифицирующей схеме, позволяет
/// классифицировать тип биома по передаваемым параметрам
/// </summary>
public class BiomesManager : MonoBehaviour
{
    [SerializeField]
    private Texture2D biomeSchemeImage;  // Изображение с матрицей классификации биомов
    [SerializeField]
    private Biome[] biomes;
    
    private Dictionary<Color, BiomesGroup> biomeGroupByColor;  // Соответствия цветов и биомов
    private Dictionary<uint, Biome> biomeById;

    private Color32[] biomeMapColors;  // Массив цветов изображения с матрицей биомов
    private int widthTemperature;  // Ширина матрицы биомов
    private int heightMoisture;  // Высота матрицы биомов

    public void Initialize()
    {
        // Загрузка цветов изображения-схемы биомов
        biomeMapColors = biomeSchemeImage.GetPixels32();
        widthTemperature = biomeSchemeImage.width;
        heightMoisture = biomeSchemeImage.height;

        biomeGroupByColor = new Dictionary<Color, BiomesGroup>();
        biomeById = new Dictionary<uint, Biome>();
        foreach (Biome biome in biomes) {
            if (!biomeGroupByColor.ContainsKey(biome.GroupColor)) {
                biomeGroupByColor.Add(biome.GroupColor, new BiomesGroup());
            }
            biomeGroupByColor[biome.GroupColor].Add(biome);

            biomeById.Add(biome.BiomeId, biome);
        }
    }

    public Biome GetBiomeById(uint id) {
        return biomeById[id];
    }

    public uint GetBiomeId(float moisture, float temperature, float radiation, float variety)
    {
        // Определение позиции в матрице биомов на основе влажности и температуры
        int x = Mathf.FloorToInt(temperature * (widthTemperature - 1));
        int y = Mathf.FloorToInt(moisture * (heightMoisture - 1));

        // Определение цвета в позиции x, y в матрице биомов
        Color32 color = biomeMapColors[y * widthTemperature + x];

        // Определение типа биома на основе цвета
        if (biomeGroupByColor.ContainsKey(color))
        {
            return biomeGroupByColor[color]
                .WithRadiation(radiation)
                .OfVariety(variety)
                .GetOne().BiomeId;
        }
        else
        {
            Debug.LogError("Unknown biome color: " + color);
            return 0;
        }
    }
}

