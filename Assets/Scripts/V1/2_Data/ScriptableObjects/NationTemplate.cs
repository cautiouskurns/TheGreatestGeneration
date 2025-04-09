using UnityEngine;

namespace V1.Data
{
    /// CLASS PURPOSE:
    /// NationTemplate defines the foundational parameters and preferences for procedurally
    /// generating a nation on the map, including its location, terrain affinity, and economic profile.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Define color, name, and map placement of the nation
    /// - Influence region generation via terrain and expansion preferences
    /// - Provide base multipliers for initializing wealth and production
    ///
    /// KEY COLLABORATORS:
    /// - NationGenerator or MapGenerator: Uses these templates to instantiate nations procedurally
    /// - TerrainMapDataSO and TerrainTypeDataSO: Help determine expansion feasibility and terrain scoring
    /// - EconomyInitializer: Applies wealth and production multipliers during setup
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Encapsulated as a serializable class for use in ScriptableObjects or generation systems
    /// - All parameters are normalized and designer-tunable via Unity Inspector
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Move terrain preferences into a dictionary or data-driven system for flexibility
    /// - Consider encapsulating logic for scoring tiles into a utility class
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Include cultural or ideological tags for flavor and gameplay interaction
    /// - Add dynamic weighting for seasonal or event-based expansion logic
    /// - Support faction-specific biases or regional uniqueness
    /// 
    [System.Serializable]
    public class NationTemplate
    {
        public string name;
        public Color color;
        
        [Header("Position")]
        [Tooltip("Normalized position (0-1) for nation center")]
        public Vector2 centerPosition;
        
        [Header("Expansion")]
        [Tooltip("How far from center the nation will try to expand (0-1)")]
        [Range(0.1f, 1.0f)]
        public float expansionRadius = 0.5f;
        
        [Header("Terrain Preferences")]
        [Tooltip("Nation will avoid expanding into water")]
        public bool isLandlocked = false;
        
        [Tooltip("Nation will prefer expanding into mountains")]
        public bool isMountainous = false;
        
        [Tooltip("Nation's preference for specific terrain types (0-1)")]
        [Range(0, 1)]
        public float forestPreference = 0.5f;
        
        [Range(0, 1)]
        public float desertPreference = 0.5f;
        
        [Range(0, 1)]
        public float plainsPreference = 0.5f;
        
        [Header("Economy")]
        [Tooltip("Base multiplier for initial wealth")]
        [Range(0.5f, 2.0f)]
        public float wealthMultiplier = 1.0f;
        
        [Tooltip("Base multiplier for initial production")]
        [Range(0.5f, 2.0f)]
        public float productionMultiplier = 1.0f;
    }
}