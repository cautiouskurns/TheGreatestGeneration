// Assets/Scripts/4_Components/ResourceComponent.cs
using System.Collections.Generic;
using UnityEngine;

public class ResourceComponent
{
    // Resource storage - tracks quantities of each resource type
    private Dictionary<string, float> resources = new Dictionary<string, float>();
    
    // Production and consumption rates per turn
    private Dictionary<string, float> productionRates = new Dictionary<string, float>();
    private Dictionary<string, float> consumptionRates = new Dictionary<string, float>();
    
    // Reference to region owning this component
    private RegionEntity ownerRegion;
    
    // Constructor
    public ResourceComponent(RegionEntity owner)
    {
        ownerRegion = owner;
        InitializeBasicResources();
    }
    
    // Initialize with basic resources
    private void InitializeBasicResources()
    {
        // Start with some basic resources
        resources["Food"] = 100;
        resources["Wood"] = 50;
        resources["Stone"] = 25;
        resources["Iron"] = 10;
        
        // Set initial production rates based on terrain
        CalculateBaseProduction();
    }
    
    // Calculate production rates based on terrain
    public void CalculateBaseProduction()
    {
        // Default production values
        productionRates["Food"] = 10;
        productionRates["Wood"] = 5;
        productionRates["Stone"] = 3;
        productionRates["Iron"] = 1;
        
        // Default consumption values
        consumptionRates["Food"] = 5;
        consumptionRates["Wood"] = 2;
        consumptionRates["Stone"] = 1;
        consumptionRates["Iron"] = 0.5f;
        
        // Apply terrain modifiers if available
        if (ownerRegion.terrainType != null)
        {
            // Food production modified by agriculture potential
            float agricultureMod = ownerRegion.terrainType.GetMultiplierForSector("agriculture");
            productionRates["Food"] *= agricultureMod;
            
            // Wood production boosted in forests
            if (ownerRegion.terrainType.terrainName == "Forest")
            {
                productionRates["Wood"] *= 2.0f;
            }
            
            // Stone and iron boosted in mountains
            if (ownerRegion.terrainType.terrainName == "Mountains")
            {
                productionRates["Stone"] *= 2.0f;
                productionRates["Iron"] *= 1.5f;
            }
            
            // Water reduces all production except food
            if (ownerRegion.terrainType.terrainName == "Water")
            {
                productionRates["Food"] *= 0.8f; // Fish
                productionRates["Wood"] *= 0.2f;
                productionRates["Stone"] *= 0.1f;
                productionRates["Iron"] *= 0.1f;
            }
        }
    }
    
    // Fix for ResourceComponent.ProcessTurn method
    public void ProcessTurn()
    {
        // Create a copy of the keys to avoid modifying the collection during iteration
        List<string> resourceKeys = new List<string>(resources.Keys);
        
        // Process production and consumption for each resource
        foreach (string resource in resourceKeys)
        {
            float production = productionRates.ContainsKey(resource) ? productionRates[resource] : 0;
            float consumption = consumptionRates.ContainsKey(resource) ? consumptionRates[resource] : 0;
            
            // Calculate net change
            float netChange = production - consumption;
            
            // Apply change
            resources[resource] += netChange;
            
            // Don't go below zero
            resources[resource] = Mathf.Max(0, resources[resource]);
        }
    }
        
    // Add a resource
    public void AddResource(string resourceName, float amount)
    {
        if (!resources.ContainsKey(resourceName))
        {
            resources[resourceName] = 0;
        }
        
        resources[resourceName] += amount;
    }
    
    // Remove a resource
    public bool RemoveResource(string resourceName, float amount)
    {
        if (!resources.ContainsKey(resourceName) || resources[resourceName] < amount)
        {
            return false; // Not enough resources
        }
        
        resources[resourceName] -= amount;
        return true;
    }
    
    // Get amount of a resource
    public float GetResourceAmount(string resourceName)
    {
        if (!resources.ContainsKey(resourceName))
        {
            return 0;
        }
        
        return resources[resourceName];
    }
    
    // Get all resources for display
    public Dictionary<string, float> GetAllResources()
    {
        return new Dictionary<string, float>(resources);
    }
    
    // Get production rate for a resource
    public float GetProductionRate(string resourceName)
    {
        if (!productionRates.ContainsKey(resourceName))
        {
            return 0;
        }
        
        return productionRates[resourceName];
    }
    
    // Get consumption rate for a resource
    public float GetConsumptionRate(string resourceName)
    {
        if (!consumptionRates.ContainsKey(resourceName))
        {
            return 0;
        }
        
        return consumptionRates[resourceName];
    }
    
    // Get all production rates
    public Dictionary<string, float> GetAllProductionRates()
    {
        return new Dictionary<string, float>(productionRates);
    }
    
    // Get all consumption rates
    public Dictionary<string, float> GetAllConsumptionRates()
    {
        return new Dictionary<string, float>(consumptionRates);
    }
}
