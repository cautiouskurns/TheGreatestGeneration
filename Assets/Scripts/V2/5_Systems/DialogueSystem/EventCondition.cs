using UnityEngine;
using V2.Entities;
using V2.Systems;

namespace V2.Systems.DialogueSystem
{
    [System.Serializable]
    public class EventCondition
    {
        public enum ParameterType
        {
            Wealth,
            Production,
            LaborAvailable,
            InfrastructureLevel,
            PopulationSatisfaction,
            ProductivityFactor,
            LaborElasticity,
            CapitalElasticity,
            CycleMultiplier,
            WealthGrowthRate,
            PriceVolatility,
            DecayRate,
            MaintenanceCostMultiplier,
            LaborConsumptionRate
        }
        
        public enum ComparisonType
        {
            GreaterThan,
            LessThan,
            Equals
        }
        
        public ParameterType parameter;
        public ComparisonType comparison;
        public float thresholdValue;
        
        public bool CheckCondition(EconomicSystem system, RegionEntity region)
        {
            if (system == null || region == null)
                return false;
                
            float currentValue = GetParameterValue(parameter, system, region);
            
            switch (comparison)
            {
                case ComparisonType.GreaterThan:
                    return currentValue > thresholdValue;
                case ComparisonType.LessThan:
                    return currentValue < thresholdValue;
                case ComparisonType.Equals:
                    return Mathf.Approximately(currentValue, thresholdValue);
                default:
                    return false;
            }
        }
        
        private float GetParameterValue(ParameterType parameter, EconomicSystem system, RegionEntity region)
        {
            switch (parameter)
            {
                case ParameterType.Wealth:
                    return region.Economy.Wealth;
                case ParameterType.Production:
                    return region.Economy.Production;
                case ParameterType.LaborAvailable:
                    return region.Population.LaborAvailable;
                case ParameterType.InfrastructureLevel:
                    return region.Infrastructure.Level;
                case ParameterType.PopulationSatisfaction:
                    return region.Population.Satisfaction;
                // Economic system parameters
                case ParameterType.ProductivityFactor:
                    return system.productivityFactor;
                case ParameterType.LaborElasticity:
                    return system.laborElasticity;
                case ParameterType.CapitalElasticity:
                    return system.capitalElasticity;
                case ParameterType.CycleMultiplier:
                    return system.cycleMultiplier;
                case ParameterType.WealthGrowthRate:
                    return system.wealthGrowthRate;
                case ParameterType.PriceVolatility:
                    return system.priceVolatility;
                case ParameterType.DecayRate:
                    return system.decayRate;
                case ParameterType.MaintenanceCostMultiplier:
                    return system.maintenanceCostMultiplier;
                case ParameterType.LaborConsumptionRate:
                    return system.laborConsumptionRate;
                default:
                    return 0f;
            }
        }
        
        public override string ToString()
        {
            return $"{parameter} {comparison} {thresholdValue}";
        }
    }
}