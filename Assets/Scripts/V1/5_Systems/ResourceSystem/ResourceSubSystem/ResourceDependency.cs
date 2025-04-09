using UnityEngine;

namespace V1.Systems
{
    /// CLASS PURPOSE:
    /// ResourceDependency defines a non-consumable secondary requirement that must
    /// be present for certain production processes to function efficiently.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Specify the name and required amount of a dependency resource
    /// - Indicate whether it affects efficiency and to what extent
    /// - Provide a human-readable description for design or UI clarity
    ///
    /// KEY COLLABORATORS:
    /// - ProductionComponent: Checks for presence of dependencies when calculating output efficiency
    /// - ResourceComponent: Tracks quantities of available resources in the region
    /// - ResourceProductionRecipe: May reference these to define full production conditions
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses linear efficiency weighting with adjustable impact factor
    /// - Serialized for easy use in lists and editors inside ScriptableObjects or MonoBehaviours
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace string resourceName with a reference to ResourceDataSO for better type safety
    /// - Add validation logic to ensure requiredAmount and impactWeight are within valid ranges
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Support dynamic conditions (e.g., seasonal or event-driven dependencies)
    /// - Add optional modifiers or benefits granted by the dependency
    /// - Enable partial efficiency bonuses from partially fulfilled dependencies
    /// 
    /// <summary>
    /// Represents a secondary dependency for production that isn't consumed but needed
    /// </summary>
    [System.Serializable]
    public class ResourceDependency
    {
        public string resourceName;
        public float requiredAmount = 1.0f;
        public bool affectsEfficiency = true;
        [Range(0f, 1f)]
        public float impactWeight = 1.0f; // How much this affects efficiency (1.0 = linear)
        
        [TextArea(1, 2)]
        public string description;
    }
}