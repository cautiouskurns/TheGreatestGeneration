using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles population consumption and satisfaction calculations
    /// </summary>
    public class ConsumptionManager : EconomicSubsystem
    {
        public ConsumptionManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Population Consumption...");
            
            float consumption = region.Population.LaborAvailable * 1.5f;
            float unmetDemand = Mathf.Max(0, consumption - region.Economy.Production);
            float satisfaction = 1.0f;
            
            if (consumption > 0)
            {
                float unrestFactor = unmetDemand / consumption;
                satisfaction = Mathf.Clamp01(1.0f - unrestFactor);
            }

            // Update population satisfaction
            region.Population.UpdateSatisfaction(satisfaction);
            
            Debug.Log($"[Consumption] Total: {consumption}, Unmet: {unmetDemand}, Satisfaction: {satisfaction:F2}");
        }
    }
}