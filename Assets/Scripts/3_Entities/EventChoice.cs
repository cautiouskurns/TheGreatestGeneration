using UnityEngine; 
using System.Collections.Generic;

[System.Serializable]
public class EventChoice
{
    public string Text;
    public List<EventOutcome> Outcomes = new List<EventOutcome>();
    
    // For state-dependent text variations
    public List<DialogueVariant> Variants = new List<DialogueVariant>();
    
    public string GetProcessedText(GameStateManager state)
    {
        // Same logic as DialogueNode.GetProcessedText
        // ...
        return Text;
    }
}
