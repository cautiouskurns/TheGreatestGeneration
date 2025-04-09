/// CLASS PURPOSE:
/// MapModel handles the construction and in-memory representation of all regional game data.
/// It provides access to individual regions, facilitates economic updates per turn,
/// and enables region selection and modification throughout gameplay.
/// 
/// CORE RESPONSIBILITIES:
/// - Construct all RegionEntity instances from MapDataSO
/// - Provide region access and selection methods
/// - Trigger relevant events (e.g., RegionSelected, RegionUpdated, MapModelTurnProcessed)
/// - Process turn-level economic updates for all regions
/// 
/// KEY COLLABORATORS:
/// - GameManager: Coordinates game-wide flow and holds top-level systems
/// - RegionEntity: Stores per-region economic data and modifiers
/// - EventBus: Handles decoupled event communication across systems
/// - MapDataSO / TerrainTypeDataSO: Supplies static region and terrain configuration
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - Loosely coupled via event triggers
/// - Stores region data in a dictionary for quick access
/// - Selected region is stored as state and shared through events
/// 
/// REFACTORING SUGGESTIONS:
/// - Consider separating region creation logic into a RegionFactory
/// - Move terrainType lookup logic into a helper for readability
/// - Make UpdateRegion more robust to partial updates or additive changes
/// 
/// EXTENSION OPPORTUNITIES:
/// - Add support for project queues or construction states in regions
/// - Include historical data logging for wealth/production changes over turns
/// - Expand ProcessTurn to incorporate economic cycle modifiers

using System.Collections.Generic;
using UnityEngine;

public class MapModel
{
    #region Fields
    private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();
    private RegionEntity selectedRegion;
    private MapDataSO mapData;
    private Dictionary<string, TerrainTypeDataSO> terrainTypes;
    #endregion

    #region Initialization
    // Constructor to accept terrain types
    public MapModel(MapDataSO mapData, Dictionary<string, TerrainTypeDataSO> terrainTypes = null)
    {
        this.mapData = mapData;
        this.terrainTypes = terrainTypes; // Store reference to terrain types
        InitializeRegions();
    }

    private void InitializeRegions()
    {
        foreach (var nation in mapData.nations)
        {
            foreach (var regionData in nation.regions)
            {
                // Get terrain type from dictionary if available
                TerrainTypeDataSO terrain = null;
                if (!string.IsNullOrEmpty(regionData.terrainTypeName) && 
                    terrainTypes != null && 
                    terrainTypes.ContainsKey(regionData.terrainTypeName))
                {
                    terrain = terrainTypes[regionData.terrainTypeName];
                }
                
                // Create region with terrain
                RegionEntity region = new RegionEntity(
                    regionData.regionName,
                    regionData.initialWealth,
                    regionData.initialProduction,
                    nation.nationName,
                    nation.nationColor,
                    terrain  // Pass the terrain to the RegionEntity
                );

                regions.Add(region.regionName, region);
                EventBus.Trigger("RegionCreated", region);
            }
        }

        EventBus.Trigger("RegionEntitiesReady", regions);
    }
    #endregion

    #region Region Access
    public RegionEntity GetRegion(string regionName)
    {
        if (regions.ContainsKey(regionName))
        {
            return regions[regionName];
        }
        return null;
    }

    public Dictionary<string, RegionEntity> GetAllRegions()
    {
        return regions;
    }
    #endregion

    #region Region Selection
    public void SelectRegion(string regionName)
    {
        Debug.Log($"MapModel.SelectRegion called with: {regionName}");
        
        if (regions.ContainsKey(regionName))
        {
            selectedRegion = regions[regionName];
            EventBus.Trigger("RegionSelected", selectedRegion);
            Debug.Log("RegionSelected event triggered");
        }
        else
        {
            Debug.LogWarning($"Region '{regionName}' not found in regions dictionary");
        }
    }

    public RegionEntity GetSelectedRegion()
    {
        return selectedRegion;
    }
    #endregion

    #region Region Updates
    public void UpdateRegion(RegionEntity region)
    {
        if (regions.ContainsKey(region.regionName))
        {
            regions[region.regionName] = region;
            EventBus.Trigger("RegionUpdated", region);
        }
    }

    public void ProcessTurn()
    {
        foreach (var region in regions.Values)
        {
            int wealthChange = region.production * 2;  // Wealth grows based on production
            int productionChange = Random.Range(-2, 3); // Simulate fluctuation
            region.UpdateEconomy(wealthChange, productionChange);
        }

        EventBus.Trigger("MapModelTurnProcessed", null);
    }
    #endregion
    
    #region Data Access
    // Add a method to get the MapDataSO for the GameManager
    public MapDataSO GetMapData()
    {
        return mapData;
    }
    #endregion
}