using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Данные о виде деревьев в рамках биома
/// </summary>
[System.Serializable]
public class BiomeTree
{
    [SerializeField]
    private Tree tree;
    /// <summary>
    /// Вид дерева
    /// </summary>
    public Tree Tree { get => tree; set => tree = value; }

    [SerializeField]
    private float prevalence = 1;
    /// <summary>
    /// Распространенность дерева, учитывающаяся при выборе дерева для расстановки.
    /// Допустимы любые положительные значения,
    /// чем больше, тем больше вероятность выбора данного дерева
    /// </summary>
    public float Prevalence { get => prevalence; set => prevalence = value; }
}
