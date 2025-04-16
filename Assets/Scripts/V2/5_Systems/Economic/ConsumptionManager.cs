using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles consumption of resources by population and industry
    /// </summary>
    public class ConsumptionManager : EconomicSubsystem
    {
        public ConsumptionManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Consumption...");
            
            // Get labor consumption rate from the economic system
            float laborConsumptionRate = economicSystem.laborConsumptionRate;
            
            int laborAvailable = region.Population.LaborAvailable;
            
            // Calculate total consumption based on population size and consumption rate
            int totalConsumption = Mathf.RoundToInt(laborAvailable * laborConsumptionRate);
            
            // Calculate satisfaction based on production vs. consumption ratio
            float productionConsumptionRatio = (float)region.Economy.Production / Mathf.Max(1, totalConsumption);
            float satisfaction = Mathf.Clamp01(productionConsumptionRatio);
            
            // Update population satisfaction
            region.Population.UpdateSatisfaction(satisfaction);
            
            Debug.Log($"[Consumption] Labor: {laborAvailable}, Rate: {laborConsumptionRate:F2}, " +
                     $"Total Consumption: {totalConsumption}, Production/Consumption Ratio: {productionConsumptionRatio:F2}, " +
                     $"Satisfaction: {satisfaction:F2}");
        }
    }
}