using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SimpleDialogueEvent
{
    public string id;
    public string title;
    [TextArea(3, 5)]
    public string description;
    public List<DialogueLine> lines = new List<DialogueLine>();
    public List<DialogueChoice> choices = new List<DialogueChoice>();
    
    // Simple condition to determine if this event should be shown
    public string requiredEconomicPhase = ""; // If empty, no phase requirement
    public string requiredResourceShortage = ""; // If empty, no shortage requirement
}

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string text;
    public string speakerName = "";
    
    // Method to process text with state variables
    public string GetProcessedText(GameStateManager stateManager)
    {
        if (stateManager == null) return text;
        
        string processedText = text;
        
        // Replace economic phase
        processedText = processedText.Replace("{EconomicPhase}", 
            stateManager.Economy.CurrentEconomicCyclePhase);
        
        // Replace turn count
        processedText = processedText.Replace("{CurrentTurn}", 
            stateManager.GetCurrentTurn().ToString());
        
        // Add more replacements as needed
        
        return processedText;
    }
}

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 3)]
    public string text;
    public List<DialogueOutcome> outcomes = new List<DialogueOutcome>();
    
    // Optional condition to show this choice
    public bool hasCondition = false;
    public string requiredState = ""; // For simple conditions
    public float requiredValue = 0;
}

[System.Serializable]
public class DialogueOutcome
{
    public enum OutcomeType
    {
        AddResource,
        RemoveResource,
        ChangeRelation,
        ChangeSatisfaction,
        SetEconomicPhase,
        RecordDecision
    }
    
    public OutcomeType type;
    public string targetId; // Resource, nation, or region id
    public float value;
    [TextArea(1, 2)]
    public string description;
}