using UnityEngine;
using System.Collections.Generic;

public class RegionEntity
{
    // Basic properties
    public string regionName;
    public int wealth;
    public int production;
    public string ownerNationName;
    public Color regionColor;
    
    // Terrain data
    public TerrainTypeDataSO terrainType;
    
    // Change tracking for visual feedback
    public bool hasChangedThisTurn = false;
    public int wealthDelta = 0;
    public int productionDelta = 0;
    
    // Add resource component
    public ResourceComponent resources;
    public ProductionComponent productionComponent;

    // Constructor for basic region
    public RegionEntity(string name, int initialWealth, int initialProduction, string nationName, Color color)
    {
        regionName = name;
        wealth = initialWealth;
        production = initialProduction;
        ownerNationName = nationName;
        regionColor = color;
    }

    // Constructor that includes terrain
    public RegionEntity(string name, int initialWealth, int initialProduction, string nationName, Color color, TerrainTypeDataSO terrain)
        : this(name, initialWealth, initialProduction, nationName, color)
    {
        terrainType = terrain;
        
        // Initialize resource component
        resources = new ResourceComponent(this);
        productionComponent = new ProductionComponent(this, resources);

        if (terrainType != null)
        {
            // Add different starting resources based on terrain type
            switch (terrainType.terrainName)
            {
                case "Forest":
                    resources.AddResource("Wood", 50);
                    resources.AddResource("Food", 30);
                    break;
                case "Mountains":
                    resources.AddResource("Coal", 50);
                    resources.AddResource("Iron Ore", 30);
                    break;
                case "Plains":
                    resources.AddResource("Food", 50);
                    resources.AddResource("Wood", 20);
                    break;
                default:
                    // Default starting resources
                    resources.AddResource("Food", 30);
                    resources.AddResource("Wood", 30);
                    break;
            }
        }
    }

    // In RegionEntity.UpdateEconomy method
    public void UpdateEconomy(int wealthChange, int productionChange)
    {
        // Apply changes to wealth and production as before
        wealth += wealthChange;
        production += productionChange;
        
        // Define region "size" - could be based on production or a new property
        float regionSize = production / 10.0f;
        
        // Process resources with wealth and size factors
        if (resources != null)
        {
            resources.ProcessTurn(wealth, regionSize);
            
            // You could add effects based on consumption satisfaction
            Dictionary<string, float> satisfaction = resources.GetConsumptionSatisfaction();
            
            // Example: If food satisfaction is low, reduce wealth
            if (satisfaction.ContainsKey("Food") && satisfaction["Food"] < 0.5f)
            {
                wealth -= Mathf.RoundToInt((0.5f - satisfaction["Food"]) * 10);
            }
        }

        if (productionComponent != null)
        {
            productionComponent.ProcessProduction();
        }
        
        // Update tracking flags and notify systems
        hasChangedThisTurn = true;
        wealthDelta = wealthChange;
        productionDelta = productionChange;
        
        EventBus.Trigger("RegionUpdated", this);
    }
    
    // Reset changes after visualization
    public void ResetChangeFlags()
    {
        hasChangedThisTurn = false;
        wealthDelta = 0;
        productionDelta = 0;
    }
    
    // Get a description that includes terrain information
    public string GetDescription()
    {
        string description = $"Region: {regionName}\n" +
                            $"Nation: {ownerNationName}\n" +
                            $"Wealth: {wealth}\n" +
                            $"Production: {production}";
        
        if (terrainType != null)
        {
            description += $"\n\nTerrain: {terrainType.terrainName}\n" + 
                          $"{terrainType.description}\n\n" + 
                          $"{GetTerrainEffectsDescription()}";
        }
        
        return description;
    }
    
    // Get a description that includes resources
    public string GetDetailedDescription()
    {
        string description = GetDescription();
        
        // Add resource information if available
        if (resources != null)
        {
            description += "\n\n=== Resources ===\n";
            
            var allResources = resources.GetAllResources();
            var productionRates = resources.GetAllProductionRates();
            var consumptionRates = resources.GetAllConsumptionRates();
            
            foreach (var resource in allResources.Keys)
            {
                float amount = allResources[resource];
                float production = productionRates.ContainsKey(resource) ? productionRates[resource] : 0;
                float consumption = consumptionRates.ContainsKey(resource) ? consumptionRates[resource] : 0;
                float netChange = production - consumption;
                
                string changeIndicator = netChange > 0 ? "↑" : (netChange < 0 ? "↓" : "→");
                description += $"{resource}: {amount:F1} {changeIndicator} ({netChange:+0.0;-0.0})\n";
            }
        }
        
        return description;
    }
    
    // Get a description of the terrain's economic effects
    private string GetTerrainEffectsDescription()
    {
        if (terrainType == null)
            return "";
            
        string effects = "Economic Effects:";
        
        float agricultureMod = terrainType.GetMultiplierForSector("agriculture");
        if (agricultureMod != 1.0f)
            effects += $"\nAgriculture: {FormatModifier(agricultureMod)}";
            
        float miningMod = terrainType.GetMultiplierForSector("mining");
        if (miningMod != 1.0f)
            effects += $"\nMining: {FormatModifier(miningMod)}";
            
        float industryMod = terrainType.GetMultiplierForSector("industry");
        if (industryMod != 1.0f)
            effects += $"\nIndustry: {FormatModifier(industryMod)}";
            
        float commerceMod = terrainType.GetMultiplierForSector("commerce");
        if (commerceMod != 1.0f)
            effects += $"\nCommerce: {FormatModifier(commerceMod)}";
            
        return effects;
    }
    
    // Format a multiplier as a percentage modifier (e.g., +50%, -25%)
    private string FormatModifier(float modifier)
    {
        float percentage = (modifier - 1.0f) * 100f;
        string sign = percentage >= 0 ? "+" : "";
        return $"{sign}{percentage:F0}%";
    }
}

