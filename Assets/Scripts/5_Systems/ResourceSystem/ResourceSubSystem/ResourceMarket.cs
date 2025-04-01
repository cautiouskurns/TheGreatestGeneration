using UnityEngine;
using System.Collections.Generic;

public class ResourceMarket : MonoBehaviour
{
    // Singleton reference
    public static ResourceMarket Instance { get; private set; }
    
    // Price tracking
    private Dictionary<string, float> basePrices = new Dictionary<string, float>();
    private Dictionary<string, float> currentPrices = new Dictionary<string, float>();
    private Dictionary<string, List<float>> priceHistory = new Dictionary<string, List<float>>();
    
    // Market parameters
    [Header("Market Settings")]
    [Range(0.1f, 1.0f)]
    public float priceVolatility = 0.2f;
    [Range(0.01f, 0.5f)]
    public float demandElasticity = 0.1f;
    [Range(1, 50)]
    public int historyLength = 20;
    
    // Optional visualizations
    [Header("Debug Visualization")]
    public bool showDebugInfo = false;
    public TMPro.TextMeshProUGUI debugText;
    
    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        InitializeBasePrices();
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe("TurnEnded", OnTurnEnded);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
    }
    
    private void InitializeBasePrices()
    {
        // Get resource definitions from ResourceRegistry
        ResourceRegistry registry = ResourceRegistry.Instance;
        if (registry == null)
        {
            Debug.LogWarning("ResourceRegistry not found, creating fallback prices");
            
            // Create some fallback prices
            basePrices["Food"] = 10f;
            currentPrices["Food"] = 10f;
            priceHistory["Food"] = new List<float> { 10f };
            
            basePrices["Wood"] = 5f;
            currentPrices["Wood"] = 5f;
            priceHistory["Wood"] = new List<float> { 5f };
            
            return;
        }
        
        var resources = registry.GetAllResourceDefinitions();
        foreach (var entry in resources)
        {
            basePrices[entry.Key] = entry.Value.baseValue;
            currentPrices[entry.Key] = entry.Value.baseValue;
            priceHistory[entry.Key] = new List<float> { entry.Value.baseValue };
            
            Debug.Log($"Initialized price for {entry.Key}: {entry.Value.baseValue}");
        }
    }
    
    private void OnTurnEnded(object _)
    {
        UpdatePrices();
        
        if (showDebugInfo && debugText != null)
        {
            UpdateDebugInfo();
        }
    }
    
    private void UpdatePrices()
    {
        // Get global supply and demand
        Dictionary<string, float> globalSupply = CalculateGlobalSupply();
        Dictionary<string, float> globalDemand = CalculateGlobalDemand();
        
        // Update prices based on supply/demand
        foreach (var resourceName in basePrices.Keys)
        {
            float supply = globalSupply.ContainsKey(resourceName) ? globalSupply[resourceName] : 0.1f;
            float demand = globalDemand.ContainsKey(resourceName) ? globalDemand[resourceName] : 0.1f;
            
            // Ensure non-zero supply to avoid division by zero
            supply = Mathf.Max(0.1f, supply);
            
            // Calculate demand/supply ratio
            float ratio = demand / supply;
            
            // Get current price
            float currentPrice = currentPrices[resourceName];
            
            // Calculate new price (with smoothing)
            float targetPrice = basePrices[resourceName] * Mathf.Pow(ratio, demandElasticity);
            float maxChange = currentPrice * priceVolatility;
            
            // Add small random variation
            float randomFactor = Random.Range(0.95f, 1.05f);
            targetPrice *= randomFactor;
            
            // Limit changes to avoid wild swings
            float newPrice = Mathf.Clamp(
                targetPrice,
                currentPrice - maxChange,
                currentPrice + maxChange
            );
            
            // Update current price
            currentPrices[resourceName] = newPrice;
            
            // Add to history
            priceHistory[resourceName].Add(newPrice);
            
            // Limit history length
            if (priceHistory[resourceName].Count > historyLength)
                priceHistory[resourceName].RemoveAt(0);
        }
        
        // Trigger price updated event
        EventBus.Trigger("ResourcePricesUpdated", currentPrices);
    }
    
    private Dictionary<string, float> CalculateGlobalSupply()
    {
        // Get all regions and sum production rates
        Dictionary<string, float> globalSupply = new Dictionary<string, float>();
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return globalSupply;
        
        var regions = gameManager.GetAllRegions();
        
        foreach (var region in regions.Values)
        {
            if (region.resources == null) continue;
            
            var production = region.resources.GetAllProductionRates();
            foreach (var entry in production)
            {
                if (!globalSupply.ContainsKey(entry.Key))
                    globalSupply[entry.Key] = 0;
                    
                globalSupply[entry.Key] += entry.Value;
            }
        }
        
        return globalSupply;
    }
    
    private Dictionary<string, float> CalculateGlobalDemand()
    {
        // Get all regions and sum consumption rates
        Dictionary<string, float> globalDemand = new Dictionary<string, float>();
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null) return globalDemand;
        
        var regions = gameManager.GetAllRegions();
        
        foreach (var region in regions.Values)
        {
            if (region.resources == null) continue;
            
            var consumption = region.resources.GetAllConsumptionRates();
            foreach (var entry in consumption)
            {
                if (!globalDemand.ContainsKey(entry.Key))
                    globalDemand[entry.Key] = 0;
                    
                globalDemand[entry.Key] += entry.Value;
            }
        }
        
        return globalDemand;
    }
    
    private void UpdateDebugInfo()
    {
        string info = "Market Prices:\n";
        
        foreach (var entry in currentPrices)
        {
            float basePrice = basePrices[entry.Key];
            float currentPrice = entry.Value;
            float ratio = currentPrice / basePrice;
            
            string trend = ratio > 1.1f ? "↑" : (ratio < 0.9f ? "↓" : "→");
            string color = ratio > 1.1f ? "red" : (ratio < 0.9f ? "green" : "white");
            
            info += $"{entry.Key}: {currentPrice:F1} <color={color}>{trend}</color>\n";
        }
        
        debugText.text = info;
    }
    
    // Public accessors
    public float GetCurrentPrice(string resourceName)
    {
        if (currentPrices.ContainsKey(resourceName))
            return currentPrices[resourceName];
            
        return 10f; // Default fallback price
    }
    
    public float GetBasePrice(string resourceName)
    {
        if (basePrices.ContainsKey(resourceName))
            return basePrices[resourceName];
            
        return 10f;
    }
    
    public List<float> GetPriceHistory(string resourceName)
    {
        if (priceHistory.ContainsKey(resourceName))
            return new List<float>(priceHistory[resourceName]);
            
        return new List<float>();
    }
    
    public Dictionary<string, float> GetAllCurrentPrices()
    {
        return new Dictionary<string, float>(currentPrices);
    }
    
    public float GetPriceRatio(string resourceName)
    {
        if (currentPrices.ContainsKey(resourceName) && basePrices.ContainsKey(resourceName))
        {
            return currentPrices[resourceName] / basePrices[resourceName];
        }
        
        return 1.0f;
    }
}