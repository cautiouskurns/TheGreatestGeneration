using System.Collections.Generic;
using UnityEngine;

public class MapModel
{
    private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();
    private RegionEntity selectedRegion;
    private MapDataSO mapData;

    public MapModel(MapDataSO mapData)
    {
        this.mapData = mapData;
        InitializeRegions();
    }

    private void InitializeRegions()
    {
        foreach (var nation in mapData.nations)
        {
            foreach (var regionData in nation.regions)
            {
                RegionEntity region = new RegionEntity(
                    regionData.regionName,
                    regionData.initialWealth,
                    regionData.initialProduction,
                    nation.nationName,
                    nation.nationColor
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
}