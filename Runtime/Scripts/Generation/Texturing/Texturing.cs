using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using sc.terrain.proceduralpainter;
using UnityEngine;

/// <summary>
/// Использует сторонний компонент ProceduralTerrainPainter и данные биомов,
/// чтобы текстурировать чанки соответственно биомам
/// </summary>
public class Texturing : GenerationStage
{
    [SerializeField]
    [Tooltip("Во сколько раз будет увеличена маска биомов для получения текстуры. "
        + "Чем больше, тем более плавными будут границы биомов, но тем ресурсозатратнее вычисления")]
    private int scaleFactor;

    [SerializeField]
    private TerrainPainter terrainPainter;

    [SerializeField]
    private BiomesManager biomesManager;

    private int layersAddedForCurrentChunk;

    protected async override Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        // Добавление слоев, специфичных для биомов чанка
        foreach (var biomeIdAndMask in chunkData.BiomeMaskById) {
            Biome biome = biomesManager.GetBiomeById(biomeIdAndMask.Key);
            if (biome == null)
                Debug.LogError("Unknown biome");
            if (biome.LayerSettings == null || biome.LayerSettings.Length == 0)
                continue;

            float[,] interpolatedMask = await Task.Run(() => InterpolateBiomeMask(biomeIdAndMask.Value));
            chunkData.InterpolatedBiomeMask = interpolatedMask;
            
            AddBiomeLayerSettings(biome, await BiomeMaskToTexture2D(interpolatedMask));    
        }

        terrainPainter.SetTargetTerrains(new Terrain[] { chunkData.Terrain });
        terrainPainter.RepaintAll();

        // К следующим чанкам специфичные слои биома не должны применяться
        if (layersAddedForCurrentChunk > 0)
            RemoveLayerSettings();
        
        return chunkData;
    }

    // Интерполяция и сглаживание маски биома для нормального вида в игре
    private float[,] InterpolateBiomeMask(float[,] biomeMask) {
        float[,] interpolatedMask = MatrixProcessingUtils.InterpolateBilinear(
            biomeMask, scaleFactor);
        // float[,] smoothedMask = MatrixProcessingUtils.BlurLinear(
        //     interpolatedMask, fadeWidth
        // );

        return biomeMask;
    }

    // Создание текстуры маски биома
    private async Task<Texture2D> BiomeMaskToTexture2D(float[,] biomeMask) {
        int biomeMaskHeight = biomeMask.GetLength(0);
        int biomeMaskWidth = biomeMask.GetLength(1);

        Color[] biomeMaskColors = await Task.Run(() => NoiseMapToTextureUtils.NoiseMapToColorMap(biomeMask));
        Texture2D biomeMaskTexture = NoiseMapToTextureUtils.ColorMapToTexture(
            biomeMaskWidth, biomeMaskHeight, biomeMaskColors);

        return biomeMaskTexture;
    }

    private void AddBiomeLayerSettings(Biome biome, Texture2D biomeMaskTexture) {
        foreach (LayerSettings layerSettings in biome.LayerSettings) {

            // Исходный объект не должен изменяться, поэтому создается новый
            LayerSettings newLayerSettings = new LayerSettings() {
                enabled = layerSettings.enabled,
                layer = layerSettings.layer,
                modifierStack = new List<Modifier>(layerSettings.modifierStack)
            };
            TextureMask textureMask = (TextureMask)ScriptableObject.CreateInstance("TextureMask");
            textureMask.enabled = true;
            textureMask.texture = biomeMaskTexture;

            // В новый LayerSettings добавляется модификатор со сгенерированной в Runtime текстурой
            newLayerSettings.modifierStack.Add(textureMask);

            // Todo: из-за проблемы с сериализацией модификаторов в ассете самые необходимые 
            // модификаторы, независимые от биома, временно добавляются хардкодом, а должны
            // - статически в инспекторе

            // Модификатор для игнорирования склонов
            var slope = (Slope)ScriptableObject.CreateInstance("Slope");
            slope.enabled = true;
            slope.minMax = new Vector2(0f, 0f);
            slope.blendMode = Modifier.BlendMode.Multiply;
            // slope.min = slope.max = 0f;
            slope.minFalloff = 1f;
            slope.maxFalloff = 8f;
            newLayerSettings.modifierStack.Add(slope);

            terrainPainter.layerSettings.Insert(0, newLayerSettings);
            layersAddedForCurrentChunk++;
        }
    }

    private void RemoveLayerSettings() {
        for (; layersAddedForCurrentChunk > 0; layersAddedForCurrentChunk--) {
            terrainPainter.layerSettings.RemoveAt(0);
        }
    }
}
