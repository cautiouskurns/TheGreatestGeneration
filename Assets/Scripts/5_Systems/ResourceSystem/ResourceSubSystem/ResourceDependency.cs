using UnityEngine;

/// <summary>
/// Represents a secondary dependency for production that isn't consumed but needed
/// </summary>
[System.Serializable]
public class ResourceDependency
{
    public string resourceName;
    public float requiredAmount = 1.0f;
    public bool affectsEfficiency = true;
    [Range(0f, 1f)]
    public float impactWeight = 1.0f; // How much this affects efficiency (1.0 = linear)
    
    [TextArea(1, 2)]
    public string description;
}