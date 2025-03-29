using UnityEngine;
using System.Collections.Generic;

public class EconomyState
{
    public string CurrentEconomicCyclePhase { get; set; } = "Expansion";
    public int TurnsInCurrentPhase { get; set; } = 0;
    public Dictionary<string, float> GlobalResourcePrices { get; set; } = new Dictionary<string, float>();
    // public Dictionary<string, ResourceTrend> ResourceTrends { get; set; } = new Dictionary<string, ResourceTrend>();
    public float GlobalSatisfaction { get; set; } = 0.5f;
    public bool IsResourceShortage { get; set; } = false;
    public List<string> ResourcesInShortage { get; set; } = new List<string>();
    public List<string> ResourcesInSurplus { get; set; } = new List<string>();
}
