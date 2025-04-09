using UnityEngine;
using System.Collections.Generic;

/// CLASS PURPOSE:
/// EconomicEventTrigger is responsible for monitoring key economic indicators each turn
/// and triggering appropriate narrative events (e.g., shortages, crises, prosperity).
/// It links economic simulation data to the dialogue system and GameStateManager.
///
/// CORE RESPONSIBILITIES:
/// - Listen to turn completion events
/// - Evaluate economic conditions (e.g., resource balance, satisfaction, wealth)
/// - Trigger narrative events based on defined thresholds and event libraries
/// - Coordinate with GameStateManager to ensure events aren't duplicated
///
/// KEY COLLABORATORS:
/// - GameManager: Provides access to global region data
/// - GameStateManager: Tracks event history, resource metrics, and satisfaction
/// - EventDialogueManager: Displays narrative events with choices and outcomes
/// - EventBus: Dispatches and subscribes to simulation lifecycle events
///
/// CURRENT ARCHITECTURE NOTES:
/// - Clear separation between resource crisis, economic crisis, and prosperity logic
/// - Uses SO-based event library for modular event configuration
/// - Logic currently mixes condition evaluation with event dispatching
///
/// REFACTORING SUGGESTIONS:
/// - Abstract condition evaluation into separate evaluators (e.g., CrisisEvaluator)
/// - Convert event category arrays into dictionaries for faster lookup by type
/// - Centralize all thresholds and logic into configurable profiles or SOs
///
/// EXTENSION OPPORTUNITIES:
/// - Expand to support sector-specific or regional narrative events
/// - Link event types to ideology, factions, or seasonal modifiers
/// - Add cooldown and weight-based event frequency logic

public class EconomicEventTrigger : MonoBehaviour
{
    [Header("Event Libraries")]
    public SimpleDialogueEvent[] resourceShortageEvents;
    public SimpleDialogueEvent[] economicCrisisEvents;
    public SimpleDialogueEvent[] prosperityEvents;
    
    [Header("Trigger Thresholds")]
    [Range(0f, 1f)] public float resourceShortageThreshold = 0.3f;
    [Range(0f, 1f)] public float economicCrisisThreshold = 0.4f;
    [Range(0f, 1f)] public float prosperityThreshold = 0.8f;

    [Header("Event Library")]
    [SerializeField] private DialogueEventLibrarySO eventLibrary; // Use this for ScriptableObject approach
 
    
    private GameManager gameManager;
    private GameStateManager stateManager;
    
    private void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        stateManager = GameStateManager.Instance;
        
        if (eventLibrary != null)
        {
            eventLibrary.Initialize();
        }
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe("TurnProcessed", CheckEconomicTriggers);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("TurnProcessed", CheckEconomicTriggers);
    }
    
    private void CheckEconomicTriggers(object _)
    {
        if (gameManager == null || stateManager == null) return;
        
        // Check for resource shortages
        CheckResourceShortage();
        
        // Check for economic crisis
        CheckEconomicCrisis();
        
        // Check for prosperity
        CheckProsperity();
    }
    
    private void CheckResourceShortage()
    {
        // Get global resource balance
        Dictionary<string, float> resourceBalance = GetGlobalResourceBalance();
        
        foreach (var entry in resourceBalance)
        {
            string resourceName = entry.Key;
            float balance = entry.Value;
            
            // Check for shortage (negative balance)
            if (balance < -20 && IsCriticalResource(resourceName))
            {
                // Only trigger if we don't have a recent event for this resource
                if (!stateManager.HasRecentEventForResource(resourceName))
                {
                    // Get appropriate event from array
                    SimpleDialogueEvent shortageEvent = GetRandomEvent(resourceShortageEvents);
                    
                    if (shortageEvent != null)
                    {
                        EventDialogueManager.ShowEvent(shortageEvent);
                            Debug.Log($"Triggered {resourceName} shortage event");
                            
                            // Record event
                        stateManager.RecordResourceEvent(resourceName);
                    }
                }
            }
        }
    }
    
    private void TriggerResourceShortageEvent(string resourceName)
    {
        // Skip if we recently had an event for this resource
        if (stateManager.HasRecentEventForResource(resourceName))
            return;
            
        // Find a suitable event from the library
        SimpleDialogueEvent eventToShow = null;
        
        foreach (var evt in resourceShortageEvents)
        {
            if (evt.requiredResourceShortage == resourceName)
            {
                eventToShow = evt;
                break;
            }
        }
        
        // If found, trigger the event
        if (eventToShow != null)
        {
            EventDialogueManager.ShowEvent(eventToShow);
            Debug.Log($"Triggered shortage event for {resourceName}");
        }
    }
    
