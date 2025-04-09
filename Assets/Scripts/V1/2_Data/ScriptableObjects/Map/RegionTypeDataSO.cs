using UnityEngine;

namespace V1.Data
{

    /// CLASS PURPOSE:
    /// RegionTypeDataSO defines the archetype of a region by specifying its economic
    /// and visual characteristics. It is used to inform gameplay mechanics and map visuals.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Represent potential resource strengths across different economic sectors
    /// - Provide color and icon references for map representation
    /// - Offer designer-readable descriptions for use in UI or tooltips
    ///
    /// KEY COLLABORATORS:
    /// - MapManager: Applies mapColor and regionIcon for visual rendering
    /// - RegionEntity or RegionController: Uses economic potentials to simulate production and development
    /// - UIManager: Displays region type data in tooltips or summaries
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - All fields are exposed for easy balancing and tuning by designers
    /// - Uses simple int-based potential ranges (0–10) to represent economic capabilities
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace raw sector potential fields with a dictionary or modular data structure
    /// - Add derived values or calculated modifiers for more nuanced gameplay mechanics
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Include terrain tags or environmental modifiers (e.g., coastal, mountainous)
    /// - Add support for regional bonuses or penalties based on tech, policies, or events
    /// - Integrate with procedural generation for dynamic region creation

    [CreateAssetMenu(fileName = "NewRegionType", menuName = "Game/Region Type")]
    public class RegionTypeDataSO : ScriptableObject
    {
        public string regionTypeName;
        public Color mapColor;
        [Range(0, 10)] public int agriculturePotential;
        [Range(0, 10)] public int industryPotential;
        [Range(0, 10)] public int commercePotential;
        [Range(0, 10)] public int miningPotential;
        [Range(0, 10)] public int researchPotential;
        public Sprite regionIcon;
        [TextArea(3, 5)] public string description;
    }
}