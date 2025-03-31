using UnityEngine;
using UnityEngine.UI;
using TMPro;

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