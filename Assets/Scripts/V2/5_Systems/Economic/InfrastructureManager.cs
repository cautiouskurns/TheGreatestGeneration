using UnityEngine;
using V2.Entities;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles infrastructure maintenance, decay and costs
    /// </summary>
    public class InfrastructureManager : EconomicSubsystem
    {
        public InfrastructureManager(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Infrastructure...");

            // Get parameters from the economic system
            float decayRate = economicSystem.decayRate;
            float maintenanceCostMultiplier = economicSystem.maintenanceCostMultiplier;

            int infrastructureLevel = region.Infrastructure.Level;
            
            // Calculate infrastructure decay amount
            float decayAmount = infrastructureLevel * decayRate;
            
            // Calculate maintenance cost based on infrastructure level
            int maintenanceCost = Mathf.RoundToInt(infrastructureLevel * maintenanceCostMultiplier);
            
            // Deduct maintenance cost from wealth
            region.Economy.Wealth = Mathf.Max(0, region.Economy.Wealth - maintenanceCost);
            
            // We could also add code here to actually decay infrastructure level if it's not maintained,
            // but that would require an additional method in InfrastructureComponent to reduce level
            
            Debug.Log($"[Infrastructure] Level: {infrastructureLevel}, Decay Rate: {decayRate:F3}, " +
                     $"Maintenance Cost: {maintenanceCost}, Decay Amount: {decayAmount:F2}");
        }
    }
}