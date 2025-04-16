using UnityEngine;
using System;
using System.Collections.Generic;

namespace V2.Editor
{
    /// <summary>
    /// Represents a group of related economic parameters
    /// </summary>
    [Serializable]
    public class ParameterGroup
    {
        public string name;
        public Color groupColor;
        public string[] parameterNames;
        
        public ParameterGroup(string name, Color groupColor, params string[] parameterNames)
        {
            this.name = name;
            this.groupColor = groupColor;
            this.parameterNames = parameterNames;
        }
    }

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
            "Productivity", "Labor Elasticity", "Capital Elasticity", "Cycle Multiplier",
            "Wealth Growth Rate", "Price Volatility", "Decay Rate", 
            "Maintenance Cost Multiplier", "Labor Consumption Rate"
        };
        
        // Parameter groups for organization
        private static readonly ParameterGroup[] _parameterGroups = new ParameterGroup[] {
            new ParameterGroup("Production", new Color(0.9f, 0.7f, 0.3f, 0.6f), 
                "Productivity", "Labor Elasticity", "Capital Elasticity"),
                
            new ParameterGroup("Economic Cycles", new Color(0.3f, 0.7f, 0.9f, 0.6f),
                "Cycle Multiplier", "Wealth Growth Rate", "Price Volatility"),
                
            new ParameterGroup("Infrastructure", new Color(0.7f, 0.3f, 0.9f, 0.6f),
                "Decay Rate", "Maintenance Cost Multiplier"),
                
            new ParameterGroup("Population", new Color(0.3f, 0.9f, 0.4f, 0.6f),
                "Labor Consumption Rate")
        };
        
        public static string[] ParameterNames => _parameterNames;
        public static ParameterGroup[] ParameterGroups => _parameterGroups;
        
        /// <summary>
        /// Get all parameters as a dictionary
        /// </summary>
        public Dictionary<string, float> GetAllParameters()
        {
            return new Dictionary<string, float>
            {
                { "Productivity", productivityFactor },
                { "Labor Elasticity", laborElasticity },
                { "Capital Elasticity", capitalElasticity },
                { "Cycle Multiplier", cycleMultiplier },
                { "Wealth Growth Rate", wealthGrowthRate },
                { "Price Volatility", priceVolatility },
                { "Decay Rate", decayRate },
                { "Maintenance Cost Multiplier", maintenanceCostMultiplier },
                { "Labor Consumption Rate", laborConsumptionRate }
            };
        }
        
        /// <summary>
        /// Get parameter value by name
        /// </summary>
        public float GetParameterByName(string name)
        {
            switch (name.ToLower())
            {
                case "productivity": return productivityFactor;
                case "labor elasticity": return laborElasticity;
                case "capital elasticity": return capitalElasticity;
                case "cycle multiplier": return cycleMultiplier;
                case "wealth growth rate": return wealthGrowthRate;
                case "price volatility": return priceVolatility;
                case "decay rate": return decayRate;
                case "maintenance cost multiplier": return maintenanceCostMultiplier;
                case "labor consumption rate": return laborConsumptionRate;
                default: return 0f;
            }
        }
        
        /// <summary>
        /// Get recommended max value for parameter visualization
        /// </summary>
        public float GetRecommendedMaxValue(string parameterName)
        {
            switch (parameterName.ToLower())
            {
                case "productivity": return 5.0f;
                case "labor elasticity": 
                case "capital elasticity": return 1.0f;
                case "cycle multiplier": return 1.2f;
                case "wealth growth rate": return 20.0f;
                case "price volatility": return 0.5f;
                case "decay rate": return 0.05f;
                case "maintenance cost multiplier": return 2.0f;
                case "labor consumption rate": return 3.0f;
                default: return 1.0f;
            }
        }
        
        // Copy values from an economic system component
        public void SyncFromSystem(V2.Systems.EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            productivityFactor = economicSystem.productivityFactor;
            laborElasticity = economicSystem.laborElasticity;
            capitalElasticity = economicSystem.capitalElasticity;
            cycleMultiplier = economicSystem.cycleMultiplier;
            wealthGrowthRate = economicSystem.wealthGrowthRate;
            priceVolatility = economicSystem.priceVolatility;
            decayRate = economicSystem.decayRate;
            maintenanceCostMultiplier = economicSystem.maintenanceCostMultiplier;
            laborConsumptionRate = economicSystem.laborConsumptionRate;
        }
        
        // Apply values to an economic system component
        public void ApplyToSystem(V2.Systems.EconomicSystem economicSystem)
        {
            if (economicSystem == null) return;
            
            economicSystem.productivityFactor = productivityFactor;
            economicSystem.laborElasticity = laborElasticity;
            economicSystem.capitalElasticity = capitalElasticity;
            economicSystem.cycleMultiplier = cycleMultiplier;
            economicSystem.wealthGrowthRate = wealthGrowthRate;
            economicSystem.priceVolatility = priceVolatility;
            economicSystem.decayRate = decayRate;
            economicSystem.maintenanceCostMultiplier = maintenanceCostMultiplier;
            economicSystem.laborConsumptionRate = laborConsumptionRate;
        }
        
        // Get parameter value by index for graphing (for backward compatibility)
        public float GetParameterValueByIndex(int index)
        {
            if (index >= 0 && index < _parameterNames.Length)
            {
                return GetParameterByName(_parameterNames[index]);
            }
            return 0f;
        }
    }
}