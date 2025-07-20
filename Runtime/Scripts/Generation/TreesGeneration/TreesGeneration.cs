using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class TreesGeneration : GenerationStage
{
    [Tooltip("Значение, изменяющее сетку прохода по точкам чанка для расстановки деревьев. "
    + "1, чтобы проход осуществлялся по каждой точке чанка (ресурсозатратно)")]
    [SerializeField]
    private float treeDensityModifier = 0.2f;

    [SerializeField]
    private BiomesManager biomesManager;

    protected async override Task<ChunkData> ProcessChunkImplAsync(ChunkData chunkData)
    {
        await CreateTrees(chunkData);
        
        return chunkData;
    }

    private async Task CreateTrees(ChunkData chunkData) {
        TerrainData terrainData = chunkData.TerrainData;
        var prototypesAndInstances = await Task.Run(() =>
            CreateTreePrototypesAndInstances(worldData, chunkData));
        terrainData.treePrototypes = prototypesAndInstances.Item1.ToArray();
        terrainData.SetTreeInstances(prototypesAndInstances.Item2.ToArray(), true);
    }

    private Dictionary<Biome, float> sumOfTreePrevalenceByBiome = new Dictionary<Biome, float>();

    private Tree SelectTree(Biome biome, float moisture, float radiation) {
        // Без учета prevalence видов деревьев:
        // int len = biome.Trees.Length;
        // int index = Mathf.FloorToInt(
        //     len * (float)randomForCurrentChunk.NextDouble());
        // return (len == 0 ? null : biome.Trees[index].Tree, index);
        
        BiomeTree[] trees = biome.Trees;

        if (!sumOfTreePrevalenceByBiome.ContainsKey(biome)) {
            sumOfTreePrevalenceByBiome[biome] = trees.Sum(x => x.Prevalence);
        }
        float totalPrevalence = sumOfTreePrevalenceByBiome[biome];
        float randomNum = randomForCurrentChunk.Range(0, totalPrevalence);

        foreach (BiomeTree tree in trees) {
            randomNum -= tree.Prevalence;
            if (randomNum <= 0) {
                return tree.Tree;
            }
        }

        return null;
    }
    
    /// <param name="position">Локальная позиция дерева на Terrain, в диапазоне [0, 1]</param>
    private TreeInstance CreateTreeInstance(int prototypeIndex, Vector3 position,
        float scale, float minSize, float maxSize) {
        Debug.Log("PrototypeIndex arg: " + prototypeIndex);
        TreeInstance treeInstance = new TreeInstance();

        treeInstance.prototypeIndex = prototypeIndex;

        treeInstance.position = position;
        treeInstance.rotation = randomForCurrentChunk.Range(0, 2 * Mathf.PI);

        float rndScale = scale * randomForCurrentChunk.Range(minSize, maxSize);

        treeInstance.widthScale = rndScale;
        treeInstance.heightScale = rndScale;

        treeInstance.color = Color.white;
        treeInstance.lightmapColor = Color.white;

        Debug.Log("ProtIndex in res: " + treeInstance.prototypeIndex);
        return treeInstance;
    }

    /// <summary>
    /// Вероятность возникновения дерева в точке заданного биома с заданной
    /// влажностью
    /// </summary>    
    private float TreeProbability(Biome biome, float moisture) {
        return Mathf.Clamp01(biome.TreesDensity * moisture);
        // Todo: разные виды деревьев и лесов по-разному воспринимают радиацию.
        // Кроме того, радиация может не только снижать количество деревьев, но,
        // например, влиять на распределение видов
    }

    private (List<TreePrototype>, List<TreeInstance>) CreateTreePrototypesAndInstances(
        WorldGenerationData worldData, ChunkData chunkData) {
        var instances = new List<TreeInstance>();
        var prototypes = new List<TreePrototype>();

        float chunkSize = worldData.ChunkResolution;

        // Проход осуществляется не по каждой точке чанка,
        // а по сетке со скорректированным масштабом

        // treeDensityModifier обычно принимает значения меньше 1 для укрупнения сетки
        // и уменьшения количества проходов, а WorldScale также корректирует сетку,
        // чтобы деревья были рассажены правильно даже с учетом масштабирования
        float gridModifier = treeDensityModifier * worldData.WorldScale;

        // Половина ячейки сетки
        float treeCellHalfSize = 1 / gridModifier / 2;

        // Деревья, которые были посажены в чанке и уже добавлены в результирующий
        // список прототипов, а также их будущие индексы прототипа (их много, т.к. дерево
        // может быть представлено множеством моделей)
        Dictionary<Tree, int[]> treesInChunkAndProtIndexes
            = new Dictionary<Tree, int[]>();

        for (float x = 0; x < chunkSize; x += 1 / gridModifier)
        {
            for (float z = 0; z < chunkSize; z += 1 / gridModifier)
            {
                int biomeZ = Mathf.FloorToInt(z);
                int biomeX = Mathf.FloorToInt(x);
                Biome biome = biomesManager.GetBiomeById(chunkData.BiomeIds[biomeZ, biomeX]);

                float moisture = chunkData.Moisture[biomeZ, biomeX];
                float radiation = chunkData.Radiation[biomeZ, biomeX];
                
                float treeProbability = TreeProbability(biome, moisture);

                float fixingValue = (float)randomForCurrentChunk.NextDouble();
                if (fixingValue < treeProbability)
                {
                    // === Вычисление позиции дерева ===
                    // Изначальная позиция размещения дерева - середина ячейки
                    Vector3 gridTreePos = new Vector3(x + treeCellHalfSize, 0,
                        z + treeCellHalfSize);

                    // Сетка обхода при сильной модификации слишком заметна,
                    // поэтому каждое дерево смещается (максимум на размер одной ячейки)
                    Vector3 offset = new Vector3((float)randomForCurrentChunk.NextDouble(),
                        0, (float)randomForCurrentChunk.NextDouble()).normalized
                        / gridModifier;

                    // === Выбор дерева === 
                    Tree tree = SelectTree(biome, moisture, radiation);

                    // Но выбрать дерево недостаточно, нужно добавить прототип (если его еще не было)

                    // === Создание прототипов для дерева (если не созданы) ===
                    if (tree != null && !treesInChunkAndProtIndexes.ContainsKey(tree)) {
                        int[] treeVariantsIndexes = new int[tree.TreePrefabs.Length];
                        
                        // Все варианты моделей дерева добавляются как прототипы
                        for (int i = 0; i < tree.TreePrefabs.Length; i++) {
                            // Добавление результата
                            prototypes.Add(new TreePrototype() {
                                prefab = tree.TreePrefabs[i]
                            });
                            int protIndex = prototypes.Count - 1;
                            // Индекс только что добавленного
                            treeVariantsIndexes[i] = protIndex;
                        }
                        // По Tree доступен список индексов прототипов его вариантов
                        treesInChunkAndProtIndexes.Add(tree, treeVariantsIndexes);
                    }

                    // === Создание TreeInstance ===
                    if (tree != null) {
                        // Выбор случайного варианта дерева
                        int[] variantsProtIndexes = treesInChunkAndProtIndexes[tree];
                        int rndProtIndex = variantsProtIndexes[randomForCurrentChunk
                            .Next(variantsProtIndexes.Length)];

                        // В terrain используются позиции в диапазоне [0, 1]
                        Vector3 treePosInTerrain = (gridTreePos + offset) / chunkSize;
                        var treeInstance = CreateTreeInstance(rndProtIndex, treePosInTerrain,
                            tree.ScaleMultiplier / worldData.WorldScale,
                            minSize: tree.MinSize, maxSize: tree.MaxSize);
                        instances.Add(treeInstance);
                    }
                }
                // Debug.Log($"Foliage. x: {x}, z: {z}");
            }
        }

        return (prototypes, instances);
    }
}
