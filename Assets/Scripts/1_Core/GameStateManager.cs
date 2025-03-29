using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameStateManager : MonoBehaviour
{
    // Singleton pattern for easy access
    public static GameStateManager Instance { get; private set; }
    
    // Core state data
    private Dictionary<string, object> gameStateData = new Dictionary<string, object>();
    
    // Economy-related state
    public EconomyState Economy { get; private set; } = new EconomyState();
    
    // Nation relations state
    public DiplomacyState Diplomacy { get; private set; } = new DiplomacyState();
    
    // Player history/decisions state
    public PlayerHistoryState History { get; private set; } = new PlayerHistoryState();
    
    // Regional states
    public Dictionary<string, RegionState> RegionStates { get; private set; } = new Dictionary<string, RegionState>();
    
    private void Awake()
    {
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
    
    // Initialize states from current game data
    public void InitializeState(/* dependencies */)
    {
        // Populate state from actual game systems
    }
    
    // Update state based on turn events
    public void UpdateState()
    {
        // Update all state objects from current game data
    }
    
    // Get/Set generic state data
    public T GetState<T>(string key)
    {
        if (gameStateData.ContainsKey(key) && gameStateData[key] is T)
            return (T)gameStateData[key];
        return default;
    }
    
    public void SetState<T>(string key, T value)
    {
        gameStateData[key] = value;
    }

    // Add to GameStateManager
    public void SyncWithGameSystems()
    {
        // Get references to other game systems
        var gameManager = FindFirstObjectByType<GameManager>();
        var tradeSystem = FindFirstObjectByType<TradeSystem>();
        var economicSystem = FindFirstObjectByType<EconomicSystem>();
        
        // Sync economy state
        //Economy.CurrentEconomicCyclePhase = FindFirstObjectByType<EconomicCycleSystem>()?.CurrentPhase ?? "Expansion";
        
        // Sync nation relations
        foreach (var nationEntry in gameManager.GetAllNations())
        {
            string nationName = nationEntry.Key;
            var nation = nationEntry.Value;
            
            // Set nation statistics
            SetState($"Nation.{nationName}.TotalWealth", nation.GetTotalWealth());
            SetState($"Nation.{nationName}.TotalProduction", nation.GetTotalProduction());
            
            // Set nation resources
            var resources = nation.GetAggregatedResources();
            foreach (var resource in resources)
            {
                SetState($"Nation.{nationName}.Resources.{resource.Key}", resource.Value);
            }
        }
        
        // Sync region states
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
            
            // Get dominant sector
            var laborAllocation = region.laborAllocation;
            if (laborAllocation.Count > 0)
            {
                regionState.DominantSector = laborAllocation
                    .OrderByDescending(kvp => kvp.Value)
                    .First().Key;
            }
            
            // Get resource production
            if (region.resources != null)
            {
                regionState.ResourceProduction = new Dictionary<string, float>(
                    region.resources.GetAllProductionRates()
                );
            }
        }
        
        // Sync trade data
        if (tradeSystem != null)
        {
            foreach (var regionName in RegionStates.Keys)
            {
                var imports = tradeSystem.GetRecentImports(regionName);
                var exports = tradeSystem.GetRecentExports(regionName);
                
                // Add trading partners
                var partners = new List<string>();
                foreach (var import in imports)
                    if (!partners.Contains(import.partnerName))
                        partners.Add(import.partnerName);
                        
                foreach (var export in exports)
                    if (!partners.Contains(export.partnerName))
                        partners.Add(export.partnerName);
                        
                Diplomacy.ActiveTradingPartners[regionName] = partners;
            }
        }
        
        // Sync resource shortages and surpluses
        var globalResources = new Dictionary<string, float>();
        var globalConsumption = new Dictionary<string, float>();
        
        foreach (var region in gameManager.GetAllRegions().Values)
        {
            if (region.resources == null) continue;
            
            // Aggregate all resources
            var resources = region.resources.GetAllResources();
            foreach (var resource in resources)
            {
                if (!globalResources.ContainsKey(resource.Key))
                    globalResources[resource.Key] = 0;
                globalResources[resource.Key] += resource.Value;
            }
            
            // Aggregate all consumption
            var consumption = region.resources.GetAllConsumptionRates();
            foreach (var consumptionEntry in consumption)
            {
                if (!globalConsumption.ContainsKey(consumptionEntry.Key))
                    globalConsumption[consumptionEntry.Key] = 0;
                globalConsumption[consumptionEntry.Key] += consumptionEntry.Value;
            }
        }
        
        // Determine shortages and surpluses
        Economy.ResourcesInShortage.Clear();
        Economy.ResourcesInSurplus.Clear();
        
        foreach (var entry in globalConsumption)
        {
            string resourceName = entry.Key;
            float consumptionRate = entry.Value;
            
            if (globalResources.ContainsKey(resourceName))
            {
                float available = globalResources[resourceName];
                
                if (available < consumptionRate * 0.9f) // 10% threshold for shortage
                    Economy.ResourcesInShortage.Add(resourceName);
                else if (available > consumptionRate * 1.5f) // 50% threshold for surplus
                    Economy.ResourcesInSurplus.Add(resourceName);
            }
        }
        
        // Set overall resource shortage flag
        Economy.IsResourceShortage = Economy.ResourcesInShortage.Count > 0;
    }
    
    // Add to your GameStateManager class
    // Fields to replace missing variables
    private string currentPhase = "Expansion"; // Default economic cycle phase
    private int currentTurn = 0;
    private int currentGeneration = 0;

    // Placeholder objects/models to reference in methods
    private EconomicCycleModel economicCycleModel;
    private ResourceManager resourceManager = new ResourceManager();
    private DiplomacyManager diplomacyManager = new DiplomacyManager();
    private RegionManager regionManager = new RegionManager();
    private InfrastructureSystem infrastructureSystem = new InfrastructureSystem();
    private EventHistoryManager eventHistoryManager = new EventHistoryManager();

    public string GetCurrentEconomicCyclePhase()
    {
        // Return from the model if available, otherwise use default
        return economicCycleModel != null ? economicCycleModel.currentPhase : currentPhase;
    }

    public bool IsResourceInShortage(string resourceName)
    {
        // Check using resource manager
        return resourceManager != null && resourceManager.IsInShortage(resourceName);
    }

    public bool IsResourceInSurplus(string resourceName)
    {
        // Similar to above
        return resourceManager != null && resourceManager.IsInSurplus(resourceName);
    }

    public float GetNationRelation(string nationName)
    {
        // Access diplomatic relations
        return diplomacyManager != null && diplomacyManager.Relations.ContainsKey(nationName) 
            ? diplomacyManager.Relations[nationName] 
            : 0;
    }

    public float GetRegionSatisfaction(string regionName)
    {
        // Get the region from region manager
        var region = regionManager != null ? regionManager.GetRegion(regionName) : null;
        return region != null ? region.Satisfaction : 0;
    }

    public int GetCurrentTurn()
    {
        return currentTurn;
    }

    public int GetCurrentGeneration()
    {
        return currentGeneration;
    }

    public float GetResourceAmount(string resourceName)
    {
        // Get resource amount from resource manager
        return resourceManager != null ? resourceManager.GetAmount(resourceName) : 0;
    }

    public float GetInfrastructureLevel(string infrastructureType)
    {
        // Get infrastructure level from infrastructure system
        return infrastructureSystem != null ? infrastructureSystem.GetLevel(infrastructureType) : 0;
    }

    public bool HasEventOccurred(string eventId)
    {
        // Check if event has occurred in event history
        return eventHistoryManager != null && eventHistoryManager.HasOccurred(eventId);
    }

    public int GetLastEventOccurrenceTurn(string eventId)
    {
        // Get last occurrence from event history
        return eventHistoryManager != null ? eventHistoryManager.GetLastOccurrenceTurn(eventId) : -1;
    }

    // Placeholder helper/accessor methods
    private ResourceManager GetResourceState()
    {
        return resourceManager;
    }

    private Dictionary<string, float> GetDiplomaticRelations()
    {
        return diplomacyManager?.Relations;
    }

    private Region GetRegionByName(string regionName)
    {
        return regionManager?.GetRegion(regionName);
    }

    private InfrastructureSystem GetInfrastructureSystem()
    {
        return infrastructureSystem;
    }

    private EventHistoryManager GetEventHistory()
    {
        return eventHistoryManager;
    }

    // Placeholder classes for the managers
    private class ResourceManager
    {
        public bool IsInShortage(string resourceName) { return false; }
        public bool IsInSurplus(string resourceName) { return false; }
        public float GetAmount(string resourceName) { return 0; }
    }

    private class DiplomacyManager
    {
        public Dictionary<string, float> Relations { get; } = new Dictionary<string, float>();
    }

    private class RegionManager
    {
        public Region GetRegion(string name) { return new Region(); }
    }

    private class Region
    {
        public float Satisfaction { get; set; } = 0.5f;
    }

    private class InfrastructureSystem
    {
        public float GetLevel(string type) { return 0; }
    }

    private class EventHistoryManager
    {
        public bool HasOccurred(string eventId) { return false; }
        public int GetLastOccurrenceTurn(string eventId) { return -1; }
    }

    private class EconomicCycleModel
    {
        public string currentPhase = "Expansion";
    } 
}