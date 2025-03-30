using UnityEngine;
using System.Collections.Generic;

public class DiplomacyState
{
    // Tracks relations with other nations (-100 to 100)
    public Dictionary<string, float> NationRelations { get; set; } = new Dictionary<string, float>();
    
    // Tracks active trading partners for each region
    public Dictionary<string, List<string>> ActiveTradingPartners { get; set; } = new Dictionary<string, List<string>>();
}
