using UnityEngine;
using System.Collections.Generic;
using V1.Entities;

namespace V1.Components
{
        
    /// CLASS PURPOSE:
    /// RegionEconomyComponent manages the economic state of a region, including its wealth,
    /// production, efficiency, and capital investment, and simulates how these values evolve over time.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Track and update core economic indicators: wealth, production, efficiency
    /// - Apply changes based on satisfaction and capital investment
    /// - Compute economic changes using production and resource effects
    /// - Track and expose deltas for UI feedback or external systems
    ///
    /// KEY COLLABORATORS:
    /// - RegionEntity: Hosts this component and manages per-turn updates
    /// - ResourceComponent: Supplies resource balances used in economic calculations
    /// - UI systems: Access delta values for display and summaries
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Production and wealth are updated independently but tracked together
    /// - Satisfaction modifies wealth based on morale thresholds
    /// - Resource influence is applied as a net wealth modifier
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Encapsulate satisfaction thresholds and scaling in configurable profiles
    /// - Consider extracting economic logic into services for testability and reuse
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Integrate policy or tech modifiers to affect economic performance
    /// - Track historical trends in economic indicators
    /// - Add support for regional specialization or sector-specific economies
    /// 
    public class RegionEconomyComponent
    {
        private RegionEntity region;
        
        // Economic properties
        public int wealth;
        public int production;
        public float productionEfficiency = 1.0f;
        public float capitalInvestment = 10.0f;

        public float infrastructureLevel = 1.0f;
        
        // Change tracking
        public int wealthDelta = 0;
        public int productionDelta = 0;
        
        public RegionEconomyComponent(RegionEntity owner, int initialWealth, int initialProduction)
        {
            region = owner;
            wealth = initialWealth;
            production = initialProduction;
        }
        
        // Apply economy changes
        public void ApplyChanges(int wealthChange, int productionChange)
        {
            wealth += wealthChange;
            production += productionChange;
            
            // Track changes for UI
            wealthDelta = wealthChange;
            productionDelta = productionChange;
        }
        
        // Apply satisfaction effects to economy
        public void ApplySatisfactionEffects(float satisfaction)
        {
            if (satisfaction < 0.5f)
            {
                // Low satisfaction leads to wealth loss
                int wealthLoss = Mathf.RoundToInt((0.5f - satisfaction) * 20);
                wealth -= wealthLoss;
                wealthDelta -= wealthLoss;
            }
        }
        
        // Adjust capital investment
        public void AdjustCapitalInvestment(float amount)
        {
            capitalInvestment += amount;
        }
        
        // Reset change tracking flags
        public void ResetChangeFlags()
        {
            wealthDelta = 0;
            productionDelta = 0;
        }

        // Add these methods to RegionEconomyComponent
        public int CalculateBaseWealthChange()
        {
            return production * 2;  // Your original formula
        }

        public int CalculateBaseProductionChange()
        {
            return Random.Range(-2, 3);  // Your original formula
        }

        // Add this for resource effects
        public int CalculateResourceEffect(Dictionary<string, float> resourceBalance)
        {
            // Calculate wealth effect based on resource surpluses and shortages
            int effect = 0;
            
            foreach (var entry in resourceBalance)
            {
                // Positive balance increases wealth, negative decreases
                effect += Mathf.RoundToInt(entry.Value);
            }
            
            return effect;
        }
    }
}