using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all nations in the game world
/// </summary>
public class NationModel
{
    private Dictionary<string, NationEntity> nations = new Dictionary<string, NationEntity>();
    private NationEntity selectedNation;
    
    // Initialize the nation model from map data
    public NationModel(MapDataSO mapData)
    {
        InitializeNations(mapData);
    }
    
    // Create nation entities from MapDataSO
    private void InitializeNations(MapDataSO mapData)
    {
        if (mapData == null || mapData.nations == null)
        {
            Debug.LogError("Invalid map data for nation initialization");
            return;
        }
        
        // Create nations from map data
        foreach (var nationData in mapData.nations)
        {
            NationEntity nation = new NationEntity(nationData.nationName, nationData.nationColor);
            nations.Add(nationData.nationName, nation);
        }
        
        Debug.Log($"NationModel: Initialized {nations.Count} nations");
    }
    
    // Add a region to its nation
    public void RegisterRegion(RegionEntity region)
    {
        if (region == null) return;
        
        string nationName = region.ownerNationName;
        if (string.IsNullOrEmpty(nationName)) return;
        
        if (nations.ContainsKey(nationName))
        {
            nations[nationName].AddRegion(region);
            Debug.Log($"Region {region.regionName} registered to nation {nationName}");
        }
        else
        {
            Debug.LogWarning($"Nation {nationName} not found for region {region.regionName}");
        }
    }
    
    // Get a nation by name
    public NationEntity GetNation(string nationName)
    {
        if (nations.ContainsKey(nationName))
        {
            return nations[nationName];
        }
        return null;
    }
    
    // Get all nations
    public Dictionary<string, NationEntity> GetAllNations()
    {
        return new Dictionary<string, NationEntity>(nations);
    }
    
    // Select a nation
    public void SelectNation(string nationName)
    {
        if (nations.ContainsKey(nationName))
        {
            selectedNation = nations[nationName];
            EventBus.Trigger("NationSelected", selectedNation);
        }
    }
    
    // Get currently selected nation
    public NationEntity GetSelectedNation()
    {
        return selectedNation;
    }
    
    // Update all nations' aggregated data
    public void UpdateAllNations()
    {
        foreach (var nation in nations.Values)
        {
            nation.UpdateAggregatedData();
        }
    }
    
    // Process turn for all nations
    public void ProcessTurn()
    {
        // This would be called after all regions have been processed
        // to update nation-level statistics
        UpdateAllNations();
        
        // Trigger event for UI updates
        EventBus.Trigger("NationModelUpdated", this);
    }
    
    // Get resource balance across all nations (for global market)
    public Dictionary<string, float> GetGlobalResourceBalance()
    {
        Dictionary<string, float> globalBalance = new Dictionary<string, float>();
        
        foreach (var nation in nations.Values)
        {
            var nationBalance = nation.GetResourceBalance();
            
            foreach (var entry in nationBalance)
            {
                string resourceName = entry.Key;
                float balance = entry.Value;
                
                if (!globalBalance.ContainsKey(resourceName))
                    globalBalance[resourceName] = 0;
                
                globalBalance[resourceName] += balance;
            }
        }
        
        return globalBalance;
    }
}
