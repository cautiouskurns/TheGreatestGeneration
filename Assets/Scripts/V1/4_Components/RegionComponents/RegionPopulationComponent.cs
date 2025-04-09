using UnityEngine;
using System.Collections.Generic;
using V1.Entities;


namespace V1.Components
{       
    /// CLASS PURPOSE:
    /// RegionPopulationComponent models the population dynamics within a region,
    /// including labor availability, satisfaction, and labor distribution across economic sectors.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Track population satisfaction and update it based on fulfilled needs
    /// - Simulate population growth or decline based on satisfaction thresholds
    /// - Manage labor allocation to sectors like agriculture, industry, and commerce
    ///
    /// KEY COLLABORATORS:
    /// - RegionEntity: Owns this component and provides context for economic and resource behavior
    /// - RegionEconomyComponent: Receives capital investment updates tied to satisfaction
    /// - ResourceComponent: Supplies satisfaction inputs from fulfilled population needs
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Simple scalar thresholds drive growth and decline logic
    /// - Satisfaction is averaged from input metrics and stored as a normalized float
    /// - Labor allocation is stored as normalized floats in a dictionary keyed by sector
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace string keys in laborAllocation with an enum or strongly typed structure
    /// - Decouple satisfaction logic into a service or strategy class for testing and reuse
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add demographic factors such as age distribution or education levels
    /// - Support sector-specific morale or productivity modifiers
    /// - Incorporate population migration between regions
    /// 
    public class RegionPopulationComponent
    {
        private RegionEntity region;
        
        // Population properties
        public int laborAvailable = 100;
        public float satisfaction = 0.7f;
        
        // Labor allocation
        public Dictionary<string, float> laborAllocation = new Dictionary<string, float>
        {
            { "agriculture", 0.6f },
            { "industry", 0.3f },
            { "commerce", 0.1f }
        };
        
        public RegionPopulationComponent(RegionEntity owner)
        {
            region = owner;
        }
        
        // Update satisfaction based on resource needs
        public void UpdateSatisfaction(Dictionary<string, float> needsSatisfaction)
        {
            float totalSatisfaction = 0f;
            int resourceCount = 0;
            
            foreach (var entry in needsSatisfaction)
            {
                totalSatisfaction += entry.Value;
                resourceCount++;
            }
            
            if (resourceCount > 0)
            {
                satisfaction = totalSatisfaction / resourceCount;
            }
        }
        
        // Update population based on satisfaction
        public void UpdatePopulation()
        {
            if (satisfaction < 0.5f)
            {
                // Low satisfaction leads to population decline
                //laborAvailable = Mathf.Max(50, laborAvailable - Mathf.RoundToInt((0.5f - satisfaction) * 10));
                laborAvailable = laborAvailable - Mathf.RoundToInt((0.5f - satisfaction) * 10);

            }
            else if (satisfaction > 0.8f)
            {
                // High satisfaction leads to population growth
                laborAvailable += Mathf.RoundToInt((satisfaction - 0.8f) * 15);
                
                // And reinvestment in capital
                region.economy.AdjustCapitalInvestment((satisfaction - 0.8f) * 0.5f);
            }
        }
        
        // Get labor allocation for a sector
        public float GetLaborAllocation(string sector)
        {
            if (laborAllocation.ContainsKey(sector))
            {
                return laborAllocation[sector];
            }
            
            return 0f;
        }
        
        // Set labor allocation for a sector
        public void SetLaborAllocation(string sector, float amount)
        {
            // Ensure the amount is valid
            amount = Mathf.Clamp01(amount);
            
            // Store the allocation
            laborAllocation[sector] = amount;
        }
    }
}