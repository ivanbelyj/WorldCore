using System.Collections.Generic;
using UnityEngine;

public class WorldGenerationConfigurationProvider : MonoBehaviour, IWorldGenerationConfigurationProvider
{
    public List<HeightmapRegionGenerationSchema> regionSchemas;

    public List<HeightmapRegionGenerationSchema> GetRegionSchemas()
    {
        return regionSchemas;
    }
}
