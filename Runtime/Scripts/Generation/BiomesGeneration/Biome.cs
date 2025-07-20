using System.Collections;
using System.Collections.Generic;
using sc.terrain.proceduralpainter;
using UnityEngine;

/// <summary>
/// Данные о биоме
/// </summary>
[CreateAssetMenu(fileName = "New Biome", menuName = "Procedural World/Biome", order = 51)]
public class Biome : ScriptableObject
{
    [SerializeField]
    private byte biomeId;
    public byte BiomeId { get => biomeId; set => biomeId = value; }

    [SerializeField]
    private Color groupColor;
    /// <summary>
    /// Цвет, которым на классифицирующей схеме обозначается группа,
    /// к которой относится данный биом
    /// </summary>
    public Color GroupColor { get => groupColor; set => groupColor = value; }

    [SerializeField]
    private float radiationMin = 0f;
    public float RadiationMin { get => radiationMin; set => radiationMin = value; }

    [SerializeField]
    private float radiationMax = 1f;
    public float RadiationMax { get => radiationMax; set => radiationMax = value; }

    [SerializeField]
    private float varietyMin = 0f;
    /// <summary>
    /// "Разновидность" - дополнительный параметр для выделения разновидностей
    /// биома. Чем меньше диапазон, тем реже встречается биом
    /// </summary>
    public float VarietyMin { get => varietyMin; set => varietyMin = value; }

    [SerializeField]
    private float varietyMax = 1f;
    /// <summary>
    /// "Разновидность" - дополнительный параметр для выделения разновидностей
    /// биома. Чем меньше диапазон, тем реже встречается биом
    /// </summary>
    public float VarietyMax { get => varietyMax; set => varietyMax = value; }

    [SerializeField]
    private BiomeTree[] trees;
    public BiomeTree[] Trees { get => trees; set => trees = value; }

    [SerializeField]
    private float treesDensity = 0.5f;
    /// <summary>
    /// Плотность рассадки деревьев. Значение от 0 до 1 включительно
    /// </summary>
    public float TreesDensity { get => treesDensity; set => treesDensity = value; }

    [SerializeField]
    private LayerSettings[] layerSettings;

    /// <summary>
    /// Слои текстурирования ландшафта, специфичные для данного биома
    /// </summary>
    public LayerSettings[] LayerSettings { get => layerSettings; set => layerSettings = value; }

    [SerializeField]
    private BiomeDetail[] biomeDetails;
    /// <summary>
    /// Детали ландшафта, такие как трава, камни, и т.п.
    /// </summary>
    public BiomeDetail[] BiomeDetails => biomeDetails;
}
