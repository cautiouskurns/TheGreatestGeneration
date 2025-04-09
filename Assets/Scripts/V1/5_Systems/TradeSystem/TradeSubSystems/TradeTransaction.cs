using UnityEngine;
using V1.Entities;

namespace V1.Systems
{ 
    /// CLASS PURPOSE:
    /// TradeTransaction models a single trade event between two regions, capturing
    /// the movement of resources and wealth as part of the game's economic simulation.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Store source (exporter) and destination (importer) region references
    /// - Define the resource being traded and amounts before and after efficiency
    /// - Execute the transaction, transferring resources and updating regional wealth
    ///
    /// KEY COLLABORATORS:
    /// - RegionEntity: Supplies and receives traded resources, and tracks wealth
    /// - ResourceComponent: Called internally to add/remove resources per region
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Wealth distribution is hardcoded with simple ratio logic
    /// - Trade efficiency is applied before transfer via ReceivedAmount
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Abstract trade value calculation to allow more complex pricing or modifiers
    /// - Add error handling or validation for available resources
    /// - Consider event dispatching or transaction logging after execution
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Track transaction timestamps, trade reasons, or associated policies
    /// - Enable partial fulfillment when exporter lacks full amount
    /// - Add directional or player-controlled modifiers to affect trade dynamics
    /// 
    public class TradeTransaction
    {
        public RegionEntity Exporter { get; set; }
        public RegionEntity Importer { get; set; }
        public string ResourceName { get; set; }
        public float Amount { get; set; }
        public float ReceivedAmount { get; set; } // After efficiency applied
        
        // Execute this trade (transfer resources and generate wealth)
        public void Execute()
        {
            // Transfer resources
            Exporter.resources.RemoveResource(ResourceName, Amount);
            Importer.resources.AddResource(ResourceName, ReceivedAmount);
            
            // Generate wealth for both parties based on trade
            float tradeValue = ReceivedAmount * 0.5f; // Simple wealth generation from trade
            Exporter.wealth += Mathf.RoundToInt(tradeValue);
            Importer.wealth += Mathf.RoundToInt(tradeValue * 0.5f); // Importer gets some wealth too
        }
    }
}