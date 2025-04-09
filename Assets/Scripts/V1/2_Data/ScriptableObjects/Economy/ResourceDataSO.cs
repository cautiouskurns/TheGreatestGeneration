using UnityEngine;
using System.Collections.Generic;
using V1.Systems;
using V1.Data;

namespace V1.Data
{

    /// CLASS PURPOSE:
    /// ResourceDataSO defines a single resource type used throughout the game economy.
    /// It includes visual details, economic properties, production mechanics, and categorization,
    /// enabling both simulation systems and UI layers to interact with a standardized data format.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Define metadata, visual, and economic properties of a resource
    /// - Store production rules for converting inputs to this resource
    /// - Distinguish resources by type (material, food, wealth, etc.) and category (primary, secondary, etc.)
    ///
    /// KEY COLLABORATORS:
    /// - EconomicSystem: Uses this data for calculating production and trade behavior
    /// - ResourceBalanceDataSO: Adjusts resource interactions based on balance configuration
    /// - UI Systems: Display resource icons, names, and descriptions
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Production recipes support input dependencies, infrastructure requirements, and efficiency scaling
    /// - Transport and storage stats are included for logistical modeling
    /// - Quality factors are optional and only affect output if enabled
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Consider replacing infrastructure string references with SOs or enums
    /// - Allow recipes to support multiple output types or byproducts
    /// - Validate recipes on load for missing dependencies or invalid inputs
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Introduce tiered quality levels per resource
    /// - Support seasonal or terrain-based modifiers
    /// - Link resources to narrative or event triggers for dynamic simulation


    [CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource")]
    public class ResourceDataSO : ScriptableObject
    {
        [Header("Basic Information")]
        public string resourceName;
        public ResourceType resourceType;
        public Sprite resourceIcon;
        public Color resourceColor = Color.white;
        [TextArea(2, 4)] public string description;
        
        [Header("Resource Category")]
        public ResourceCategory category;
        
        [Header("Economic Properties")]
        public float baseValue = 10f;
        public float marketVolatility = 1f; // How much the price fluctuates (1 = normal)
        
        [Header("Storage Properties")]
        public float maxStoragePerCapacity = 100f; // How much can be stored per unit of storage capacity
        public float perishRate = 0f; // How much is lost per turn (0 = non-perishable)
        
        [Header("Transportation Properties")]
        [Range(0.1f, 2.0f)] public float transportFactor = 1.0f; // How easily it can be transported (higher = easier)
        public float weightPerUnit = 1.0f; // For transportation calculations
        
        [Header("Production Chain")]
        public bool isRawResource = true; // If true, can be produced without input resources
        public ResourceProductionRecipe[] productionRecipes; // How this resource is produced
        
        public enum ResourceType
        {
            Material,   // Physical goods
            Food,       // Edible resources
            Wealth,     // Money and valuables
            Labor,      // Work capacity
            Research,   // Knowledge and innovation
            Culture,    // Cultural and social resources
            Military    // Military resources
        }
        
        public enum ResourceCategory
        {
            Primary,    // Raw resources extracted directly from the land
            Secondary,  // Processed resources requiring primary inputs
            Tertiary,   // Complex goods and services
            Abstract    // Non-physical resources like wealth or research
        }
    }

    // Class to define production recipes
    [System.Serializable]
    public class ResourceProductionRecipe
    {
        public string recipeName; // Optional name for the recipe
        public ResourceInput[] inputs; // Required resources to produce
        public float outputAmount = 1.0f; // How much is produced per production cycle
        public float productionTime = 1.0f; // How many turns it takes to complete
        public string requiredInfrastructureType; // What infrastructure is needed
        public int minimumInfrastructureLevel = 1; // Minimum level required
        
        // Add dependencies that affect production but aren't directly consumed
        public ResourceDependency[] dependencies;
        
        // Efficiency scaling based on technology/skill
        [Range(0.5f, 2.0f)] public float efficiencyMultiplier = 1.0f;
        
        // Optional output scaling based on inputs quality
        public bool qualityAffectsOutput = false;
        [Range(0.1f, 0.5f)] public float qualityImpact = 0.2f;
    }

    // Class for resource inputs in recipes
    [System.Serializable]
    public class ResourceInput
    {
        public ResourceDataSO resource;
        public float amount = 1.0f;
        public bool consumed = true; // If false, the resource is used but not consumed (like tools)
    }
}