using UnityEngine;
using System;
using System.Collections.Generic;

namespace V1.Data
{


    /// CLASS PURPOSE:
    /// EnhancedEconomicModelSO defines configurable economic rules for simulation systems,
    /// covering supply and demand mechanics, production transformation, infrastructure effects,
    /// population behavior, economic cycles, and price volatility.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Store all economic rule sets used by the economy simulation systems
    /// - Provide tunable parameters via ScriptableObject for designers
    /// - Offer validation logic to ensure economic consistency and catch errors
    ///
    /// KEY COLLABORATORS:
    /// - EconomicSystem: Uses this data to calculate output, demand, prices, and volatility
    /// - RegionEntity / NationEntity: Apply modifiers from this data to simulate regional outcomes
    /// - GameManager / BalanceTools: Run validation to test and iterate tuning
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Organized into modular rule classes (SupplyDemand, Production, etc.)
    /// - Uses nested Serializable classes to enable inspector exposure
    /// - Includes runtime validation to catch misconfigurations
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Add data versioning or compatibility validation across model updates
    /// - Split out rule classes into separate SOs for composability or overrides
    /// - Allow export of settings for balance testing or external tuning
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Introduce AI heuristics that use this model for decision-making
    /// - Track economic indicators over time using these parameters
    /// - Enable dynamic runtime modification for advanced scenarios or policies
    /// 
    [CreateAssetMenu(fileName = "EnhancedEconomicModel", menuName = "Economic Cycles/Enhanced Economic Model")]
    public class EnhancedEconomicModelSO : ScriptableObject
    {
        [Serializable]
        public class SupplyDemandRules
        {
            [Header("Supply and Demand Mechanics")]
            [Tooltip("Base elasticity for supply and demand interactions")]
            public float baseSupplyElasticity = 0.5f;

            [Tooltip("Elasticity values for different resource types")]
            public Dictionary<string, float> resourceElasticityMap = new Dictionary<string, float>
            {
                { "Food", 0.3f },
                { "Materials", 0.5f },
                { "Luxury", 0.7f }
            };
            

            [Tooltip("Cross-price elasticity between resource types")]
            public Dictionary<string, Dictionary<string, float>> crossPriceElasticityMap = 
                new Dictionary<string, Dictionary<string, float>>
                {
                    { 
                        "Food", new Dictionary<string, float> 
                        { 
                            { "Bread", 0.2f }, 
                            { "Rice", 0.3f } 
                        } 
                    }
                };

            [Tooltip("How demand is calculated based on population, wealth, and infrastructure")]
            public AnimationCurve demandCalculationCurve = AnimationCurve.Linear(0, 0.5f, 1, 1f);
        }

        [Serializable]
        public class ProductionTransformationRules
        {
            [Header("Cobb-Douglas Production Function")]
            [Tooltip("Productivity factor (A)")]
            public float productivityFactor = 1f;

            [Tooltip("Labor elasticity (α)")]
            public float laborElasticity = 0.7f;

            [Tooltip("Capital/Infrastructure elasticity (β)")]
            public float capitalElasticity = 0.3f;

            [Tooltip("Sector-specific production multipliers")]
            public Dictionary<string, float> sectorProductivityMultipliers = new Dictionary<string, float>
            {
                { "Agriculture", 1.1f },
                { "Industry", 1.0f },
                { "Commerce", 0.9f }
            };
        }

        [Serializable]
        public class InfrastructureImpactRules
        {
            [Header("Infrastructure Efficiency")]
            [Tooltip("Base infrastructure efficiency multiplier")]
            public float baseInfrastructureMultiplier = 1f;

            [Tooltip("Infrastructure efficiency curve")]
            public AnimationCurve infrastructureEfficiencyCurve = AnimationCurve.Linear(0, 0.5f, 5, 2f);

            [Tooltip("Infrastructure decay rate")]
            public float infrastructureDecayRate = 0.05f;

            [Tooltip("Maintenance cost thresholds")]
            public AnimationCurve maintenanceCostCurve = AnimationCurve.Linear(0, 0.1f, 5, 0.5f);
        }

        [Serializable]
        public class PopulationConsumptionRules
        {
            [Header("Consumption Mechanics")]
            [Tooltip("Base consumption rate")]
            public float baseConsumptionRate = 0.5f;

