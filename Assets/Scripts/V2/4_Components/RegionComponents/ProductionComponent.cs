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
        private ResourceComponent resources;
        private int output = 10; // Default output
        
        public ProductionComponent(ResourceComponent resources)
        {
            this.resources = resources;
        }

        public void ProcessProduction()
        {
            Debug.Log("Processing production...");
            
            // Basic resource consumption
            resources.UseResource("Food", 2);
            resources.UseResource("Wood", 1);
            
            // Calculate output
            output = 10; // Simplified for now
            
            Debug.Log($"Production completed: output = {output}");
        }

        public int GetOutput()
        {
            return output;
        }
        
        /// <summary>
        /// Sets the production output value
        /// </summary>
        /// <param name="newOutput">The new production output value</param>
        public void SetOutput(int newOutput)
        {
            // Only update and log if value actually changes
            if (newOutput != output)
            {
                int oldOutput = output;
                output = newOutput;
                Debug.Log($"Production output updated: {oldOutput} → {newOutput}");
            }
        }
    }
}