using UnityEngine;

[CreateAssetMenu(fileName = "TerrainMap", menuName = "Game/Terrain Map Data")]
public class TerrainMapDataSO : ScriptableObject
{
    public int width;
    public int height;
    public int seed;
    public float elevationScale;
    public float moistureScale;
    
    // Add this property to match what's being used in MapDebugVisualizer
    public string[,] terrainData;
}