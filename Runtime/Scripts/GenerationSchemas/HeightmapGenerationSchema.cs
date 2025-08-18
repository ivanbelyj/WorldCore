using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Heightmap Generation Schema",
    menuName = "Procedural World/Heightmap Generation Schema", order = 51)]
[Serializable]
public class HeightmapGenerationSchema : ScriptableObject
{
    public NoiseData noiseData;
}
