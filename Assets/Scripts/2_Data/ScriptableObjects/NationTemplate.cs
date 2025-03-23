using UnityEngine;

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