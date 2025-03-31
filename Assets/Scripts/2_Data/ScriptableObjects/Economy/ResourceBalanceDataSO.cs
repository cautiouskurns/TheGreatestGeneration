using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ResourceBalance", menuName = "Game/Resource Balance")]
public class ResourceBalanceDataSO : ScriptableObject
{
    [Header("Production Rates")]
    public TerrainResourceModifier[] terrainModifiers;
    
    [Header("Consumption Rates")]
    public PopulationConsumptionRate[] populationConsumption;
    
    [Header("Infrastructure Scaling")]
    public InfrastructureScalingFactor[] infrastructureScaling;
    
    [Header("Global Settings")]
    public float baseProductionMultiplier = 1.0f;
    public float baseConsumptionMultiplier = 1.0f;
    public float storageEfficiencyFactor = 1.0f; // How efficiently resources are stored
    public float transportationBaseEfficiency = 1.0f; // Base efficiency for transportation
}

// How terrain affects resource production
[System.Serializable]
public class TerrainResourceModifier
{
    public string terrainTypeName; // Name of terrain type
    public ResourceProductionModifier[] resourceModifiers;
}

// How population consumes resources
[System.Serializable]
public class PopulationConsumptionRate
{
    public string populationClassName; // E.g., "Peasants", "Merchants", "Nobles"
    public ResourceConsumptionRate[] resourceConsumption;
}

// How infrastructure scales resource production
[System.Serializable]
public class InfrastructureScalingFactor
{
    public string infrastructureType;
    public AnimationCurve scalingCurve = AnimationCurve.Linear(0, 1, 5, 3); // Default: level 0 = 1x, level 5 = 3x
    public ResourceTypeModifier[] resourceTypeModifiers;
}

// Modifier for specific resource production
[System.Serializable]
public class ResourceProductionModifier
{
    public ResourceDataSO.ResourceType resourceType;
    [Range(0.1f, 5.0f)] public float productionMultiplier = 1.0f;
}

// Consumption rate for a specific resource
[System.Serializable]
public class ResourceConsumptionRate
{
    public ResourceDataSO resource;
    public float baseConsumptionPerCapita = 1.0f;
    public bool isEssential = false; // If true, shortage causes penalties
}

// How infrastructure affects different resource types
[System.Serializable]
public class ResourceTypeModifier
{
    public ResourceDataSO.ResourceType resourceType;
    public float additionalMultiplier = 1.0f;
}