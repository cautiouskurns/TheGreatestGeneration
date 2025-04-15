using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles infrastructure maintenance and decay
    /// </summary>
    public class InfrastructureManager : EconomicSubsystem
    {
        public InfrastructureManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Infrastructure...");
            
            float maintenanceCost = region.Infrastructure.GetMaintenanceCost();
            
            // Apply maintenance cost to economy
            region.Economy.Wealth -= Mathf.RoundToInt(maintenanceCost);

            Debug.Log($"[Infrastructure] Level: {region.Infrastructure.Level}, Maintenance Cost: {maintenanceCost}");
        }
    }
}