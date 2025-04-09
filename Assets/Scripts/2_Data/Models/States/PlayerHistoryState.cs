/// CLASS PURPOSE:
/// PlayerHistoryState stores the evolving narrative and gameplay history of the player
/// across runs and generations. It enables persistent tracking of decisions, events,
/// and progression for use in meta-narrative, generational shifts, and unlock conditions.
/// 
/// CORE RESPONSIBILITIES:
/// - Track major player decisions that influence story or gameplay
/// - Count occurrences of key events for dynamic branching or progression
/// - Store the current generation number to support generational mechanics
/// 
/// KEY COLLABORATORS:
/// - GameManager / RunManager: Reads and writes to this state each turn
/// - EventSystem: Updates event occurrence counts
/// - NarrativeManager: Uses decisions and generations to shape emergent storytelling
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - All data is stored in basic collections (List, Dictionary)
/// - Generation is tracked as a single integer (1-indexed)
/// - No timestamping or ordering of decisions/events is present
/// 
/// REFACTORING SUGGESTIONS:
/// - Add timestamps or turn numbers to SignificantDecisions for better context
/// - Replace raw strings with enums or ID references for consistency
/// - Move generation logic to a separate struct or data object if it grows
/// 
/// EXTENSION OPPORTUNITIES:
/// - Track per-generation decisions for legacy system or timeline visualization
/// - Add player tag system (e.g., economic focus, moral tendency) based on choices
/// - Use EventCounts to dynamically unlock policies, characters, or scenarios

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
