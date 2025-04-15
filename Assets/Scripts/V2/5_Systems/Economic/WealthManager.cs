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
            
            // Update wealth accounting for production
            region.Economy.Wealth += wealthFromProduction;
            
            // In a more complex implementation, we might handle investments, returns, etc.
            
            Debug.Log($"[Wealth] Production Income: {wealthFromProduction}, Total Wealth: {region.Economy.Wealth}");
        }
    }
}