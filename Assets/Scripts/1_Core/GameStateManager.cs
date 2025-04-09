using UnityEngine;
using System.Collections.Generic;

/// CLASS PURPOSE:
/// GameStateManager is the central authority for tracking and coordinating high-level game state,
/// including economy, diplomacy, region conditions, event history, and simulation metadata.
/// It facilitates persistence, inter-system sync, temporary effects, and runtime state access.
/// 
/// CORE RESPONSIBILITIES:
/// - Maintain core state data (Economy, Diplomacy, History, RegionStates)
/// - Provide accessor methods for state querying and updates
/// - Track turn progression and temporary gameplay effects
/// - Synchronize with GameManager and apply derived values (e.g., wealth growth)
/// - Store generic key/value pairs for cross-system state lookup
/// 
/// KEY COLLABORATORS:
/// - GameManager: Supplies current world data (e.g., regions) for syncing
/// - RegionEntity: Source of dynamic economic/population inputs
/// - Event systems: Consumes/produces event tracking information
/// - Dialogue system: Pulls state for branching logic and outcome handling
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - Implements singleton pattern for global accessibility
/// - Contains both long-term state (RegionStates) and transient logic (TemporaryEffects)
/// - Game logic (e.g., ReverseProductionEffect) is mixed into state manager
/// - Tracks event occurrence metadata (per-type and per-resource)
/// 
/// REFACTORING SUGGESTIONS:
/// - Consider delegating TemporaryEffect logic to a dedicated EffectManager
/// - Extract sync logic to a GameStateSynchronizer class to isolate responsibilities
/// - Move economic calculations (e.g., CalculateTotalWealth) into an EconomicsAnalyzer utility
/// - Abstract event cooldowns to support varied durations per event/resource type
/// 
/// EXTENSION OPPORTUNITIES:
/// - Could support save/load serialization hooks
/// - Useful for implementing AI state mirroring or forecasting
/// - Base system for metrics, logging, or external simulation tools

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
    private Dictionary<string, int> lastEventTurns = new Dictionary<string, int>();
    private Dictionary<string, int> lastResourceEventTurns = new Dictionary<string, int>();
    private int eventCooldownTurns = 5; // Number of turns before the same event can happen again

    
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

    // Add these methods to GameStateManager
    public List<TemporaryEffect> TemporaryEffects { get; private set; } = new List<TemporaryEffect>();

    public void AddTemporaryEffect(TemporaryEffect effect)
    {
        TemporaryEffects.Add(effect);
    }

    public void ProcessTemporaryEffects()
    {
        List<TemporaryEffect> expiredEffects = new List<TemporaryEffect>();
        
        foreach (var effect in TemporaryEffects)
        {
            effect.RemainingTurns--;
            
            // Mark for removal if expired
            if (effect.RemainingTurns <= 0)
            {
                expiredEffects.Add(effect);
                ReverseEffect(effect);
            }
        }
        
        // Remove expired effects
        foreach (var effect in expiredEffects)
        {
            TemporaryEffects.Remove(effect);
        }
    }

    private void ReverseEffect(TemporaryEffect effect)
    {
        // Reversal implementation depends on effect type
        switch (effect.Type)
        {
            case "production":
                ReverseProductionEffect(effect);
                break;
            // Handle other types...
        }
    }

    private void ReverseProductionEffect(TemporaryEffect effect)
    {
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return;
        
        RegionEntity region = gameManager.GetRegion(effect.TargetId);
        if (region != null)
        {
            // Remove the temporary modifier
            region.productionEfficiency -= effect.Value;
            Debug.Log($"Removed temporary production modifier from {effect.TargetId}");
        }
    }

    // Record an event occurrence
    public void RecordEvent(string eventType)
    {
        lastEventTurns[eventType] = GetCurrentTurn();
    }

    // Record a resource-specific event
    public void RecordResourceEvent(string resourceName)
    {
        lastResourceEventTurns[resourceName] = GetCurrentTurn();
    }

    // Check if we had an event recently
    public bool HasRecentEvent(string eventType)
    {
        if (lastEventTurns.ContainsKey(eventType))
        {
            int lastTurn = lastEventTurns[eventType];
            return (GetCurrentTurn() - lastTurn) < eventCooldownTurns;
        }
        return false;
    }

    // Check if we had a resource event recently
    public bool HasRecentEventForResource(string resourceName)
    {
        if (lastResourceEventTurns.ContainsKey(resourceName))
        {
            int lastTurn = lastResourceEventTurns[resourceName];
            return (GetCurrentTurn() - lastTurn) < eventCooldownTurns;
        }
        return false;
    }
        
    // Calculate wealth growth rate
    // Requires tracking previous turn wealth
    private float previousTotalWealth = 0;
    public float GetWealthGrowthRate()
    {
        float currentTotalWealth = CalculateTotalWealth();
        
        if (previousTotalWealth == 0)
        {
            // First calculation
            previousTotalWealth = currentTotalWealth;
            return 0;
        }
        
        // Calculate growth rate
        float growthRate = (currentTotalWealth - previousTotalWealth) / previousTotalWealth;
        
        // Store current wealth for next turn
        previousTotalWealth = currentTotalWealth;
        
        return growthRate;
    }

    private float CalculateTotalWealth()
    {
        float total = 0;
        
        // Find GameManager
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return 0;
        
        // Sum wealth across all regions
        var regions = gameManager.GetAllRegions();
        foreach (var region in regions.Values)
        {
            total += region.wealth;
        }
        
        return total;
    }   
}

[System.Serializable]
public class TemporaryEffect
{
    public string Type;
    public string TargetId;
    public float Value;
    public int RemainingTurns;
}