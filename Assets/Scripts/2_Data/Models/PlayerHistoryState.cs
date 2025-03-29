using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerHistoryState
{
    public List<string> SignificantDecisions { get; set; } = new List<string>();
    public Dictionary<string, int> EventCounts { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, DateTime> LastEventOccurrence { get; set; } = new Dictionary<string, DateTime>();
    public int GenerationNumber { get; set; } = 1;
}