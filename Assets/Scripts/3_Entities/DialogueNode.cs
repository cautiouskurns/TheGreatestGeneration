using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Serializable]
public class DialogueNode
{
    public string Id;
    public string Text;
    public List<string> NextNodeIds = new List<string>();
    public string SpeakerId;
    
    // For state-dependent text variations
    public List<DialogueVariant> Variants = new List<DialogueVariant>();
    
    public string GetProcessedText(GameStateManager state)
    {
        // Check if any variants apply based on current state
        foreach (var variant in Variants)
        {
            bool allConditionsMet = true;
            foreach (var condition in variant.Conditions)
            {
                var evaluator = new EventConditionEvaluator(state);
                if (!evaluator.EvaluateCondition(condition))
                {
                    allConditionsMet = false;
                    break;
                }
            }
            
            if (allConditionsMet)
                return ProcessStatePlaceholders(variant.Text, state);
        }
        
        // Default text if no variants match
        return ProcessStatePlaceholders(Text, state);
    }
    
    private string ProcessStatePlaceholders(string text, GameStateManager state)
    {
        // Replace placeholders like {Economy.CurrentEconomicCyclePhase} with actual values
        
        // Example: {CurrentNation} -> actual nation name
        text = text.Replace("{CurrentNation}", state.GetState<string>("CurrentNation"));
        
        // Pattern-based replacements for nested properties
        // Example regex to match {Economy.ResourcesInShortage[0]}
        var pattern = @"\{(\w+)\.(\w+)(?:\[(\d+)\])?\}";
        text = Regex.Replace(text, pattern, match => {
            var category = match.Groups[1].Value;
            var property = match.Groups[2].Value;
            var index = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : -1;
            
            // Process based on category and property
            if (category == "Economy")
            {
                if (property == "CurrentEconomicCyclePhase")
                    return state.Economy.CurrentEconomicCyclePhase;
                else if (property == "ResourcesInShortage" && index >= 0 && index < state.Economy.ResourcesInShortage.Count)
                    return state.Economy.ResourcesInShortage[index];
                // etc.
            }
            // Add more categories
            
            return match.Value; // Return original if no replacement found
        });
        
        return text;
    }
}

[System.Serializable]
public class DialogueVariant
{
    public List<EventCondition> Conditions = new List<EventCondition>();
    public string Text;
}


