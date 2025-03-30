using UnityEngine;
using System.Collections.Generic;

public class EconomyState
{
    // The current phase of the economic cycle
    public string CurrentEconomicCyclePhase { get; set; } = "Expansion";
    
    // How many turns we've been in the current phase
    public int TurnsInCurrentPhase { get; set; } = 0;
    
    // Track resources that are in shortage or surplus
    public List<string> ResourcesInShortage { get; set; } = new List<string>();
    public List<string> ResourcesInSurplus { get; set; } = new List<string>();
}
