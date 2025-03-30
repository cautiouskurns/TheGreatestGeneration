using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameStateManager Instance { get; private set; }
    
    // Core state objects
    public EconomyState Economy { get; private set; } = new EconomyState();
    public DiplomacyState Diplomacy { get; private set; } = new DiplomacyState();
    public PlayerHistoryState History { get; private set; } = new PlayerHistoryState();
    public Dictionary<string, RegionState> RegionStates { get; private set; } = new Dictionary<string, RegionState>();
    
    // Generic state data storage
    private Dictionary<string, object> gameStateData = new Dictionary<string, object>();
    
    // Gameplay tracking
    private int currentTurn = 0;
    
    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // Get/Set generic state data
    public T GetState<T>(string key, T defaultValue = default)
    {
        if (gameStateData.ContainsKey(key) && gameStateData[key] is T)
            return (T)gameStateData[key];
        return defaultValue;
    }
    
    public void SetState<T>(string key, T value)
    {
        gameStateData[key] = value;
    }
    
    // Turn tracking
    public int GetCurrentTurn()
    {
        return currentTurn;
    }
    
    public void IncrementTurn()
    {
        currentTurn++;
        // Increment turns in current economic phase
        Economy.TurnsInCurrentPhase++;
    }
    
    // Sync game state with other systems
    public void SyncWithGameSystems()
    {
        // Get references to other systems
        var gameManager = FindAnyObjectByType<GameManager>();
        if (gameManager == null) return;
        
        try
        {
            // Sync region states - simplified example
            foreach (var regionEntry in gameManager.GetAllRegions())
            {
                string regionName = regionEntry.Key;
                var region = regionEntry.Value;
                
                // Create or update region state
                if (!RegionStates.ContainsKey(regionName))
                    RegionStates[regionName] = new RegionState { RegionName = regionName };
                
                var regionState = RegionStates[regionName];
                regionState.OwnerNation = region.ownerNationName;
                regionState.Satisfaction = region.satisfaction;
                regionState.Population = region.laborAvailable;
            }
            
            Debug.Log("Game state synced successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error syncing game state: {e.Message}");
        }
    }
    
    // Helper methods for specific state access
    public bool IsResourceInShortage(string resourceName)
    {
        return Economy.ResourcesInShortage.Contains(resourceName);
    }
    
    public bool IsResourceInSurplus(string resourceName)
    {
        return Economy.ResourcesInSurplus.Contains(resourceName);
    }
    
    public float GetNationRelation(string nationName)
    {
        if (Diplomacy.NationRelations.ContainsKey(nationName))
            return Diplomacy.NationRelations[nationName];
        return 0f; // Neutral by default
    }
    
    public float GetRegionSatisfaction(string regionName)
    {
        if (RegionStates.ContainsKey(regionName))
            return RegionStates[regionName].Satisfaction;
        return 0.5f; // Neutral by default
    }
    
    public string GetCurrentEconomicCyclePhase()
    {
        return Economy.CurrentEconomicCyclePhase;
    }
    
    public int GetCurrentGeneration()
    {
        return History.GenerationNumber;
    }
    
    public bool HasEventOccurred(string eventId)
    {
        return History.EventCounts.ContainsKey(eventId) && History.EventCounts[eventId] > 0;
    }
    
    public int GetLastEventOccurrenceTurn(string eventId)
    {
        // For a simplification, we'll just return -1 (never occurred)
        // In a full implementation, you'd track when each event occurred
        return -1;
    }
}