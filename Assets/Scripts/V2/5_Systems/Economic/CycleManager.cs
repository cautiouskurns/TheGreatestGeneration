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
            
            float cycleMultiplier = economicSystem.cycleMultiplier;
            
            // Apply cycle effects to economy
            region.Economy.Production = Mathf.RoundToInt(region.Economy.Production * cycleMultiplier);
            region.Economy.Wealth = Mathf.RoundToInt(region.Economy.Wealth * cycleMultiplier);
            
            Debug.Log($"[Cycle] Production: {region.Economy.Production}, Wealth: {region.Economy.Wealth}");
        }
    }
}