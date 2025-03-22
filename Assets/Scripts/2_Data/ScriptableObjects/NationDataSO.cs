using UnityEngine;

/// <summary>
/// Represents a nation template with its properties, regions, and starting conditions.
/// </summary>
[CreateAssetMenu(fileName = "NewNation", menuName = "Game Data/Nation")]
public class NationDataSO : ScriptableObject
{
    [Header("Basic Information")]
    public string nationName;
    public Color nationColor;
    public Sprite nationFlag;
    [TextArea(3, 5)] 
    public string description;
    
    [Header("Regions")]
    [Tooltip("The regions controlled by this nation")]
    public RegionData[] regions;
    
    [Header("Starting Resources")]
    public int initialWealth = 100;
    public int initialFood = 50;
    public int initialMaterials = 50;
    
    [Header("National Traits")]
    public NationalTrait[] nationalTraits;
    
    /// <summary>
    /// Represents a single region belonging to this nation
    /// </summary>
    [System.Serializable]
    public class RegionData
    {
        public string regionName;
        
        // Changed from RegionTypeSO (or RegionTypeDataSO) to string to avoid circular dependency
        // We'll look up the RegionTypeDataSO by name when building RegionEntity objects
        [Tooltip("Name of the region type (plains, mountains, etc.)")]
        public string regionTypeName;
        
        public int initialWealth;
        public int initialProduction;
        
        [Header("Resource Production")]
        [Tooltip("Base food production per turn")]
        public int foodProduction = 10;
        [Tooltip("Base material production per turn")]
        public int materialProduction = 8;
    }
    
    /// <summary>
    /// Represents a special trait or characteristic of the nation
    /// </summary>
    [System.Serializable]
    public class NationalTrait
    {
        public string traitName;
        [TextArea(2, 3)] 
        public string description;
        
        [Tooltip("Which sector gets affected by this trait")]
        public string affectedSector;
        
        [Tooltip("Multiplicative modifier (1.0 = no change)")]
        [Range(0.5f, 2.0f)]
        public float sectorModifier = 1.0f;
    }
}