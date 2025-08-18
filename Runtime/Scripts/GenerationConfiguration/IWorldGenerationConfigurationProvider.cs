using System.Collections.Generic;
using UnityEngine;

public interface IWorldGenerationConfigurationProvider
{
    public List<HeightmapRegionGenerationSchema> GetRegionSchemas();
}
