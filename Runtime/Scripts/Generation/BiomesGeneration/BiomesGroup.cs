using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Группа биомов - набор биомов, которые совпадают в плане
/// классификации по температуре и влажности, но различающиеся
/// каким-то другим параметрам (например, радиации)
/// </summary>
public class BiomesGroup
{
    public List<Biome> Biomes { get; private set; }

    public BiomesGroup() {
        Biomes = new List<Biome>();
    }

    public BiomesGroup(List<Biome> biomes) {
        Biomes = biomes;
    }

    public void Add(Biome biome) {
        Biomes.Add(biome);
    }

    /// <summary>
    /// Получает новую группу биомов, для которых данное значение радиации входит в диапазон
    /// </summary>
    public BiomesGroup WithRadiation(float radiation) {
        return new BiomesGroup(Biomes.Where(biome =>
            biome.RadiationMin <= radiation && radiation <= biome.RadiationMax)
            .ToList());
    }

    /// <summary>
    /// Получает новую группу биомов указанной разновидности (значение данной разновидности
    /// входит в диапазон биомов)
    /// </summary>
    public BiomesGroup OfVariety(float variety) {
        return new BiomesGroup(Biomes.Where(biome =>
            biome.VarietyMin <= variety && variety <= biome.VarietyMax)
            .ToList());
    }

    public Biome GetOne() {
        return Biomes.First();
    }
}
