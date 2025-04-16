using UnityEngine;
using System.Collections.Generic;

namespace V2.Editor
{
    /// <summary>
    /// Defines preset economic scenarios for testing and simulation
    /// </summary>
    public class EconomicScenarios
    {
        // List of available scenario names for the dropdown
        public static string[] ScenarioNames = new string[] 
        {
            "Default",
            "Rapid Industrialization",
            "Economic Recession",
            "Resource-Rich Developing Economy",
            "Advanced Service Economy",
            "Hyperinflation Crisis",
            "Sustainable Growth"
        };
        
        /// <summary>
        /// Apply a specific economic scenario to the parameters
        /// </summary>
        /// <param name="scenarioIndex">Index of the scenario to apply</param>
        /// <param name="parameters">Economic parameters to modify</param>
        /// <param name="regionController">Region controller to modify</param>
        public static void ApplyScenario(int scenarioIndex, EconomicParameters parameters, EconomicRegionController regionController)
        {
            switch(scenarioIndex)
            {
                case 0: // Default
                    ApplyDefaultScenario(parameters, regionController);
                    break;
                case 1: // Rapid Industrialization
                    ApplyIndustrializationScenario(parameters, regionController);
                    break;
                case 2: // Economic Recession
                    ApplyRecessionScenario(parameters, regionController);
                    break;
                case 3: // Resource-Rich Developing Economy
                    ApplyDevelopingEconomyScenario(parameters, regionController);
                    break;
                case 4: // Advanced Service Economy
                    ApplyServiceEconomyScenario(parameters, regionController);
                    break;
                case 5: // Hyperinflation Crisis
                    ApplyHyperinflationScenario(parameters, regionController);
                    break;
                case 6: // Sustainable Growth
                    ApplySustainableGrowthScenario(parameters, regionController);
                    break;
            }
        }
        
        /// <summary>
        /// Get a description of the selected scenario
        /// </summary>
        public static string GetScenarioDescription(int scenarioIndex)
        {
            switch(scenarioIndex)
            {
                case 0: 
                    return "Default balanced economic parameters.";
                case 1: 
                    return "Simulates an economy undergoing rapid industrialization or technological advancement (like post-war economic booms or tech revolutions).";
                case 2: 
                    return "Simulates an economy experiencing recession, high unemployment, and stagnation.";
                case 3: 
                    return "Simulates a developing economy with abundant natural resources but limited infrastructure.";
                case 4: 
                    return "Simulates a developed economy shifting from manufacturing to services.";
                case 5: 
                    return "Simulates an economy experiencing extreme inflation and economic instability.";
                case 6: 
                    return "Simulates a balanced economy focusing on sustainable, long-term growth.";
                default:
                    return "Unknown scenario.";
            }
        }
        
        // Default balanced scenario
        private static void ApplyDefaultScenario(EconomicParameters parameters, EconomicRegionController regionController)
        {
            // Production parameters
            parameters.productivityFactor = 1.0f;
            parameters.laborElasticity = 0.5f;
            parameters.capitalElasticity = 0.5f;
            
            // Cycle parameters
            parameters.cycleMultiplier = 1.05f;
            parameters.wealthGrowthRate = 5.0f;
            parameters.priceVolatility = 0.1f;
            
            // Infrastructure parameters
            parameters.decayRate = 0.01f;
            parameters.maintenanceCostMultiplier = 0.5f;
            
            // Population parameters
            parameters.laborConsumptionRate = 1.5f;
            
            // Region parameters
            regionController.laborAvailable = 100;
            regionController.infrastructureLevel = 1;
        }
        
        // Rapid Industrialization / Economic Boom
        private static void ApplyIndustrializationScenario(EconomicParameters parameters, EconomicRegionController regionController)
        {
            // Production parameters
            parameters.productivityFactor = 2.0f;
            parameters.laborElasticity = 0.5f;
            parameters.capitalElasticity = 0.5f;
            
            // Cycle parameters
            parameters.cycleMultiplier = 1.15f;
            parameters.wealthGrowthRate = 12.0f;
            parameters.priceVolatility = 0.15f;
            
            // Infrastructure parameters
            parameters.decayRate = 0.005f;
            parameters.maintenanceCostMultiplier = 0.6f;
            
            // Population parameters
            parameters.laborConsumptionRate = 1.2f;
            
            // Region parameters
            regionController.laborAvailable = 200;
            regionController.infrastructureLevel = 3;
        }
        
