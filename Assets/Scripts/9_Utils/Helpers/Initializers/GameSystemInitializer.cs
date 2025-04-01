using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Responsible for initializing core game systems and models
/// Follows the separation of concerns principle
/// </summary>
public class GameSystemInitializer
{
    /// <summary>
    /// Initializes core game models and systems
    /// </summary>
    /// <param name="mapData">Procedurally or manually generated map data</param>
    /// <param name="terrainTypes">Dictionary of available terrain types</param>
    /// <returns>Initialized game systems configuration</returns>
    public GameSystemsConfig InitializeGameSystems(
        MapDataSO mapData, 
        Dictionary<string, TerrainTypeDataSO> terrainTypes,
        NationTemplate[] nationTemplates = null)
    {
        // Validate inputs
        if (mapData == null)
        {
            Debug.LogError("Cannot initialize game systems: MapData is null");
            return null;
        }

        // Create nation model
        NationModel nationModel = new NationModel(mapData);

        // Create map model with terrain types
        MapModel mapModel = new MapModel(mapData, terrainTypes);

        // Register regions with nations
        RegisterRegionsWithNations(mapModel, nationModel);

        // Initialize trade system
        TradeSystem tradeSystem = InitializeTradeSystem(mapModel);

        // Create game state manager if it doesn't exist
        EnsureGameStateManager();

        return new GameSystemsConfig
        {
            MapModel = mapModel,
            NationModel = nationModel,
            TradeSystem = tradeSystem
        };
    }

    /// <summary>
    /// Register all regions from the map model with their respective nations
    /// </summary>
    private void RegisterRegionsWithNations(MapModel mapModel, NationModel nationModel)
    {
        foreach (var regionEntry in mapModel.GetAllRegions())
        {
            RegionEntity region = regionEntry.Value;
            nationModel.RegisterRegion(region);
        }
    }

    /// <summary>
    /// Initialize or find an existing trade system
    /// </summary>
    private TradeSystem InitializeTradeSystem(MapModel mapModel)
    {
        TradeSystem tradeSystem = Object.FindFirstObjectByType<TradeSystem>();
        
        if (tradeSystem == null)
        {
            GameObject tradeSystemObj = new GameObject("TradeSystem");
            tradeSystem = tradeSystemObj.AddComponent<TradeSystem>();
        }

        return tradeSystem;
    }

    /// <summary>
    /// Ensure a GameStateManager exists in the scene
    /// </summary>
    private void EnsureGameStateManager()
    {
        if (GameStateManager.Instance == null)
        {
            GameObject gameStateObj = new GameObject("GameStateManager");
            gameStateObj.AddComponent<GameStateManager>();
            Debug.Log("Created GameStateManager");
        }
    }
}

/// <summary>
/// Configuration container for initialized game systems
/// </summary>
public class GameSystemsConfig
{
    public MapModel MapModel { get; set; }
    public NationModel NationModel { get; set; }
    public TradeSystem TradeSystem { get; set; }
}