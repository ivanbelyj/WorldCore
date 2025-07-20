using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Методы для преобразования карты шума в текстуры
/// </summary>
public static class NoiseMapToTextureUtils
{
    /// <summary>
    /// Создание текстуры заданного размера с заданными цветами
    /// </summary>
    public static Texture2D ColorMapToTexture(int width, int height, Color[] colors)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }

    /// <summary>
    /// Преобразует массив с данными о шуме в массив чёрно-белых цветов, для передачи в текстуру
    /// </summary>
    public static Color[] NoiseMapToColorMap(float[,] noiseMap)
    {
        Color[] colorMap = new Color[noiseMap.Length];
        int height = noiseMap.GetLength(0);
        int width = noiseMap.GetLength(1);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[y, x]);
            }
        }
        return colorMap;
    }
}
