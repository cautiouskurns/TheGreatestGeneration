using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameStateDisplay : MonoBehaviour
{
    public TextMeshProUGUI stateText;
    public Button refreshButton;
    
    void Start()
    {
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshDisplay);
            
        RefreshDisplay();
    }
    
    public void RefreshDisplay()
    {
        var stateManager = GameStateManager.Instance;
        if (stateManager == null)
        {
            stateText.text = "GameStateManager not found!";
            return;
        }
        
        string display = "--- Game State ---\n\n";
        
        // Basic info
        display += $"Turn: {stateManager.GetCurrentTurn()}\n";
        display += $"Economic Phase: {stateManager.Economy.CurrentEconomicCyclePhase}\n";
        display += $"Turns in Phase: {stateManager.Economy.TurnsInCurrentPhase}\n\n";
        
        // Resources
        display += "Resource Shortages:\n";
        foreach (var resource in stateManager.Economy.ResourcesInShortage)
        {
            display += $"- {resource}\n";
        }
        
        display += "\nResource Surpluses:\n";
        foreach (var resource in stateManager.Economy.ResourcesInSurplus)
        {
            display += $"- {resource}\n";
        }
        
        // Diplomatic relations
        display += "\nDiplomatic Relations:\n";
        foreach (var relation in stateManager.Diplomacy.NationRelations)
        {
            display += $"- {relation.Key}: {relation.Value:F1}\n";
        }
        
        // Regions
        display += "\nRegions:\n";
        foreach (var region in stateManager.RegionStates)
        {
            display += $"- {region.Key}: Satisfaction {region.Value.Satisfaction:F2}\n";
        }
        
        // Decisions
        display += "\nSignificant Decisions:\n";
        foreach (var decision in stateManager.History.SignificantDecisions)
        {
            display += $"- {decision}\n";
        }
        
        stateText.text = display;
    }
}