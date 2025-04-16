using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles economic cycles and applies cycle multipliers to the economy
    /// </summary>
    public class CycleManager : EconomicSubsystem
    {
        public CycleManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Economic Cycle...");
            
            // Get cycle multiplier from the economic system
            float cycleMultiplier = economicSystem.cycleMultiplier;
            
            // Apply cycle effects to economy
            int originalProduction = region.Economy.Production;
            int originalWealth = region.Economy.Wealth;
            
            // Apply multiplier to production and wealth
            region.Economy.Production = Mathf.RoundToInt(originalProduction * cycleMultiplier);
            region.Economy.Wealth = Mathf.RoundToInt(originalWealth * cycleMultiplier);
            
            // Calculate the change for logging
            int productionDelta = region.Economy.Production - originalProduction;
            int wealthDelta = region.Economy.Wealth - originalWealth;
            
            Debug.Log($"[Cycle] Multiplier: {cycleMultiplier:F2}, Production: {originalProduction} → {region.Economy.Production} ({productionDelta:+0;-#}), " +
                     $"Wealth: {originalWealth} → {region.Economy.Wealth} ({wealthDelta:+0;-#})");
        }
    }
}