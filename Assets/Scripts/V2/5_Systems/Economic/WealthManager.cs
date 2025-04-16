using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles wealth accumulation, growth, and spending
    /// </summary>
    public class WealthManager : EconomicSubsystem
    {
        public WealthManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Wealth Management...");
            
            // Production contributes to wealth accumulation
            int wealthFromProduction = region.Economy.Production;
            
            // Apply wealth growth rate from the economic system
            float wealthGrowth = economicSystem.wealthGrowthRate;
            int additionalWealth = Mathf.RoundToInt(region.Economy.Wealth * (wealthGrowth / 100f));
            
            // Update wealth accounting for both production and growth rate
            region.Economy.Wealth += wealthFromProduction + additionalWealth;
            
            // In a more complex implementation, we might handle investments, returns, etc.
            
            Debug.Log($"[Wealth] Production Income: {wealthFromProduction}, Growth: {additionalWealth}, Total Wealth: {region.Economy.Wealth}");
        }
    }
}