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
    

    // Add to ResourceComponent class
    private float baseConsumptionFactor = 1.0f;
    private float wealthConsumptionMultiplier = 0.01f; // Consumption increases 1% per wealth point
    private float sizeConsumptionMultiplier = 0.2f;    // Consumption increases 20% per "size unit"

    // Constructor
    public ResourceComponent(RegionEntity owner)
    {
        ownerRegion = owner;
        InitializeBasicResources();
    }
    
    // Add to ResourceComponent class
    private Dictionary<string, ResourceDataSO> resourceDefinitions = new Dictionary<string, ResourceDataSO>();

    // Method to register a resource definition
    public void RegisterResourceDefinition(ResourceDataSO resourceData)
    {
        if (resourceData != null)
        {
            resourceDefinitions[resourceData.resourceName] = resourceData;
            
            // Initialize storage for this resource if it doesn't exist yet
            if (!resources.ContainsKey(resourceData.resourceName))
            {
                resources[resourceData.resourceName] = 0;
            }
        }
    }

    // Add to ResourceComponent class
    public void LoadResourceDefinitions(ResourceDataSO[] definitions)
    {
        foreach (var definition in definitions)
        {
            RegisterResourceDefinition(definition);
        }
        
        // After loading definitions, calculate base production based on definitions
        CalculateBaseProduction();
    }

    // Initialize resources based on definitions
    private void InitializeBasicResources()
    {
        // Don't do anything if we don't have resource definitions yet
        if (resourceDefinitions.Count == 0)
        {
            //Debug.LogWarning("No resource definitions available for initialization");
            return;
        }
        
        // Initialize resources with appropriate starting quantities
        foreach (var entry in resourceDefinitions)
        {
            string resourceName = entry.Key;
            ResourceDataSO resourceData = entry.Value;
            
            // Set initial quantities based on resource type and category
            float initialAmount = 0f;
            
            // Primary resources start with more
            if (resourceData.category == ResourceDataSO.ResourceCategory.Primary)
            {
                initialAmount = resourceData.baseValue * 5f;
                
                // Adjust based on resource type
                if (resourceData.resourceType == ResourceDataSO.ResourceType.Food)
                    initialAmount *= 2f; // More starting food
            }
            // Secondary resources start with moderate amounts
            else if (resourceData.category == ResourceDataSO.ResourceCategory.Secondary)
            {
                initialAmount = resourceData.baseValue * 2.5f;
            }
            // Tertiary resources start with small amounts
            else if (resourceData.category == ResourceDataSO.ResourceCategory.Tertiary)
            {
                initialAmount = resourceData.baseValue * 1f;
            }
            
            // Apply terrain type modifiers if available
            if (ownerRegion.terrainType != null)
            {
                string sector = GetSectorForResourceType(resourceData.resourceType);
                float terrainMultiplier = ownerRegion.terrainType.GetMultiplierForSector(sector);
                
                // Boost initial resources based on terrain affinity
                if (terrainMultiplier > 1.2f)
                    initialAmount *= 1.5f;
            }
            
            // Set the resource amount
            resources[resourceName] = initialAmount;
        }
        
        // Calculate initial production rates based on terrain
        CalculateBaseProduction();
    }
    
    // Update CalculateBaseProduction method
    public void CalculateBaseProduction()
    {
        // Clear existing rates
        productionRates.Clear();
        consumptionRates.Clear();
        
        // Set production rates based on resource definitions and terrain
        foreach (var entry in resourceDefinitions)
        {
            string resourceName = entry.Key;
            ResourceDataSO resourceData = entry.Value;
            
            // Set base production rate for raw resources
            if (resourceData.isRawResource)
            {
                float baseRate = resourceData.baseValue / 10f; // Convert value to production rate
                productionRates[resourceName] = baseRate;
                
                // Apply terrain modifiers if available
                if (ownerRegion.terrainType != null)
                {
                    // Match resource type to appropriate sector
                    string sector = GetSectorForResourceType(resourceData.resourceType);
                    float terrainMultiplier = ownerRegion.terrainType.GetMultiplierForSector(sector);
                    
                    productionRates[resourceName] *= terrainMultiplier;
                }
                
                // Set base consumption rate based on the same baseRate variable
                consumptionRates[resourceName] = baseRate / 2f; // Simple consumption model
            }
            else
            {
                // For non-raw resources, set minimal production and consumption
                productionRates[resourceName] = 0f;
                consumptionRates[resourceName] = resourceData.baseValue / 20f; // Small consumption rate
            }
        }
    }

    // Helper method to map resource types to sectors
    private string GetSectorForResourceType(ResourceDataSO.ResourceType resourceType)
    {
        switch (resourceType)
        {
            case ResourceDataSO.ResourceType.Food:
                return "agriculture";
            case ResourceDataSO.ResourceType.Material:
                return resourceType == ResourceDataSO.ResourceType.Material ? "mining" : "industry";
            case ResourceDataSO.ResourceType.Wealth:
                return "commerce";
            default:
                return "industry";
        }
    }

    public void CalculateConsumption(int regionWealth, float regionSize)
    {
        // Base factor affected by wealth and size
        float wealthFactor = 1.0f + (regionWealth * wealthConsumptionMultiplier);
        float sizeFactor = 1.0f + (regionSize * sizeConsumptionMultiplier);
        
        // Calculate consumption for each resource definition
        foreach (var entry in resourceDefinitions)
        {
            string resourceName = entry.Key;
            ResourceDataSO resourceData = entry.Value;
            
            // Different consumption rates based on resource type
            float baseConsumption = 0f;
            
            // Set base consumption based on resource type
            switch (resourceData.resourceType)
            {
                case ResourceDataSO.ResourceType.Food:
                    baseConsumption = resourceData.baseValue / 8f;
                    break;
                case ResourceDataSO.ResourceType.Material:
                    baseConsumption = resourceData.baseValue / 15f;
                    break;
                case ResourceDataSO.ResourceType.Wealth:
                    // Wealth is consumed more with higher wealth factor
                    baseConsumption = resourceData.baseValue / 20f * Mathf.Pow(wealthFactor, 1.5f);
                    break;
                default:
                    baseConsumption = resourceData.baseValue / 12f;
                    break;
            }
            
            // Apply wealth and size factors
            consumptionRates[resourceName] = baseConsumption * wealthFactor * sizeFactor;
            
            // Adjust consumption based on resource category
            switch (resourceData.category)
            {
                case ResourceDataSO.ResourceCategory.Primary:
                    // Primary resources are consumed more evenly
                    break;
                case ResourceDataSO.ResourceCategory.Secondary:
                    // Secondary resources scale more with wealth
                    consumptionRates[resourceName] *= wealthFactor;
                    break;
                case ResourceDataSO.ResourceCategory.Tertiary:
                    // Tertiary/luxury goods scale heavily with wealth
                    consumptionRates[resourceName] *= Mathf.Pow(wealthFactor, 2);
                    break;
            }
        }
    }
        
    // Update existing ProcessTurn method
    public void ProcessTurn(int regionWealth, float regionSize)
    {
        // Recalculate consumption based on current region properties
        CalculateConsumption(regionWealth, regionSize);
        
        // Process production and consumption as you currently do
        List<string> resourceKeys = new List<string>(resources.Keys);
        
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

     // Add to ResourceComponent class
    public Dictionary<string, float> GetConsumptionSatisfaction()
    {
        Dictionary<string, float> satisfaction = new Dictionary<string, float>();
        
        foreach (var entry in consumptionRates)
        {
            string resource = entry.Key;
            float needed = entry.Value;
            
            if (needed <= 0) continue;
            
            float available = resources.ContainsKey(resource) ? resources[resource] : 0;
            satisfaction[resource] = Mathf.Clamp01(available / needed);
        }
        
        return satisfaction;
    }   

    public float GetOverallSatisfaction() 
    {
        var satisfaction = GetConsumptionSatisfaction();
        if (satisfaction.Count == 0) return 1.0f;
        
        float total = 0f;
        foreach (var value in satisfaction.Values) {
            total += value;
        }
        
        return total / satisfaction.Count;
    }

    public bool HasConsumptionNeeds()
    {
        foreach (var entry in consumptionRates) {
            if (entry.Value > 0) return true;
        }
        return false;
    }

    public float GetWealthConsumptionFactor(int wealth)
    {
        return 1.0f + (wealth * wealthConsumptionMultiplier);
    }

    public float GetSizeConsumptionFactor(float size)
    {
        return 1.0f + (size * sizeConsumptionMultiplier);
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

    // Add to ResourceComponent
    public Dictionary<string, ResourceDataSO> GetResourceDefinitions()
    {
        return new Dictionary<string, ResourceDataSO>(resourceDefinitions);
    }

    // Add population demand factors
    private Dictionary<string, float> baseConsumptionPerCapita = new Dictionary<string, float>();

    // Calculate demand based on population
    public void CalculateDemand()
    {
        // Clear existing consumption rates
        consumptionRates.Clear();

        // For each resource
        foreach (var resource in resourceDefinitions.Keys)
        {
            // Base consumption per capita (default or from definition)
            float perCapitaNeed = 0.1f; // Default
            if (baseConsumptionPerCapita.ContainsKey(resource))
                perCapitaNeed = baseConsumptionPerCapita[resource];

            // Total consumption = population × per capita need × wealth multiplier
            float totalDemand = ownerRegion.laborAvailable * perCapitaNeed *
                                (1.0f + ownerRegion.wealth * 0.001f); // Wealth increases consumption

            consumptionRates[resource] = totalDemand;
        }
    }

    // Calculate production based on labor allocation
    public void CalculateProduction()
    {
        // Clear existing production rates
        productionRates.Clear();

        foreach (var resource in resourceDefinitions.Values)
        {
            // Skip non-raw resources (handled by ProductionComponent)
            if (!resource.isRawResource) continue;

            // Get appropriate sector for this resource
            string sector = GetSectorForResourceType(resource.resourceType);

            // Get labor allocated to this sector
            float laborShare = 0.0f;
            if (ownerRegion.laborAllocation.ContainsKey(sector))
                laborShare = ownerRegion.laborAllocation[sector];

            // Base production = resource base value × labor allocation × land productivity
            float baseProduction = resource.baseValue * 0.2f *
                                (ownerRegion.laborAvailable * laborShare) *
                                ownerRegion.landProductivity;

            // Apply production efficiency and capital investment
            baseProduction *= ownerRegion.productionEfficiency *
                            (1.0f + ownerRegion.capitalInvestment * 0.05f);

            // Apply terrain modifiers
            if (ownerRegion.terrainType != null)
            {
                float terrainModifier = ownerRegion.terrainType.GetMultiplierForSector(sector);
                baseProduction *= terrainModifier;
            }

            productionRates[resource.resourceName] = baseProduction;
        }
    }
}
