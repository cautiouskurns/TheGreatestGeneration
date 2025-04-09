/// CLASS PURPOSE:
/// ProjectDataSO defines the blueprint for infrastructure or development projects
/// that regions can initiate. It includes cost, duration, targeted improvements, and
/// optional special effects. Used to configure and balance development options.
///
/// CORE RESPONSIBILITIES:
/// - Define basic metadata for a project (name, description, icon)
/// - Store costs in resources (wealth, materials, labor)
/// - Specify what the project improves (sector and/or infrastructure)
/// - Indicate if the project has any narrative or gameplay special effects
///
/// KEY COLLABORATORS:
/// - ProjectSystem: Processes the application of the project's effects
/// - RegionEntity: Tracks which projects are active or completed
/// - UI Components: Display project info, cost, and icons to the player
///
/// CURRENT ARCHITECTURE NOTES:
/// - Sector and infrastructure targets are represented by strings
/// - Costs are stored directly as integers (consider resource type refactoring)
/// - Optional visual and descriptive fields enhance UI representation
///
/// REFACTORING SUGGESTIONS:
/// - Replace string fields for sector/infrastructure with enums or SO references
/// - Move special effects into a modular system or callback structure
/// - Normalize cost and duration into reusable balance profiles
///
/// EXTENSION OPPORTUNITIES:
/// - Add prerequisites or tech unlocks for project availability
/// - Introduce project categories or types for sorting and filtering
/// - Support dynamic project effects or runtime customization

using UnityEngine;

[CreateAssetMenu(fileName = "NewProject", menuName = "Game/Project")]
public class ProjectDataSO : ScriptableObject
{
    public string projectName;
    public string description;
    [Range(1, 10)] public int turnsToComplete;
    
    // Resource costs
    public int wealthCost;
    public int materialsCost;
    public int laborCost;
    
    // Target improvements
    public string targetSector; // "Agriculture", "Industry", etc.
    public string targetInfrastructure; // "Economic", "Transportation", etc.
    public int sectorBonus;
    public int infrastructureBonus;
    
    // Special effects
    public bool hasSpecialEffects;
    [TextArea(2, 4)] public string specialEffectDescription;
    
    // Visual representation
    public Sprite projectIcon;
    public bool showConstructionIndicator = true;
}