using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Компонент, позволяющий отображать карту шума как спрайт
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class NoiseMapRenderer : MonoBehaviour
{
    public enum MapType
    {
        Noise,
        Color
    }
    
    private SpriteRenderer spriteRenderer;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void RenderMap(int width, int height, Color[] colorMap)
    {
        ApplyColorMap(width, height, colorMap);
    }

    /// <summary>
    /// Применение текстуры и спрайта для отображения
    /// </summary>
    private void ApplyColorMap(int width, int height, Color[] colors)
    {
        Texture2D texture = NoiseMapToTextureUtils.ColorMapToTexture(width, height, colors);
        spriteRenderer.sprite = Sprite.Create(texture,
            new Rect(0.0f, 0.0f, width, height),
            new Vector2(0.5f, 0.5f), 100.0f);
    }
}
