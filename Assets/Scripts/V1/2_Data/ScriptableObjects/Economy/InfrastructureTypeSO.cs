using UnityEngine;
using System.Collections.Generic;

namespace V1.Data
{

    /// CLASS PURPOSE:
    /// InfrastructureTypeDataSO defines the static configuration for a type of infrastructure
    /// available for regional development. Each infrastructure type can have multiple levels,
    /// each providing different economic effects and descriptive flavor.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Define name, icon, and description for an infrastructure type
    /// - Configure level-based sector bonuses and development details
    /// - Provide cost and time settings for project implementation
    ///
    /// KEY COLLABORATORS:
    /// - ProjectSystem: Uses this data when applying infrastructure upgrades
    /// - RegionEntity: Applies bonuses based on the current level of this infrastructure
    /// - UI Components: Display name, icon, and descriptions to the player
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses a serializable LevelData class for multi-tier bonuses
    /// - Sector bonuses are stored in a Dictionary per level
    /// - Cost progression is controlled via a base value and multiplier
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace Dictionary<string, float> with a typed data structure for validation
    /// - Add methods for retrieving bonuses or costs by level
    /// - Include tags or categories for UI filtering and sorting
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Introduce infrastructure-specific effects beyond sector bonuses
    /// - Support unlock conditions or prerequisites per level
    /// - Add historical or narrative context to each level

    [CreateAssetMenu(fileName = "NewInfrastructureType", menuName = "Game/Infrastructure Type")]
    public class InfrastructureTypeDataSO : ScriptableObject
    {
        public string typeName;
        public Sprite icon;
        [TextArea(2, 4)] public string description;
        
        // Economic impacts per level
        [System.Serializable]
        public class LevelData
        {
            public int level;
            public string levelName;
            [TextArea(1, 2)] public string levelDescription;
            public Dictionary<string, float> sectorBonuses = new Dictionary<string, float>();
        }
        
        public LevelData[] levels;
        
        // Requirements for development
        public int baseCost;
        public float costMultiplierPerLevel = 1.5f;
        public int baseTurnsToComplete;
    }
}