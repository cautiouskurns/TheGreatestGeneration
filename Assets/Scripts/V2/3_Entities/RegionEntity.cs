using UnityEngine;
using V2.Components;
using V2.Managers;
using System.Collections.Generic;
using System.Linq;

namespace V2.Entities
{
    /// <summary>
    /// CLASS PURPOSE:
    /// RegionEntity represents a single economic unit in the simulation. It encapsulates basic
    /// economic state (wealth, production) and delegates resource and production behavior
    /// to internal components.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Maintain core simulation variables: wealth and production
    /// - Delegate resource generation and production processing to internal components
    /// - React to global turn-based events via `ProcessTurn`
    /// - Emit region updates using the EventBus
    ///
    /// KEY COLLABORATORS:
    /// - ResourceComponent: Manages raw resource generation and storage
    /// - ProductionComponent: Simulates transformation of resources into production output
    /// - EventBus: Sends notifications such as "RegionUpdated" for UI or other systems
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Designed for simplicity and testability in early-stage prototypes
    /// - Encapsulates internal systems to ensure isolation and modular growth
    /// - No dependencies on visual/UI systems to remain headless and simulation-driven
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Introduce a RegionConfig or RegionTemplate for dynamic instantiation
    /// - Add support for multiple economic subsystems (infrastructure, policies, traits)
    /// - Split economic logic into strategies for adjustable rulesets
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add meta-data like region size, terrain, owner nation, or name localization
    /// - Expose computed economic metrics like satisfaction, balance sheets, or risk levels
    /// - Support event history or logs for replay/debugging purposes
    /// </summary>
    public class RegionEntity
    {
        // Identity
        public string Name { get; set; }
        
        // Components
        public ResourceComponent Resources { get; private set; }
        public ProductionComponent Production { get; private set; }
        public RegionEconomyComponent Economy { get; set; }
        public InfrastructureComponent Infrastructure { get; set; }
        public PopulationComponent Population { get; set; }

        // Constructor with all parameters
        public RegionEntity(string name, int initialWealth, int initialProduction)
        {
            Name = name;
            
            // Initialize components
            Resources = new ResourceComponent();
            Production = new ProductionComponent(Resources);
            Economy = new RegionEconomyComponent(initialWealth, initialProduction);
            Infrastructure = new InfrastructureComponent();
            Population = new PopulationComponent();
        }

        // Additional constructor with default values
        public RegionEntity(string name) : this(name, 100, 50)
        {
            // This calls the main constructor with default values
            // Default wealth: 100
            // Default production: 50
        }

        public void ProcessTurn()
        {
            Debug.Log($"[Region: {Name}] Processing Turn...");
            
            // Process components
            Resources.GenerateResources();
            
            // Instead of using the hardcoded production value, we'll let the EconomicSystem handle it
            // The Production.ProcessProduction() call is kept for backward compatibility and resource consumption
            Production.ProcessProduction();
            
            // We'll update the Economy component in EconomicSystem.ManualTick() using the Cobb-Douglas function
            
            // Notify systems
            EventBus.Trigger("RegionUpdated", this);
        }

        public string GetSummary()
        {
            return $"[{Name}] Wealth: {Economy.Wealth}, Production: {Economy.Production}, Resources: {Resources.GetResourceOverview()}";
        }

    }
}