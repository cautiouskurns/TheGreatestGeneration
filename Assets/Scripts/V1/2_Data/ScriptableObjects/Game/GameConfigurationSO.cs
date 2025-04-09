using UnityEngine;

namespace V1.Data
{
    /// CLASS PURPOSE:
    /// GameConfigurationSO defines global settings for a game session, including
    /// map generation parameters, economic scaling, trade system configuration,
    /// difficulty tuning, and game speed. It serves as a centralized configuration
    /// object for initializing and controlling the overall gameplay experience.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Store parameters for procedural or predefined map generation
    /// - Control game pacing and difficulty level
    /// - Provide configuration for trade system behavior
    /// - Define global economic modifiers affecting resource production and growth
    /// 
    /// KEY COLLABORATORS:
    /// - GameManager: Reads these settings during initialization
    /// - MapGenerator: Uses map and terrain generation parameters
    /// - EconomicSystem: Applies global modifiers for simulation tuning
    /// - TradeSystem: Uses trade radius, cost, and volume limits
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Exposes config via ScriptableObject for easy tuning in editor
    /// - Includes modular sub-objects (TradeSystemConfig, EconomicModifiers)
    /// - Difficulty setting is an enum, easily extendable
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Allow loading presets or profiles at runtime
    /// - Break out economic and trade settings into reusable SOs
    /// - Add validation and constraints for interdependent settings
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Introduce game modes or scenario flags
    /// - Add support for player-selected modifiers or mutators
    /// - Support multiplayer/shared config serialization
    /// 
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
}