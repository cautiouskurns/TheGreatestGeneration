using UnityEngine;

public class RegionState
{
    public string RegionName { get; set; }
    public string OwnerNation { get; set; }
    public string DominantSector { get; set; }
    public float Satisfaction { get; set; } = 0.5f; // 0 to 1
    public int Population { get; set; } = 100;
}