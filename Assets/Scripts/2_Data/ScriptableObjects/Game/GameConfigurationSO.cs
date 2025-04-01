using UnityEngine;

[CreateAssetMenu(fileName = "GameConfiguration", menuName = "Game Data/Game Configuration")]
public class GameConfigurationSO : ScriptableObject
{
    [Header("Map Generation")]
    public bool useProceduralMap = true;
    public MapDataSO predefinedMapData;
    
    [Header("Terrain Generation")]
    public bool useSavedTerrainMap = true;
    public TerrainMapDataSO savedTerrainMap;
    
    [Header("Procedural Map Settings")]
    public int mapWidth = 10;
    public int mapHeight = 10;
    public int nationCount = 3;
    public int regionsPerNation = 5;
    public bool useRandomSeed = true;
    public int mapSeed = 12345;
    public float elevationScale = 30f;
    public float moistureScale = 50f;
    
    [Header("Game Settings")]
    [Range(0.5f, 3.0f)]
    public float gameSpeed = 1.0f;
    public DifficultyLevel difficultyLevel = DifficultyLevel.Normal;
    
    [Header("Trade System")]
    public TradeSystemConfig tradeSystemConfig;
    
    [Header("Economic Settings")]
    public EconomicModifiers economicModifiers;
}

[System.Serializable]
public class TradeSystemConfig
{
    public int baseTradeRadius = 5;
    public float tradeCostPerDistance = 0.1f;
    public int maxTradeVolume = 100;
}

[System.Serializable]
public class EconomicModifiers
{
    [Range(0.5f, 2.0f)]
    public float globalProductionModifier = 1.0f;
    [Range(0.5f, 2.0f)]
    public float globalWealthModifier = 1.0f;
    [Range(0.5f, 2.0f)]
    public float resourceGrowthRate = 1.0f;
}

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard
}