using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V1.Core;

namespace V1.Utils
{  
    /// CLASS PURPOSE:
    /// GameStateDebugger is a developer utility component that displays key elements of the
    /// current game state, including turn number, economic phase, generational progress,
    /// region statuses, and diplomatic relations.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Trigger game state synchronization with internal systems
    /// - Format and display key state variables in a debug text panel
    /// - Bind debug logic to a UI button for easy access during development
    ///
    /// KEY COLLABORATORS:
    /// - GameStateManager: Central provider of turn, region, history, and diplomacy data
    /// - Unity UI (TextMeshProUGUI, Button): Interface for triggering and viewing debug output
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses direct property and field access from GameStateManager singleton
    /// - Outputs structured multiline string summarizing multiple subsystems
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Extract text formatting into a separate builder or formatter class
    /// - Add runtime toggles for filtering sections (e.g., show only diplomacy)
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Include region-level economic stats or historical data
    /// - Enable log export or copy-to-clipboard functionality
    /// - Support integration with in-editor debugging or cheat systems

    public class GameStateDebugger : MonoBehaviour
    {
        public TextMeshProUGUI debugText;
        public Button syncButton;
        
        private void Start()
        {
            if (syncButton != null)
                syncButton.onClick.AddListener(SyncAndDebug);
        }
        
        public void SyncAndDebug()
        {
            if (GameStateManager.Instance == null)
            {
                debugText.text = "GameStateManager not found!";
                return;
            }
            
            // Sync with game systems
            GameStateManager.Instance.SyncWithGameSystems();
            
            // Display state info
            string stateInfo = "Game State:\n";
            stateInfo += $"Current Turn: {GameStateManager.Instance.GetCurrentTurn()}\n";
            stateInfo += $"Economic Phase: {GameStateManager.Instance.Economy.CurrentEconomicCyclePhase}\n";
            stateInfo += $"Generation: {GameStateManager.Instance.History.GenerationNumber}\n\n";
            
            stateInfo += "Regions:\n";
            foreach (var entry in GameStateManager.Instance.RegionStates)
            {
                var region = entry.Value;
                stateInfo += $"- {region.RegionName}: {region.OwnerNation} (Sat: {region.Satisfaction:F2})\n";
            }
            
            stateInfo += "\nDiplomacy:\n";
            foreach (var entry in GameStateManager.Instance.Diplomacy.NationRelations)
            {
                stateInfo += $"- {entry.Key}: {entry.Value:F1}\n";
            }
            
            debugText.text = stateInfo;
        }
    }
}