            [Tooltip("Wealth per capita consumption elasticity")]
            public float wealthConsumptionElasticity = 0.6f;

            [Tooltip("Unmet demand impact curves")]
            public AnimationCurve unrestFromUnmetDemandCurve = AnimationCurve.Linear(0, 0, 1, 1f);
            public AnimationCurve migrationFromUnmetDemandCurve = AnimationCurve.Linear(0, 0, 1, 1f);
        }

        [Serializable]
        public class EconomicCycleRules
        {
            [Header("Economic Cycle Effects")]
            [Tooltip("Cycle phase multipliers")]
            public Dictionary<string, float> cyclePhaseMultipliers = new Dictionary<string, float>
            {
                { "Expansion", 1.2f },
                { "Peak", 1.1f },
                { "Contraction", 0.8f },
                { "Recession", 0.6f }
            };

            [Tooltip("Cycle effect coefficient")]
            public float cycleEffectCoefficient = 0.1f;
        }

        [Serializable]
        public class PriceVolatilityRules
        {
            [Header("Price Volatility and Market Feedback")]
            [Tooltip("Base price volatility")]
            public float basePriceVolatility = 0.2f;

            [Tooltip("Stochastic shock magnitude")]
            public float stochasticShockMagnitude = 0.1f;

            [Tooltip("Supply shock sensitivity")]
            public float supplyShockSensitivity = 0.5f;

            [Tooltip("Consumption trend impact")]
            public float consumptionTrendImpact = 0.3f;
        }

        [Header("Supply and Demand Rules")]
        public SupplyDemandRules supplyDemandRules = new SupplyDemandRules();

        [Header("Production Transformation Rules")]
        public ProductionTransformationRules productionRules = new ProductionTransformationRules();

        [Header("Infrastructure Impact Rules")]
        public InfrastructureImpactRules infrastructureRules = new InfrastructureImpactRules();

        [Header("Population Consumption Rules")]
        public PopulationConsumptionRules populationConsumptionRules = new PopulationConsumptionRules();

        [Header("Economic Cycle Rules")]
        public EconomicCycleRules economicCycleRules = new EconomicCycleRules();

        [Header("Price Volatility Rules")]
        public PriceVolatilityRules priceVolatilityRules = new PriceVolatilityRules();

        // Comprehensive validation method
        public void ValidateEconomicModel()
        {
            Debug.Log("Validating Enhanced Economic Model...");

            // Validate Supply and Demand
            ValidateSupplyDemandRules();

            // Validate Production Transformation
            ValidateProductionRules();

            // Validate Infrastructure
            ValidateInfrastructureRules();

            // Validate Population Consumption
            ValidatePopulationConsumptionRules();

            // Validate Economic Cycles
            ValidateEconomicCycleRules();

            // Validate Price Volatility
            ValidatePriceVolatilityRules();
        }

        private void ValidateSupplyDemandRules()
        {
            if (supplyDemandRules.baseSupplyElasticity <= 0)
                Debug.LogWarning("Supply elasticity should be positive!");
        }

        private void ValidateProductionRules()
        {
            if (productionRules.productivityFactor <= 0)
                Debug.LogWarning("Productivity factor should be positive!");
            
            if (productionRules.laborElasticity + productionRules.capitalElasticity > 1)
                Debug.LogWarning("Combined elasticity might lead to decreasing returns to scale!");
        }

        private void ValidateInfrastructureRules()
        {
            if (infrastructureRules.baseInfrastructureMultiplier <= 0)
                Debug.LogWarning("Base infrastructure multiplier should be positive!");
        }

        private void ValidatePopulationConsumptionRules()
        {
            if (populationConsumptionRules.baseConsumptionRate < 0)
                Debug.LogWarning("Base consumption rate cannot be negative!");
        }

        private void ValidateEconomicCycleRules()
        {
            if (economicCycleRules.cycleEffectCoefficient < 0)
                Debug.LogWarning("Cycle effect coefficient should not be negative!");
        }

        private void ValidatePriceVolatilityRules()
        {
            if (priceVolatilityRules.basePriceVolatility < 0 || priceVolatilityRules.basePriceVolatility > 1)
                Debug.LogWarning("Price volatility should be between 0 and 1!");
        }
    }
}