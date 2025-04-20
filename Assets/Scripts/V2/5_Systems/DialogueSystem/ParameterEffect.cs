using UnityEngine;
using V2.Systems;
using V2.Entities;

namespace V2.Systems.DialogueSystem
{
    [System.Serializable]
    public class ParameterEffect
    {
        public enum EffectTarget
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
        
        public enum EffectType
        {
            Add,           // Add a fixed value
            Multiply,      // Multiply by a factor
            Set            // Set to a specific value
        }
        
        public EffectTarget target;
        public EffectType effectType;
        public float value;
        public string description;
        
        // Apply the effect to the economic system
        public void Apply(EconomicSystem system, RegionEntity region)
        {
            if (system == null || region == null)
                return;
            
            // Get the current value
            float currentValue = GetCurrentValue(target, system, region);
            float newValue = currentValue;
            
            // Apply the modification
            switch (effectType)
            {
                case EffectType.Add:
                    newValue = currentValue + value;
                    break;
                case EffectType.Multiply:
                    newValue = currentValue * value;
                    break;
                case EffectType.Set:
                    newValue = value;
                    break;
            }
            
            // Apply the new value
            SetParameterValue(target, newValue, system, region);
            
            Debug.Log($"Applied economic effect: {target} {effectType} {value} => {newValue}");
        }
        
        // Get the current value of the target parameter
        private float GetCurrentValue(EffectTarget target, EconomicSystem system, RegionEntity region)
        {
            switch (target)
            {
                // Region entity parameters
                case EffectTarget.Wealth:
                    return region.Economy.Wealth;
                case EffectTarget.Production:
                    return region.Economy.Production;
                case EffectTarget.LaborAvailable:
                    return region.Population.LaborAvailable;
                case EffectTarget.InfrastructureLevel:
                    return region.Infrastructure.Level;
                case EffectTarget.PopulationSatisfaction:
                    return region.Population.Satisfaction;
                
                // Economic system parameters
                case EffectTarget.ProductivityFactor:
                    return system.productivityFactor;
                case EffectTarget.LaborElasticity:
                    return system.laborElasticity;
                case EffectTarget.CapitalElasticity:
                    return system.capitalElasticity;
                case EffectTarget.CycleMultiplier:
                    return system.cycleMultiplier;
                case EffectTarget.WealthGrowthRate:
                    return system.wealthGrowthRate;
                case EffectTarget.PriceVolatility:
                    return system.priceVolatility;
                case EffectTarget.DecayRate:
                    return system.decayRate;
                case EffectTarget.MaintenanceCostMultiplier:
                    return system.maintenanceCostMultiplier;
                case EffectTarget.LaborConsumptionRate:
                    return system.laborConsumptionRate;
                default:
                    return 0f;
            }
        }
        
        // Set the new value for the parameter
        private void SetParameterValue(EffectTarget target, float newValue, EconomicSystem system, RegionEntity region)
        {
            switch (target)
            {
                // Region entity parameters
                case EffectTarget.Wealth:
                    region.Economy.Wealth = Mathf.RoundToInt(newValue);
                    break;
                case EffectTarget.Production:
                    region.Economy.Production = Mathf.RoundToInt(newValue);
                    region.Production.SetOutput(Mathf.RoundToInt(newValue));
                    break;
                case EffectTarget.LaborAvailable:
                    int laborDelta = Mathf.RoundToInt(newValue) - region.Population.LaborAvailable;
                    region.Population.UpdateLabor(laborDelta);
                    break;
                case EffectTarget.InfrastructureLevel:
                    // For infrastructure level, we need to use specific methods or reflection
                    int levels = Mathf.RoundToInt(newValue - region.Infrastructure.Level);
                    for (int i = 0; i < levels; i++)
                    {
                        region.Infrastructure.Upgrade();
                    }
                    break;
                case EffectTarget.PopulationSatisfaction:
                    region.Population.UpdateSatisfaction(Mathf.Clamp01(newValue));
                    break;
                
                // Economic system parameters
                case EffectTarget.ProductivityFactor:
                    system.productivityFactor = newValue;
                    break;
                case EffectTarget.LaborElasticity:
                    system.laborElasticity = newValue;
                    break;
                case EffectTarget.CapitalElasticity:
                    system.capitalElasticity = newValue;
                    break;
                case EffectTarget.CycleMultiplier:
                    system.cycleMultiplier = newValue;
                    break;
                case EffectTarget.WealthGrowthRate:
                    system.wealthGrowthRate = newValue;
                    break;
                case EffectTarget.PriceVolatility:
                    system.priceVolatility = newValue;
                    break;
                case EffectTarget.DecayRate:
                    system.decayRate = newValue;
                    break;
                case EffectTarget.MaintenanceCostMultiplier:
                    system.maintenanceCostMultiplier = newValue;
                    break;
                case EffectTarget.LaborConsumptionRate:
                    system.laborConsumptionRate = newValue;
                    break;
            }
        }
    }
}