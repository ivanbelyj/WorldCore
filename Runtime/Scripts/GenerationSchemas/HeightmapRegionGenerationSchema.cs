using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Heightmap Region Generation Schema",
    menuName = "Procedural World/Heightmap Region Generation Schema", order = 51)]
[Serializable]
public class HeightmapRegionGenerationSchema : ScriptableObject
{
    public List<HeightmapRegionZoneGenerationSchema> zoneSchemas;
    public Color debugColor = Color.red;
}
