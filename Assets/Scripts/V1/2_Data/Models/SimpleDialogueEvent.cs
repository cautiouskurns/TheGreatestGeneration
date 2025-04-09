/// CLASS PURPOSE:
/// This file defines the structure for simple narrative events and dialogue interactions in the game.
/// It provides a lightweight system for displaying multi-line dialogues with player choices,
/// each of which can lead to various gameplay outcomes or effects.
/// 
/// CORE RESPONSIBILITIES:
/// - Store and structure narrative dialogue events (SimpleDialogueEvent)
/// - Enable variable substitution in dialogue lines for dynamic context
/// - Provide branching dialogue choices with conditional visibility
/// - Define outcome types that influence game state (resources, policies, projects, etc.)
/// 
/// KEY COLLABORATORS:
/// - GameStateManager: Supplies state values for variable substitution and conditions
/// - DialogueSystem or UI layer: Displays dialogue lines and choices to the player
/// - ResourceManager, EconomySystem: Receive and process dialogue outcome effects
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - DialogueLines can dynamically substitute text placeholders (e.g., {CurrentTurn})
/// - DialogueChoices can have optional conditions tied to simple game state values
/// - DialogueOutcomes support both temporary and permanent effects with broad flexibility
/// 
/// REFACTORING SUGGESTIONS:
/// - Introduce ID-based triggers to avoid duplicate event firings
/// - Move condition checking logic out of UI and into centralized validators
/// - Expand placeholder substitution to support more state variables or modifiers
/// 
/// EXTENSION OPPORTUNITIES:
/// - Link events to specific regions, sectors, or project completion
/// - Add support for timed reactivation or one-time events
/// - Create modifiable event chains or evolving dialogue trees over multiple turns

using System.Collections.Generic;
using UnityEngine;
using V1.Core;

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
        if (stateManager == null) 
        {
            Debug.LogWarning("Cannot process text variables: GameStateManager is null");
            return text;
        }
        
        string processedText = text;
        
        // Replace economic phase
        processedText = processedText.Replace("{EconomicPhase}", 
            stateManager.Economy.CurrentEconomicCyclePhase);
        
        // Replace turn count
        processedText = processedText.Replace("{CurrentTurn}", 
            stateManager.GetCurrentTurn().ToString());
        
        Debug.Log($"Processed text: '{text}' -> '{processedText}'");
        
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
        // Existing types
        AddResource,
        RemoveResource,
        ChangeRelation,
        ChangeSatisfaction,
        SetEconomicPhase,
        RecordDecision,
        
        // New economic-focused types
        ModifyProductionEfficiency,
        ModifyInfrastructure,
        AdjustTaxRate,
        ModifyResourcePrice,
        ApplyPolicyModifier,
        TriggerEconomicEvent,
        GrantProject,
        ChangeLabor
    }
    
    public OutcomeType type;
    public string targetId; // Resource, nation, region, policy id
    public float value;
    public string description;
    public int durationTurns = 0; // For temporary effects
}