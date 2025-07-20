using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Условная редкость детали, регулирующая
/// вероятность ее появления
/// </summary>
public enum DetailRarity
{
    Dominant = 1,
    Ordinary = 10,
    SometimesOccuring = 100,
    Infrequent = 1000,
    Rare = 10_000,
    TheRarest = 100_000
}
