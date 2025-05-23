using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using V1.Managers;
using V1.Core;

namespace V1.UI
{   
    /// CLASS PURPOSE:
    /// GameStateDisplay is a UI controller that formats and renders a summary of the current
    /// game state, including economic conditions, diplomatic relations, regional satisfaction,
    /// and decision history.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Display current turn, economic phase, and regional satisfaction
    /// - Show lists of resources in shortage or surplus
    /// - Present diplomatic relations and decision history
    /// - Refresh UI content on user interaction or dialogue completion
    ///
    /// KEY COLLABORATORS:
    /// - GameStateManager: Primary data source for all displayed game state information
    /// - EventBus: Used to listen for dialogue-related or UI-refreshing events
    /// - Unity UI (TextMeshProUGUI, Button): Displays formatted state data and supports user interaction
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses HTML-style formatting for rich UI text display
    /// - Event-driven and manual update triggers ensure the UI stays in sync with game state
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Split formatting logic into a separate builder class or helper methods
    /// - Improve modularity by using subcomponents or widgets for each section
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add filtering or sorting options for regions and decisions
    /// - Include graphs or charts for economic or satisfaction trends
    /// - Support multi-language or accessibility features in the UI


    public class GameStateDisplay : MonoBehaviour
    {
        public TextMeshProUGUI stateText;
        public Button refreshButton;
        
        void Start()
        {
            if (refreshButton != null)
                refreshButton.onClick.AddListener(RefreshDisplay);
                
            // Register for events to update after dialogues
            EventBus.Subscribe("DialogueEnded", OnDialogueEnded);
            
            // Initial refresh
            RefreshDisplay();
        }
        
        void OnDestroy()
        {
            // Clean up event subscription
            EventBus.Unsubscribe("DialogueEnded", OnDialogueEnded);
            
            if (refreshButton != null)
                refreshButton.onClick.RemoveListener(RefreshDisplay);
        }
        
        private void OnDialogueEnded(object _)
        {
            // Refresh display when dialogue ends
            RefreshDisplay();
        }
        
        public void RefreshDisplay()
        {
            var stateManager = GameStateManager.Instance;
            if (stateManager == null)
            {
                if (stateText != null)
                    stateText.text = "GameStateManager not found!";
                return;
            }
            
            string display = "<size=20><b>Game State</b></size>\n\n";
            
            // Basic info
            display += $"<b>Turn:</b> {stateManager.GetCurrentTurn()}\n";
            display += $"<b>Economic Phase:</b> {stateManager.Economy.CurrentEconomicCyclePhase}\n";
            display += $"<b>Turns in Phase:</b> {stateManager.Economy.TurnsInCurrentPhase}\n\n";
            
            // Resources
            display += "<b>Resource Shortages:</b>\n";
            if (stateManager.Economy.ResourcesInShortage.Count == 0)
            {
                display += "None\n";
            }
            else
            {
                foreach (var resource in stateManager.Economy.ResourcesInShortage)
                {
                    display += $"• {resource}\n";
                }
            }
            
            display += "\n<b>Resource Surpluses:</b>\n";
            if (stateManager.Economy.ResourcesInSurplus.Count == 0)
            {
                display += "None\n";
            }
            else
            {
                foreach (var resource in stateManager.Economy.ResourcesInSurplus)
                {
                    display += $"• {resource}\n";
                }
            }
            
            // Diplomatic relations
            display += "\n<b>Diplomatic Relations:</b>\n";
            if (stateManager.Diplomacy.NationRelations.Count == 0)
            {
                display += "No relations established\n";
            }
            else
            {
                foreach (var relation in stateManager.Diplomacy.NationRelations)
                {
                    string relationColor = relation.Value >= 0 ? "green" : "red";
                    display += $"• {relation.Key}: <color={relationColor}>{relation.Value:F1}</color>\n";
                }
            }
            
            // Regions
            display += "\n<b>Regions:</b>\n";
            if (stateManager.RegionStates.Count == 0)
            {
                display += "No regions defined\n";
            }
            else
            {
                foreach (var region in stateManager.RegionStates)
                {
                    string satisfactionColor = region.Value.Satisfaction >= 0.5f ? "green" : "red";
                    display += $"• {region.Key}: Satisfaction <color={satisfactionColor}>{region.Value.Satisfaction:F2}</color>\n";
                }
            }
            
            // Decisions
            display += "\n<b>Significant Decisions:</b>\n";
            if (stateManager.History.SignificantDecisions.Count == 0)
            {
                display += "No decisions made yet\n";
            }
            else
            {
                foreach (var decision in stateManager.History.SignificantDecisions)
                {
                    display += $"• {decision}\n";
                }
            }
            
            if (stateText != null)
                stateText.text = display;
        }
    }
}