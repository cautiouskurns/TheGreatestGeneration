using UnityEngine;

public class RegionEconomyComponent
{
    private RegionEntity region;
    
    // Economic properties
    public int wealth;
    public int production;
    public float productionEfficiency = 1.0f;
    public float capitalInvestment = 10.0f;
    
    // Change tracking
    public int wealthDelta = 0;
    public int productionDelta = 0;
    
    public RegionEconomyComponent(RegionEntity owner, int initialWealth, int initialProduction)
    {
        region = owner;
        wealth = initialWealth;
        production = initialProduction;
    }
    
    // Apply economy changes
    public void ApplyChanges(int wealthChange, int productionChange)
    {
        wealth += wealthChange;
        production += productionChange;
        
        // Track changes for UI
        wealthDelta = wealthChange;
        productionDelta = productionChange;
    }
    
    // Apply satisfaction effects to economy
    public void ApplySatisfactionEffects(float satisfaction)
    {
        if (satisfaction < 0.5f)
        {
            // Low satisfaction leads to wealth loss
            int wealthLoss = Mathf.RoundToInt((0.5f - satisfaction) * 20);
            wealth -= wealthLoss;
            wealthDelta -= wealthLoss;
        }
    }
    
    // Adjust capital investment
    public void AdjustCapitalInvestment(float amount)
    {
        capitalInvestment += amount;
    }
    
    // Reset change tracking flags
    public void ResetChangeFlags()
    {
        wealthDelta = 0;
        productionDelta = 0;
    }
}