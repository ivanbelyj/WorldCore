using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class BiomesGeneration : GenerationStage
{
    private const int moistureSeedC = 1233;
    private const int temperatureSeedC = 7733;
    private const int radiationSeedC = 131377;
    private const int varietySeedC = 881231;

    /// <summary>
    /// Значение, на которое опускается температура на максимальной высоте
    /// (не учитывая изначальную карту температур без высот)
    /// </summary>
    private const float temperatureDecreasePerHeight = 0.4f;

    /// <summary>
    /// Коэффициент, определяющий то, насколько радиация будет рассеиваться в сухих биомах.
    /// 1, чтобы радиация развеивалась прямо пропорционально сухости
    /// (максимальная сухость развеивает максимальную радиацию)
    /// </summary>
    private const float radiationDissipationByDryness = 0.5f;

    [SerializeField]
    private BiomesManager biomesManager;

    [SerializeField]
    private NoiseData moistureNoise;

    [SerializeField]
    private NoiseData temperatureNoise;

    [SerializeField]
    private NoiseData radiationNoise;

    [SerializeField]
    private NoiseData varietyNoise;

    public override void Initialize(WorldGenerationData worldGenerationData)
    {
        base.Initialize(worldGenerationData);
        biomesManager.Initialize();
    }

    protected async override Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        // chunkData = await base.ProcessChunk(chunkData);
        int chunkRes = worldData.ChunkResolution;

        var noiseOffset = new Vector2(chunkData.ChunkPosition.X * worldData.ChunkSize,
            chunkData.ChunkPosition.Z * worldData.ChunkSize);

        float[,] moisture = chunkData.Moisture = await Task.Run(() =>
            NoiseMapUtils.GenerateNoiseMap(moistureNoise,
                worldData.Seed * moistureSeedC,
                chunkRes, chunkRes, noiseOffset,
                worldData.WorldScale));

        // Temperature
        float[,] temperatureRandom = await Task.Run(() => NoiseMapUtils.GenerateNoiseMap(
            temperatureNoise, worldData.Seed * temperatureSeedC,
            chunkRes, chunkRes, noiseOffset,
            worldData.WorldScale));
        float[,] temperatureOnHeights = chunkData.Temperature = TemperatureOnHeights(
            temperatureRandom, chunkData.TerrainData.GetHeights(0, 0, chunkRes, chunkRes));

        // Radiation
        float[,] radiationNotDissipated = await Task.Run(() => NoiseMapUtils.GenerateNoiseMap(
            radiationNoise, worldData.Seed * radiationSeedC,
            chunkRes, chunkRes, noiseOffset, worldData.WorldScale));
        float[,] radiation = chunkData.Radiation
            = await Task.Run(() => RadiationDissipatedByDryness(moisture, radiationNotDissipated));

        float[,] variety = chunkData.Variety = await Task.Run(() =>
            NoiseMapUtils.GenerateNoiseMap(varietyNoise,
                worldData.Seed * varietySeedC,
                chunkRes, chunkRes, noiseOffset,
                worldData.WorldScale));

        // id биомов, которые встретились в чанке. Необходимо на других этапах
        chunkData.ChunkBiomes = new HashSet<uint>();
        
        // id биомов, расположенных в соответствии с позициями чанка
        chunkData.BiomeIds = await Task.Run(() => {
            uint[,] biomes = new uint[chunkRes, chunkRes];
            for (int y = 0; y < chunkRes; y++) {
                for (int x = 0; x < chunkRes; x++) {
                    biomes[y, x] = biomesManager.GetBiomeId(moisture[y, x], temperatureOnHeights[y, x],
                        radiation[y, x], variety[y, x]);
                    if (!chunkData.ChunkBiomes.Contains(biomes[y, x])) {
                        chunkData.ChunkBiomes.Add(biomes[y, x]);
                    }
                }
            }
            return biomes;
        });

        return chunkData;
    }

    /// <summary>
    /// Карта температур, учитывающая также высоты
    /// </summary>
    private float[,] TemperatureOnHeights(float[,] temperature, float[,] heights) {
        int width = temperature.GetLength(1);
        int height = temperature.GetLength(0);
        float[,] res = new float[height, width];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                res[y, x] = temperature[y, x] - heights[y, x] * temperatureDecreasePerHeight;
                if (res[y, x] < 0)
                    res[y, x] = 0;
            }
        }
        return res;
    }

    /// <summary>
    /// Карта радиации, учитывающая, что радиация рассеивается в более сухих биомах
    /// </summary>
    private float[,] RadiationDissipatedByDryness(float[,] moisture, float[,] radiation) {
        int width = moisture.GetLength(1);
        int height = moisture.GetLength(0);
        float[,] res = new float[height, width];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                float dryness = 1 - moisture[y, x];
                res[y, x] = radiation[y, x] - radiationDissipationByDryness * dryness;
                if (res[y, x] < 0)
                    res[y, x] = 0;
            }
        }
        return res;
    }
}
