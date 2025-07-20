using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Этапы генерации представляются классами, реализующими интерфейс IGenerationStage.
/// Перед игрой экземпляры этих классов регистрируются в WorldGenerator и
/// позже каждый из них осуществляет определенный этап генерации (создание рельефа,
/// рек, текстурирование, и т. д.). Каждый такой класс имеет метод ProcessChunkAsync,
/// который принимает ChunkData с предыдущего этапа, производит генерацию и передает
/// измененный ChunkData далее. Такой подход позволяет писать этапы отдельно
/// (например, разными участниками проекта),
/// а потом комбинировать их в алгоритм генерации
/// </summary>
public interface IGenerationStage
{
    void Initialize(WorldGenerationData worldGenerationData);
    Task<ChunkData> ProcessChunkAsync(ChunkData chunkData);
    string StageName { get; }
    bool IncludeInGeneration { get; }
}
