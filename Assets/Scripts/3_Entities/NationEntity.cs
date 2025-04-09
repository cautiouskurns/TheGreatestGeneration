/// CLASS PURPOSE:
/// NationEntity represents a runtime instance of a nation composed of multiple regions,
/// maintaining aggregated statistics such as wealth, production, and resource flows.
///
/// CORE RESPONSIBILITIES:
/// - Manage the collection of regions assigned to the nation
/// - Aggregate and update national economic indicators from regional data
/// - Expose runtime statistics such as total wealth, production, and resource balance
///
/// KEY COLLABORATORS:
/// - RegionEntity: Provides economic data and resource rates for aggregation
/// - EventBus: Receives "NationUpdated" trigger after each data refresh
/// - UI or Debug Systems: Access GetNationSummary() for displaying nation status
///
/// CURRENT ARCHITECTURE NOTES:
/// - Aggregated dictionaries are copied on access to preserve encapsulation
/// - Assumes all regional data is valid and well-formed at time of aggregation
///
/// REFACTORING SUGGESTIONS:
/// - Introduce lazy evaluation or dirty flags to reduce redundant recalculation
/// - Move aggregation logic into a service or strategy pattern for testability
///
/// EXTENSION OPPORTUNITIES:
/// - Add political systems, happiness metrics, or national traits
/// - Track historical trends in economic data across turns
/// - Integrate with diplomacy or AI systems for behavior modeling

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a nation in the game with all its regions and aggregated properties
/// </summary>
public class NationEntity
{
    // Basic properties
    public string nationName;
    public Color nationColor;
    public List<RegionEntity> regions = new List<RegionEntity>();
    
    // Aggregated statistics
    private int totalWealth;
    private int totalProduction;
    private Dictionary<string, float> aggregatedResources = new Dictionary<string, float>();
    private Dictionary<string, float> aggregatedProduction = new Dictionary<string, float>();
    private Dictionary<string, float> aggregatedConsumption = new Dictionary<string, float>();
    
    // Constructor
    public NationEntity(string name, Color color)
    {
        nationName = name;
        nationColor = color;
    }
    
    // Add a region to this nation
    public void AddRegion(RegionEntity region)
    {
        if (region != null && !regions.Contains(region))
        {
            regions.Add(region);
            // Update aggregated statistics
            UpdateAggregatedData();
        }
    }
    
    // Remove a region from this nation
    public void RemoveRegion(RegionEntity region)
    {
        if (region != null && regions.Contains(region))
        {
            regions.Remove(region);
            // Update aggregated statistics
            UpdateAggregatedData();
        }
    }
    
    // Update all aggregated statistics
    public void UpdateAggregatedData()
    {
        // Reset aggregated values
        totalWealth = 0;
        totalProduction = 0;
        aggregatedResources.Clear();
        aggregatedProduction.Clear();
        aggregatedConsumption.Clear();
        
        // Aggregate data from all regions
        foreach (var region in regions)
        {
            // Sum basic statistics
            totalWealth += region.wealth;
            totalProduction += region.production;
            
            // Aggregate resources
            if (region.resources != null)
            {
                // Aggregate resource quantities
                var resources = region.resources.GetAllResources();
                foreach (var entry in resources)
                {
                    string resourceName = entry.Key;
                    float amount = entry.Value;
                    
                    if (!aggregatedResources.ContainsKey(resourceName))
                        aggregatedResources[resourceName] = 0;
                    
                    aggregatedResources[resourceName] += amount;
                }
                
                // Aggregate production rates
                var production = region.resources.GetAllProductionRates();
                foreach (var entry in production)
                {
                    string resourceName = entry.Key;
                    float rate = entry.Value;
                    
                    if (!aggregatedProduction.ContainsKey(resourceName))
                        aggregatedProduction[resourceName] = 0;
                    
                    aggregatedProduction[resourceName] += rate;
                }
                
                // Aggregate consumption rates
                var consumption = region.resources.GetAllConsumptionRates();
                foreach (var entry in consumption)
                {
                    string resourceName = entry.Key;
                    float rate = entry.Value;
                    
                    if (!aggregatedConsumption.ContainsKey(resourceName))
                        aggregatedConsumption[resourceName] = 0;
                    
                    aggregatedConsumption[resourceName] += rate;
                }
            }
        }
        
        // Trigger event to notify about nation data update
        EventBus.Trigger("NationUpdated", this);
    }
    
    // Get total wealth
    public int GetTotalWealth()
    {
        return totalWealth;
    }
    
    // Get total production
    public int GetTotalProduction()
    {
        return totalProduction;
    }
    
    // Get aggregated resources
    public Dictionary<string, float> GetAggregatedResources()
    {
        return new Dictionary<string, float>(aggregatedResources);
    }
    
    // Get aggregated production rates
    public Dictionary<string, float> GetAggregatedProduction()
    {
        return new Dictionary<string, float>(aggregatedProduction);
    }
    
    // Get aggregated consumption rates
    public Dictionary<string, float> GetAggregatedConsumption()
    {
        return new Dictionary<string, float>(aggregatedConsumption);
    }
    
    // Get resource balance (production - consumption)
    public Dictionary<string, float> GetResourceBalance()
    {
        Dictionary<string, float> balance = new Dictionary<string, float>();
        
        // Combine all resource names from both production and consumption
        HashSet<string> resourceNames = new HashSet<string>();
        foreach (var key in aggregatedProduction.Keys) resourceNames.Add(key);
        foreach (var key in aggregatedConsumption.Keys) resourceNames.Add(key);
        
        // Calculate balance for each resource
        foreach (var resourceName in resourceNames)
        {
            float production = aggregatedProduction.ContainsKey(resourceName) ? 
                aggregatedProduction[resourceName] : 0;
                
            float consumption = aggregatedConsumption.ContainsKey(resourceName) ? 
                aggregatedConsumption[resourceName] : 0;
                
            balance[resourceName] = production - consumption;
        }
        
        return balance;
    }
    
    // Get information about this nation
    public string GetNationSummary()
    {
        string summary = $"Nation: {nationName}\n";
        summary += $"Regions: {regions.Count}\n";
        summary += $"Total Wealth: {totalWealth}\n";
        summary += $"Total Production: {totalProduction}\n";
        
        // Add resource balance information
        summary += "\nResource Balance:\n";
        var balance = GetResourceBalance();
        foreach (var entry in balance)
        {
            string resourceName = entry.Key;
            float netChange = entry.Value;
            string indicator = netChange >= 0 ? "+" : "";
            
            summary += $"  {resourceName}: {indicator}{netChange:F1}/turn\n";
        }
        
        return summary;
    }
}
