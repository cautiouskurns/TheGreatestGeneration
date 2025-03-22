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