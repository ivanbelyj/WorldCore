using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BiomeDetail
{
    [SerializeField]
    private TerrainDetail terrainDetail;
    public TerrainDetail TerrainDetail => terrainDetail;

    public Color dryColor;
    public Color healthyColor;

    [Tooltip("Максимальные и минимальные размеры детали увеличиваются в sizeMultiplier раз")]
    public float sizeMultiplier = 1f;

    public float noiseSpread = 1f;
    public float holeEdgePadding = 0f;

    public DetailsPlacingMode placingMode;

    [Tooltip("Порог случайного значения, при превышении которого деталь не располагается. ")]
    [Range(0, 1)]
    public float threshold = 0.5f;

    [Tooltip("Условная редкость детали, регулирующая вероятность ее появления. Фактически, "
        + "изменяет порядок влияния threshold (1, 10, 100...)")]
    public DetailRarity rarity = DetailRarity.Ordinary;

    [Tooltip("Множитель максимально возможного значения плотности")]
    public int densityMultiplier = 1;

    public BiomeDetail() {
        // dryColor = new Color(0.71f, 0.62f, 0.42f);
        // healthyColor = new Color(0.45f, 0.76f, 0.42f);
    }

    public DetailPrototype ToDetailPrototype(int noiseSeed, int variant) {
        bool useMesh = terrainDetail.usePrototypeMesh;
        return new DetailPrototype() {
            dryColor = dryColor,
            healthyColor = healthyColor,
            maxHeight = terrainDetail.maxHeight * sizeMultiplier,
            minHeight = terrainDetail.minHeight * sizeMultiplier,
            maxWidth = terrainDetail.maxWidth * sizeMultiplier,
            minWidth = terrainDetail.minWidth * sizeMultiplier,
            noiseSeed = noiseSeed,
            noiseSpread = noiseSpread,
            holeEdgePadding = holeEdgePadding,
            prototype = useMesh ? terrainDetail.prototypeVariants[variant] : null,
            prototypeTexture =  !useMesh ?
                terrainDetail.prototypeTextureVariants[variant] : null,
            renderMode = terrainDetail.renderMode,
            useInstancing = terrainDetail.useInstancing,
            usePrototypeMesh = useMesh
        };
    }
}