        // Economic Recession / Stagflation
        private static void ApplyRecessionScenario(EconomicParameters parameters, EconomicRegionController regionController)
        {
            // Production parameters
            parameters.productivityFactor = 0.75f;
            parameters.laborElasticity = 0.35f;
            parameters.capitalElasticity = 0.45f;
            
            // Cycle parameters
            parameters.cycleMultiplier = 0.9f;
            parameters.wealthGrowthRate = 2.0f;
            parameters.priceVolatility = 0.3f;
            
            // Infrastructure parameters
            parameters.decayRate = 0.025f;
            parameters.maintenanceCostMultiplier = 1.0f;
            
            // Population parameters
            parameters.laborConsumptionRate = 2.0f;
            
            // Region parameters
            regionController.laborAvailable = 80;
            regionController.infrastructureLevel = 4;
        }
        
        // Resource-Rich Developing Economy
        private static void ApplyDevelopingEconomyScenario(EconomicParameters parameters, EconomicRegionController regionController)
        {
            // Production parameters
            parameters.productivityFactor = 1.3f;
            parameters.laborElasticity = 0.7f;
            parameters.capitalElasticity = 0.3f;
            
            // Cycle parameters
            parameters.cycleMultiplier = 1.1f;
            parameters.wealthGrowthRate = 6.0f;
            parameters.priceVolatility = 0.25f;
            
            // Infrastructure parameters
            parameters.decayRate = 0.02f;
            parameters.maintenanceCostMultiplier = 1.5f;
            
            // Population parameters
            parameters.laborConsumptionRate = 1.8f;
            
            // Region parameters
            regionController.laborAvailable = 250;
            regionController.infrastructureLevel = 2;
        }
        
        // Advanced Service Economy
        private static void ApplyServiceEconomyScenario(EconomicParameters parameters, EconomicRegionController regionController)
        {
            // Production parameters
            parameters.productivityFactor = 1.8f;
            parameters.laborElasticity = 0.35f;
            parameters.capitalElasticity = 0.65f;
            
            // Cycle parameters
            parameters.cycleMultiplier = 1.03f;
            parameters.wealthGrowthRate = 7.0f;
            parameters.priceVolatility = 0.08f;
            
            // Infrastructure parameters
            parameters.decayRate = 0.005f;
            parameters.maintenanceCostMultiplier = 0.7f;
            
            // Population parameters
            parameters.laborConsumptionRate = 1.7f;
            
            // Region parameters
            regionController.laborAvailable = 150;
            regionController.infrastructureLevel = 8;
        }
        
        // Hyperinflation Crisis
        private static void ApplyHyperinflationScenario(EconomicParameters parameters, EconomicRegionController regionController)
        {
            // Production parameters
            parameters.productivityFactor = 0.7f;
            parameters.laborElasticity = 0.5f;
            parameters.capitalElasticity = 0.5f;
            
            // Cycle parameters
            parameters.cycleMultiplier = 0.92f;
            parameters.wealthGrowthRate = 15.0f; // High nominal growth but real value decreases
            parameters.priceVolatility = 0.45f;
            
            // Infrastructure parameters
            parameters.decayRate = 0.03f;
            parameters.maintenanceCostMultiplier = 1.8f;
            
            // Population parameters
            parameters.laborConsumptionRate = 2.5f;
            
            // Region parameters
            regionController.laborAvailable = 90;
            regionController.infrastructureLevel = 5;
        }
        
        // Sustainable Growth Economy
        private static void ApplySustainableGrowthScenario(EconomicParameters parameters, EconomicRegionController regionController)
        {
            // Production parameters
            parameters.productivityFactor = 1.4f;
            parameters.laborElasticity = 0.5f;
            parameters.capitalElasticity = 0.5f;
            
            // Cycle parameters
            parameters.cycleMultiplier = 1.04f;
            parameters.wealthGrowthRate = 7.0f;
            parameters.priceVolatility = 0.1f;
            
            // Infrastructure parameters
            parameters.decayRate = 0.005f;
            parameters.maintenanceCostMultiplier = 0.8f;
            
            // Population parameters
            parameters.laborConsumptionRate = 1.4f;
            
            // Region parameters
            regionController.laborAvailable = 130;
            regionController.infrastructureLevel = 6;
        }
    }
}