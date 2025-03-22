using UnityEngine;

[CreateAssetMenu(fileName = "NewResource", menuName = "Game/Resource")]
public class ResourceDataSO : ScriptableObject
{
    public string resourceName;
    public ResourceType resourceType;
    public Sprite resourceIcon;
    public Color resourceColor;
    public float baseValue;
    [TextArea(2, 4)] public string description;
    
    // Resource chain information (for secondary resources)
    public bool isSecondaryResource = false;
    public ResourceDataSO[] requiredResources;
    public int[] requiredAmounts;
    
    // Production info
    public string primaryProductionSector; // Which sector typically produces this
    
    public enum ResourceType
    {
        Food,
        RawMaterial,
        ProcessedGood,
        Wealth,
        Knowledge,
        Labor
    }
}