// In EconomicEventTrigger.cs
    private void CheckEconomicCrisis()
    {
        // Get overall economic health metrics
        float avgSatisfaction = GetAverageSatisfaction();
        float avgProduction = GetAverageProduction();
        
        // Crisis is detected when satisfaction is low and either production
        // or wealth is also below threshold
        if (avgSatisfaction < economicCrisisThreshold)
        {
            // Only trigger if we don't have a recent crisis event
            if (!stateManager.HasRecentEvent("EconomicCrisis"))
            {
                // Find appropriate crisis event
                SimpleDialogueEvent crisisEvent = GetRandomEvent(economicCrisisEvents);
                
                if (crisisEvent != null)
                {
                    EventDialogueManager.ShowEvent(crisisEvent);
                    Debug.Log("Triggered economic crisis event");
                    
                    // Record crisis
                    stateManager.RecordEvent("EconomicCrisis");
                }
            }
        }
    }

    private void CheckProsperity()
    {
        // Get overall economic health metrics
        float avgSatisfaction = GetAverageSatisfaction();
        float avgProduction = GetAverageProduction();
        float wealthGrowth = GetWealthGrowthRate();
        
        // Prosperity is detected when all metrics are high
        if (avgSatisfaction > prosperityThreshold && 
            avgProduction > prosperityThreshold && 
            wealthGrowth > 0.1f)
        {
            // Only trigger if we don't have a recent prosperity event
            if (!stateManager.HasRecentEvent("Prosperity"))
            {
                // Find appropriate prosperity event
                SimpleDialogueEvent prosperityEvent = GetRandomEvent(prosperityEvents);
                
                if (prosperityEvent != null)
                {
                    EventDialogueManager.ShowEvent(prosperityEvent);
                    Debug.Log("Triggered prosperity event");
                    
                    // Record prosperity
                    stateManager.RecordEvent("Prosperity");
                }
            }
        }
    }

    private Dictionary<string, float> GetGlobalResourceBalance()
    {
        Dictionary<string, float> globalBalance = new Dictionary<string, float>();
        
        if (gameManager == null) return globalBalance;
        
        var regions = gameManager.GetAllRegions();
        foreach (var region in regions.Values)
        {
            if (region.resources == null) continue;
            
            // Get production and consumption rates
            var production = region.resources.GetAllProductionRates();
            var consumption = region.resources.GetAllConsumptionRates();
            
            // Combine all resource names from both production and consumption
            HashSet<string> resourceNames = new HashSet<string>();
            foreach (var key in production.Keys) resourceNames.Add(key);
            foreach (var key in consumption.Keys) resourceNames.Add(key);
            
            // Calculate balance for each resource
            foreach (var resourceName in resourceNames)
            {
                float productionRate = production.ContainsKey(resourceName) ? 
                    production[resourceName] : 0;
                    
                float consumptionRate = consumption.ContainsKey(resourceName) ? 
                    consumption[resourceName] : 0;
                    
                float balance = productionRate - consumptionRate;
                
                // Add to global balance
                if (!globalBalance.ContainsKey(resourceName))
                    globalBalance[resourceName] = 0;
                    
                globalBalance[resourceName] += balance;
            }
        }
        
        return globalBalance;
    }

    private bool IsCriticalResource(string resourceName)
    {
        // Define which resources are critical
        return resourceName == "Food" || 
            resourceName == "Water" || 
            resourceName == "Crops";
    }

    private float GetTotalResourceAmount(string resourceName)
    {
        float total = 0;
        
        if (gameManager == null) return total;
        
        var regions = gameManager.GetAllRegions();
        foreach (var region in regions.Values)
        {
            if (region.resources == null) continue;
            
            total += region.resources.GetResourceAmount(resourceName);
        }
        
        return total;
    }

    private float GetTotalResourceConsumption(string resourceName)
    {
        float total = 0;
        
        if (gameManager == null) return total;
        
        var regions = gameManager.GetAllRegions();
        foreach (var region in regions.Values)
        {
            if (region.resources == null) continue;
            
            total += region.resources.GetConsumptionRate(resourceName);
        }
        
        return total;
    }

    private SimpleDialogueEvent GetRandomEvent(SimpleDialogueEvent[] events)
    {
        if (events == null || events.Length == 0)
            return null;
            
        return events[Random.Range(0, events.Length)];
    }

    private float GetAverageSatisfaction()
    {
        float total = 0;
        int count = 0;
        
        if (gameManager == null) return 0;
        
        var regions = gameManager.GetAllRegions();
        foreach (var region in regions.Values)
        {
            total += region.satisfaction;
            count++;
        }
        
        return count > 0 ? total / count : 0;
    }

    private float GetAverageProduction()
    {
        // Calculate average production as percentage of maximum possible
        float total = 0;
        int count = 0;
        
        if (gameManager == null) return 0;
        
        var regions = gameManager.GetAllRegions();
        foreach (var region in regions.Values)
        {
            // Normalize production to 0-1 range (assuming 100 is max production)
            total += Mathf.Clamp01(region.production / 100f);
            count++;
        }
        
        return count > 0 ? total / count : 0;
    }

    private float GetWealthGrowthRate()
    {
        // Get overall wealth growth rate from previous turn
        return stateManager.GetWealthGrowthRate();
    }

}