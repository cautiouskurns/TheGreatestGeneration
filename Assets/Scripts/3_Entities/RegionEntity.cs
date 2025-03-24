using UnityEngine;

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
    }

    // Update economy with terrain modifiers if available
    public void UpdateEconomy(int wealthChange, int productionChange)
    {
        // Apply terrain modifiers if available
        if (terrainType != null)
        {
            // Apply terrain multipliers to respective sectors
            // For wealth, we'll use the commerce multiplier as an approximation
            wealthChange = Mathf.RoundToInt(wealthChange * terrainType.GetMultiplierForSector("commerce"));
            
            // For production, we'll use the industry multiplier
            productionChange = Mathf.RoundToInt(productionChange * terrainType.GetMultiplierForSector("industry"));
        }
        
        // Apply the changes
        wealth += wealthChange;
        production += productionChange;
        
        // Process resources if available
        if (resources != null)
        {
            resources.ProcessTurn();
        }
        
        // Track changes for visual feedback
        hasChangedThisTurn = true;
        wealthDelta = wealthChange;
        productionDelta = productionChange;
        
        // Notify UI & other systems that this region has been updated
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

