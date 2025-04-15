using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles price fluctuations and market volatility
    /// </summary>
    public class PriceVolatilityManager : EconomicSubsystem
    {
        private float volatilityRange = 0.1f;
        
        public PriceVolatilityManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Price Volatility...");
            
            float volatility = Random.Range(-volatilityRange, volatilityRange);
            float priceIndex = 100f * (1 + volatility);
            
            // In a more advanced implementation, this could affect resource prices or trade values
            
            Debug.Log($"[Prices] Volatility: {volatility:F2}, Price Index: {priceIndex:F2}");
        }
    }
}