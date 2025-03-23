using UnityEngine;
public class RegionEntity
{
    public string regionName;
    public int wealth;
    public int production;
    public string ownerNationName;
    public Color regionColor;
    
    // Add a flag to track changes for visual feedback
    public bool hasChangedThisTurn = false;
    public int wealthDelta = 0;
    public int productionDelta = 0;

    public RegionEntity(string name, int initialWealth, int initialProduction, string nationName, Color color)
    {
        regionName = name;
        wealth = initialWealth;
        production = initialProduction;
        ownerNationName = nationName;
        regionColor = color;
    }

    // Update method to track changes
    public void UpdateEconomy(int wealthChange, int productionChange)
    {
        wealth += wealthChange;
        production += productionChange;
        
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
}

