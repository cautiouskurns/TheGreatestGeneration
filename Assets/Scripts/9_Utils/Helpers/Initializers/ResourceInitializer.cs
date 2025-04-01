using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages the initialization and configuration of resources across regions
/// </summary>
public class ResourceInitializer
{
    /// <summary>
    /// Initialize resources for all regions
    /// </summary>
    /// <param name="regions">Dictionary of regions to initialize</param>
    /// <param name="availableResources">Array of resource definitions</param>
    public void InitializeRegionResources(
        Dictionary<string, RegionEntity> regions, 
        ResourceDataSO[] availableResources)
    {
        // Validate inputs
        if (regions == null || regions.Count == 0)
        {
            Debug.LogWarning("No regions available for resource initialization");
            return;
        }

        if (availableResources == null || availableResources.Length == 0)
        {
            Debug.LogWarning("No resource definitions available");
            return;
        }

        // Initialize resources for each region
        foreach (var regionEntry in regions)
        {
            RegionEntity region = regionEntry.Value;
            
            if (region.resources != null)
            {
                // Load resource definitions
                region.resources.LoadResourceDefinitions(availableResources);
                
                // Activate some default recipes for testing
                if (region.productionComponent != null)
                {
                    ActivateDefaultRecipes(region);
                }
            }
        }

        // Optionally create some initial resource imbalances for testing trade
        CreateInitialResourceImbalances(regions);
    }

    /// <summary>
    /// Activate some default production recipes
    /// </summary>
    private void ActivateDefaultRecipes(RegionEntity region)
    {
        // Example of activating a specific recipe
        region.productionComponent.ActivateRecipe("Basic Iron Smelting");
    }

    /// <summary>
    /// Create some initial resource imbalances to test trade mechanics
    /// </summary>
    private void CreateInitialResourceImbalances(Dictionary<string, RegionEntity> regions)
    {
        // Ensure we have at least two regions to create imbalance
        if (regions.Count < 2)
        {
            Debug.LogWarning("Not enough regions to create resource imbalances");
            return;
        }

        // Convert regions to an array for easier indexing
        var regionArray = new RegionEntity[regions.Count];
        regions.Values.CopyTo(regionArray, 0);
        
        // Give first region excess food
        if (regionArray[0].resources != null)
        {
            regionArray[0].resources.AddResource("Crops", 100);
        }
        
        // Give second region excess iron
        if (regionArray[1].resources != null)
        {
            regionArray[1].resources.AddResource("Iron Ore", 100);
        }
    }
}