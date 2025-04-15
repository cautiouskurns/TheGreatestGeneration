using UnityEngine;
using System;

namespace V2.Editor
{
    /// <summary>
    /// Class encapsulating all economic parameters that can be adjusted in the editor
    /// </summary>
    [Serializable]
    public class EconomicParameters
    {
        // Production parameters
        [Tooltip("Controls the overall productivity multiplier of the economy")]
        public float productivityFactor = 1.0f;
        
        [Tooltip("How much labor affects production (Cobb-Douglas function)")]
        public float laborElasticity = 0.5f;
        
        [Tooltip("How much capital/infrastructure affects production (Cobb-Douglas function)")]
        public float capitalElasticity = 0.5f;
        
        // Cycle parameters
        [Tooltip("Economic cycle effect on production and wealth growth")]
        public float cycleMultiplier = 1.05f;
        
        [Tooltip("Base rate of wealth accumulation per tick")]
        public float wealthGrowthRate = 5.0f;
        
        [Tooltip("How much prices fluctuate randomly")]
        public float priceVolatility = 0.1f;
        
        // Infrastructure parameters
        [Tooltip("How quickly infrastructure deteriorates per tick")]
        public float decayRate = 0.01f;
        
        [Tooltip("Base cost of infrastructure maintenance")]
        public float maintenanceCostMultiplier = 0.5f;
        
        // Population parameters
        [Tooltip("How much each unit of labor consumes from production")]
        public float laborConsumptionRate = 1.5f;
        
        // Parameter names for display and selection
        private static readonly string[] _parameterNames = new string[] { 
            "Productivity", "Labor Elasticity", "Capital Elasticity", "Cycle Multiplier" 
        };
        
        public static string[] ParameterNames => _parameterNames;
        
        // Copy values from an economic system component
        public void SyncFromSystem(V2.Systems.EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            productivityFactor = economicSystem.productivityFactor;
            laborElasticity = economicSystem.laborElasticity;
            capitalElasticity = economicSystem.capitalElasticity;
            cycleMultiplier = economicSystem.cycleMultiplier;
            // Other parameters would be synced here if exposed by EconomicSystem
        }
        
        // Apply values to an economic system component
        public void ApplyToSystem(V2.Systems.EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            economicSystem.productivityFactor = productivityFactor;
            economicSystem.laborElasticity = laborElasticity;
            economicSystem.capitalElasticity = capitalElasticity;
            economicSystem.cycleMultiplier = cycleMultiplier;
            // Other parameters would be applied here if exposed by EconomicSystem
        }
        
        // Get parameter value by index for graphing
        public float GetParameterValueByIndex(int index)
        {
            switch (index)
            {
                case 0: return productivityFactor;
                case 1: return laborElasticity;
                case 2: return capitalElasticity;
                case 3: return cycleMultiplier;
                default: return 0f;
            }
        }
    }
}