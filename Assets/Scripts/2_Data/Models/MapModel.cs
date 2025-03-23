using System.Collections.Generic;
using UnityEngine;

public class MapModel
{
    private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();
    private RegionEntity selectedRegion;
    private MapDataSO mapData;
    private Dictionary<string, TerrainTypeDataSO> terrainTypes; // Add this

    // Update constructor to accept terrain types
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
    
    // Add a method to get the MapDataSO for the GameManager
    public MapDataSO GetMapData()
    {
        return mapData;
    }
}