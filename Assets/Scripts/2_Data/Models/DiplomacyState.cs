using UnityEngine;
using System.Collections.Generic; 

public class DiplomacyState
{
    public Dictionary<string, float> NationRelations { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, List<string>> ActiveTradingPartners { get; set; } = new Dictionary<string, List<string>>();
    public Dictionary<string, string> Conflicts { get; set; } = new Dictionary<string, string>();
}
