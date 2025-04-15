using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles supply, demand, and market imbalance calculations
    /// </summary>
    public class MarketBalancer : EconomicSubsystem
    {
        private float supplyFactor = 0.8f;
        private float demandFactor = 1.2f;
        
        public MarketBalancer(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Supply and Demand...");
            
            float supply = region.Economy.Production * supplyFactor;
            float demand = region.Population.LaborAvailable * demandFactor;
            float imbalance = demand - supply;
            
            // Store values for potential future use
            // In a more advanced implementation, these values could be stored in a market component
            
            Debug.Log($"[Supply/Demand] Supply: {supply}, Demand: {demand}, Imbalance: {imbalance}");
        }
    }
}