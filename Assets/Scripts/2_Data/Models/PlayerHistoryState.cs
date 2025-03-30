using UnityEngine;

using System.Collections.Generic;

public class PlayerHistoryState
{
    // Tracks significant decisions made by the player
    public List<string> SignificantDecisions { get; set; } = new List<string>();
    
    // Tracks how many times each event has occurred
    public Dictionary<string, int> EventCounts { get; set; } = new Dictionary<string, int>();
    
    // Which generation (era) is the player in
    public int GenerationNumber { get; set; } = 1;
}
