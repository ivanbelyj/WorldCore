using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain Detail",
    menuName = "Procedural World/Terrain Detail", order = 51)]
public class TerrainDetail : ScriptableObject
{
    public GameObject[] prototypeVariants;
    public bool usePrototypeMesh;
    public Texture2D[] prototypeTextureVariants;
    
    public float minWidth = 1f;
    public float maxWidth = 2f;
    public float minHeight = 1f;
    public float maxHeight = 2f;
    
    public DetailRenderMode renderMode;
    public bool useInstancing;
}
