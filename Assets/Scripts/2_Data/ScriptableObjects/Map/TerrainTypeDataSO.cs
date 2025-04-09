using UnityEngine;

/// CLASS PURPOSE:
/// TerrainTypeDataSO defines the visual and gameplay properties of different terrain types
/// that can be applied to regions across the map, affecting movement, economics, and appearance.
///
/// CORE RESPONSIBILITIES:
/// - Store visual identifiers such as icons, textures, and base color
/// - Define terrain modifiers that impact economic sectors (agriculture, mining, etc.)
/// - Specify passability and natural border status to influence gameplay and AI logic
///
/// KEY COLLABORATORS:
/// - MapManager: Applies terrain visuals and mechanics to map regions
/// - RegionController: References terrain effects to calculate regional outputs and behaviors
/// - UIManager: Displays terrain data in tooltips, summaries, or overlays
///
/// CURRENT ARCHITECTURE NOTES:
/// - Uses scalar multipliers for sector-based bonuses
/// - Sector multipliers are accessed via string-based lookup method
///
/// REFACTORING SUGGESTIONS:
/// - Replace string-based lookup in GetMultiplierForSector with an enum-based system
/// - Consider grouping sector bonuses into a dictionary or serializable structure for scalability
///
/// EXTENSION OPPORTUNITIES:
/// - Add terrain-based movement cost or combat modifiers
/// - Support dynamic effects or seasonal interactions
/// - Integrate with event triggers for biome-specific events or bonuses

[CreateAssetMenu(fileName = "NewTerrainType", menuName = "Game Data/Terrain Type")]
public class TerrainTypeDataSO : ScriptableObject
{
    [Header("Basic Information")]
    public string terrainName;
    [TextArea(2, 4)]
    public string description;
    
    [Header("Visual Properties")]
    public Color baseColor = Color.white;
    public Sprite terrainIcon;
    public Sprite terrainTexture;
    
    [Header("Economic Properties")]
    [Tooltip("Bonus to food production (multiplier)")]
    [Range(0.1f, 3.0f)]
    public float fertilityMultiplier = 1.0f;
    
    [Tooltip("Bonus to resource extraction (multiplier)")]
    [Range(0.1f, 3.0f)]
    public float mineralsMultiplier = 1.0f;
    
    [Tooltip("Bonus to production (multiplier)")]
    [Range(0.1f, 3.0f)]
    public float productionMultiplier = 1.0f;
    
    [Tooltip("Bonus to commerce (multiplier)")]
    [Range(0.1f, 3.0f)]
    public float commerceMultiplier = 1.0f;
    
    [Header("Special Properties")]
    [Tooltip("Can units move through this terrain?")]
    public bool isPassable = true;
    
    [Tooltip("Does this terrain act as a natural border?")]
    public bool isNaturalBorder = false;
    
    [Tooltip("Chance of special resources appearing (0-1)")]
    [Range(0f, 1f)]
    public float specialResourceChance = 0.1f;
    
    [Tooltip("Any unique gameplay effects of this terrain")]
    [TextArea(2, 4)]
    public string specialEffects;
    
    // Helper method to get the economic multiplier for a specific sector
    public float GetMultiplierForSector(string sectorName)
    {
        switch (sectorName.ToLower())
        {
            case "agriculture":
                return fertilityMultiplier;
            case "mining":
                return mineralsMultiplier;
            case "industry":
                return productionMultiplier;
            case "commerce":
                return commerceMultiplier;
            default:
                return 1.0f;
        }
    }
}