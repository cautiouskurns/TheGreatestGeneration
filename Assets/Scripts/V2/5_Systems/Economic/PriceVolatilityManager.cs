using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles price fluctuations and market volatility
    /// </summary>
    public class PriceVolatilityManager : EconomicSubsystem
    {
        public PriceVolatilityManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Price Volatility...");
            
            // Get volatility from the economic system
            float volatilityRange = economicSystem.priceVolatility;
            
            float volatility = Random.Range(-volatilityRange, volatilityRange);
            float priceIndex = 100f * (1 + volatility);
            
            // In a more advanced implementation, this could affect resource prices or trade values
            
            Debug.Log($"[Prices] Volatility: {volatility:F2}, Price Index: {priceIndex:F2}");
        }
    }
}