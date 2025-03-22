using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Represents the data structure for the game map, including regions, their properties, and relationships.
/// This class serves as the "Model" in the MVC architecture.
/// </summary>
public class MapModel: MonoBehaviour
{
    // Dictionary to store all region entities by name for quick lookup
    private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();
    
    // Reference to the source data
    private MapDataSO mapData;
    
    // The currently selected region
    private RegionEntity selectedRegion;
    
    public MapModel(MapDataSO mapData)
    {
        this.mapData = mapData;
        InitializeRegions();
    }
    
    private void InitializeRegions()
    {
        // Create RegionEntity instances based on the ScriptableObject data
        foreach (NationDataSO nation in mapData.nations)
        {
            foreach (RegionDataSO regionData in nation.regions)
            {
                RegionEntity region = new RegionEntity(
                    regionData.regionName,
                    regionData.initialWealth,
                    regionData.initialProduction
                );
                
                regions.Add(region.regionName, region);
                
                // Notify other systems that a new region has been created
                EventBus.Trigger("RegionCreated", region);
            }
        }
        
        // Notify that all regions are initialized and ready
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
        if (regions.ContainsKey(regionName))
        {
            selectedRegion = regions[regionName];
            EventBus.Trigger("RegionSelected", selectedRegion);
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
}