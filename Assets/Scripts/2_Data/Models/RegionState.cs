using UnityEngine;
using System.Collections.Generic;

public class RegionState
{
    public string RegionName { get; set; }
    public string OwnerNation { get; set; }
    public string DominantSector { get; set; }
    public Dictionary<string, float> ResourceProduction { get; set; } = new Dictionary<string, float>();
    public float Satisfaction { get; set; } = 0.5f;
    public int Population { get; set; } = 100;
    public List<string> ActiveProjects { get; set; } = new List<string>();
    public Dictionary<string, int> InfrastructureLevels { get; set; } = new Dictionary<string, int>();
}
