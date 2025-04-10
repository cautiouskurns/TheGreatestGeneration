using UnityEngine;
using System.Linq;
using V2.Components;

namespace V2.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// ProductionComponent models the transformation of raw resources into abstracted
    /// economic outputs such as goods, services, or infrastructure.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Access and potentially modify resource data from ResourceComponent
    /// - Simulate the production process during each turn
    /// - Provide utility methods for summarizing the current production context
    ///
    /// KEY COLLABORATORS:
    /// - RegionEntity: Owns this component and triggers it during each turn
    /// - ResourceComponent: Provides the input resources for production logic
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Currently acts as a stub with placeholder logic for production
    /// - Uses resource overview as a way to debug input/output states
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Implement actual consumption and transformation logic of input resources
    /// - Allow for different production recipes based on regional traits or tech levels
    /// - Abstract production formulas into configurable data structures or strategies
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add production queues, efficiency modifiers, or prioritization schemes
    /// - Emit production-related events or hooks for dependent systems
    /// - Integrate with infrastructure, workforce, or policy systems for depth
    /// </summary>
    public class ProductionComponent
    {
        private ResourceComponent resourceComponent;

        /// <summary>
        /// Constructs the ProductionComponent with a dependency on the owning region's resources.
        /// </summary>
        public ProductionComponent(ResourceComponent resourceComponent)
        {
            this.resourceComponent = resourceComponent;
        }

        /// <summary>
        /// Simulates a single turn of production. Currently a placeholder for future logic.
        /// </summary>
        public void ProcessProduction()
        {
            Debug.Log("ProductionComponent: processing production...");
            // Future: consume resources, create goods, apply modifiers
        }

        /// <summary>
        /// Returns a formatted string showing available resource inputs for production.
        /// </summary>
        public string GetResourceOverview()
        {
            var resources = resourceComponent.GetAllResources();
            return string.Join(", ", resources.Select(kv => $"{kv.Key}: {kv.Value:F1}"));
        }
    }
}