using UnityEngine;

/// <summary>
/// Defines properties for different terrain types that can exist in regions
/// </summary>
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