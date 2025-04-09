using UnityEngine;
using System.Collections.Generic;
using V1.Data;
using V1.Entities;
using V1.Systems;
using V1.Core;

namespace V1.Utils
{ 
    /// CLASS PURPOSE:
    /// GameSystemInitializer is responsible for setting up all core game systems and models
    /// required at the start of a new game session. It ensures that necessary components
    /// like the map model, nation model, and trade system are initialized and interconnected.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Validate and process the provided map and terrain data
    /// - Instantiate and connect MapModel and NationModel
    /// - Register regions to their respective nations
    /// - Ensure the TradeSystem and GameStateManager exist in the scene
    /// - Return a configuration object with all initialized systems
    /// 
    /// KEY COLLABORATORS:
    /// - MapDataSO: Source of map and region layout data
    /// - TerrainTypeDataSO: Provides terrain type definitions for simulation logic
    /// - NationModel & MapModel: Core data models for regional and national structures
    /// - TradeSystem: Simulates inter-regional resource exchange
    /// - GameStateManager: Tracks game phase, turn state, and simulation lifecycle
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Logic is split into small, private helper methods for clarity
    /// - Uses runtime instantiation for any missing MonoBehaviours
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Introduce interface-based abstraction for GameSystem creation
    /// - Consider dependency injection for testability and modularity
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Add initialization hooks for additional systems (e.g., population, events)
    /// - Support different game modes or configuration profiles
    /// - Track and log initialization progress for debugging or loading screens
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
}