using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

// Main Trade System component that ties together trade calculation, recording, and visualization
public class TradeSystem : MonoBehaviour
{
    #region Configuration Properties
    
    // Trade calculation settings
    [Header("Trade Configuration")]
    [Range(1, 5)] public int tradeRadius = 2;
    [Range(0.1f, 1.0f)] public float tradeEfficiency = 0.8f;
    [Range(1, 10)] public int maxTradingPartnersPerRegion = 3;
    
    // Visualization settings
    [Header("Visualization Settings")]
    public bool showTradeLines = true;
    public float tradeLineWidth = 0.5f;
    public float tradeLineDuration = 2.0f;
    public Color importColor = new Color(0, 1, 0, 0.5f); // Semi-transparent green
    public Color exportColor = new Color(1, 0.5f, 0, 0.5f); // Semi-transparent orange
    public bool showTradeHeatmap = false;
    public bool showSelectedRegionTradesOnly = true;
    public float minimumTradeVolumeToShow = 5.0f;
    
    #endregion
    
    #region Private Fields
    
    // Component references
    private TradeCalculator calculator;
    private TradeVisualizer visualizer;
    private TradeRecorder recorder;
    
    // Region data
    private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();
    private string selectedRegionName = "";
    
    #endregion
    
    #region Unity Lifecycle Methods
    
    private void Awake()
    {
        // Initialize components
        recorder = new TradeRecorder();
        
        visualizer = new TradeVisualizer(
            importColor, exportColor, 
            tradeLineWidth, tradeLineDuration, 
            minimumTradeVolumeToShow,
            showTradeLines);
    }
    
    private void Start()
    {
        // Try to get regions after all objects are initialized
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            try
            {
                // Get regions from GameManager
                regions = gameManager.GetAllRegions();
                if (regions != null)
                {
                    Debug.Log($"TradeSystem: Got {regions.Count} regions from GameManager");
                    
                    // Initialize calculator with regions
                    calculator = new TradeCalculator(
                        regions,
                        tradeEfficiency,
                        maxTradingPartnersPerRegion,
                        tradeRadius
                    );
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TradeSystem: Error getting regions from GameManager: {e.Message}");
            }
        }
        
        // Delayed initialization to ensure everything is properly set up
        StartCoroutine(InitializeWithDelay());
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe("TurnEnded", ProcessTrade);
        EventBus.Subscribe("RegionSelected", OnRegionSelected);
        EventBus.Subscribe("RegionEntitiesReady", OnRegionsReady);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("TurnEnded", ProcessTrade);
        EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
        EventBus.Unsubscribe("RegionEntitiesReady", OnRegionsReady);
    }
    
    #endregion
    
    #region Initialization
    
    private System.Collections.IEnumerator InitializeWithDelay()
    {
        // Wait for two frames to ensure everything is properly initialized
        yield return null;
        yield return null;
        
        // Get GameManager reference safely
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("TradeSystem: GameManager not found during delayed initialization");
            yield break;
        }
        
        Debug.Log("TradeSystem: Found GameManager, attempting to get regions");
        
        try
        {
            // Try to get regions
            var allRegions = gameManager.GetAllRegions();
            
            // Check if the result is null
            if (allRegions == null)
            {
                Debug.LogError("TradeSystem: GameManager.GetAllRegions() returned null");
                yield break;
            }
            
            // Success! Store the regions
            regions = allRegions;
            Debug.Log($"TradeSystem: Successfully retrieved {regions.Count} regions");
            
            // Initialize calculator with regions
            calculator = new TradeCalculator(
                regions,
                tradeEfficiency,
                maxTradingPartnersPerRegion,
                tradeRadius
            );
            
            // Debug the trade system
            DebugTradeSystem();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TradeSystem: Detailed error: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }
    
    private void OnRegionsReady(object regionsObj)
    {
        // Store all regions for trade calculations
        if (regionsObj is Dictionary<string, RegionEntity> regionsDict)
        {
            regions = regionsDict;
            Debug.Log($"TradeSystem: Received {regions.Count} regions via RegionEntitiesReady");
            
            // Update calculator with new regions
            calculator = new TradeCalculator(
                regions,
                tradeEfficiency,
                maxTradingPartnersPerRegion,
                tradeRadius
            );
            
            // Debug the trade system
            DebugTradeSystem();
        }
    }
    
    #endregion
    
    #region Trade Processing
    
    private void ProcessTrade(object _)
    {
        // Clear old trade data
        recorder.ClearTradeData();
        visualizer.ClearTradeLines();
        
        // Skip if we don't have necessary components
        if (calculator == null || regions.Count == 0)
        {
            Debug.LogWarning("TradeSystem: Unable to process trades - missing calculator or regions");
            return;
        }
        
        // Calculate trades for this turn
        List<TradeTransaction> trades = calculator.CalculateTrades();
        
        Debug.Log($"TradeSystem: Processing {trades.Count} trades this turn");
        
        // Execute and record each trade
        foreach (var trade in trades)
        {
            // Execute the trade (transfer resources)
            trade.Execute();
            
            // Record for history and UI
            recorder.RecordTrade(trade);
            
            // Log the transaction
            Debug.Log($"Trade: {trade.Exporter.regionName} exported {trade.Amount} {trade.ResourceName} " +
                     $"to {trade.Importer.regionName} (received {trade.ReceivedAmount} after transport)");
        }
        
        // Refresh trade visualizations
        RefreshTradeLines();
        
        // Debug trading partner information
        var partnerCounts = calculator.GetTradingPartnersCount();
        foreach (var entry in partnerCounts)
        {
            if (entry.Value > 0)
            {
                Debug.Log($"Region {entry.Key} trading with {entry.Value} partners");
            }
        }
    }
    
    #endregion
    
    #region Region Selection
    
    private void OnRegionSelected(object regionObj)
    {
        if (regionObj is RegionEntity region)
        {
            selectedRegionName = region.regionName;
            RefreshTradeLines();
        }
        else if (regionObj is string regionName)
        {
            selectedRegionName = regionName;
            RefreshTradeLines();
        }
    }
    
    #endregion
    
    #region Visualization
    
    private void RefreshTradeLines()
    {
        // Clear existing lines
        visualizer.ClearTradeLines();
        
        // Skip if no visualization needed
        if (!showTradeLines) return;
        
        if (showSelectedRegionTradesOnly && !string.IsNullOrEmpty(selectedRegionName))
        {
            // Show only trades for selected region
            ShowSelectedRegionTradeLines();
        }
        else
        {
            // Show all trade lines
            ShowAllTradeLines();
        }
    }
    
    private void ShowSelectedRegionTradeLines()
    {
        // Skip if selected region doesn't exist
        if (!regions.ContainsKey(selectedRegionName)) return;
        
        RegionEntity selectedRegion = regions[selectedRegionName];
        
        // Show imports
        var imports = recorder.GetRecentImports(selectedRegionName);
        foreach (var trade in imports)
        {
            if (!regions.ContainsKey(trade.partnerName)) continue;
            
            RegionEntity exporter = regions[trade.partnerName];
            visualizer.ShowTradeLine(exporter, selectedRegion, importColor, trade.amount);
        }
        
        // Show exports
        var exports = recorder.GetRecentExports(selectedRegionName);
        foreach (var trade in exports)
        {
            if (!regions.ContainsKey(trade.partnerName)) continue;
            
            RegionEntity importer = regions[trade.partnerName];
            visualizer.ShowTradeLine(selectedRegion, importer, exportColor, trade.amount);
        }
    }
    
    private void ShowAllTradeLines()
    {
        // Get all exports
        var allExports = recorder.GetAllExports();
        
        // Show a line for each export
        foreach (var exportPair in allExports)
        {
            string exporterName = exportPair.Key;
            
            if (!regions.ContainsKey(exporterName)) continue;
            
            RegionEntity exporter = regions[exporterName];
            
            foreach (var trade in exportPair.Value)
            {
                if (!regions.ContainsKey(trade.partnerName)) continue;
                
                RegionEntity importer = regions[trade.partnerName];
                visualizer.ShowTradeLine(exporter, importer, exportColor, trade.amount);
            }
        }
    }
    
    #endregion
    
    #region Public Methods
    
    // Access methods for UI
    public List<TradeInfo> GetRecentImports(string regionName)
    {
        return recorder.GetRecentImports(regionName);
    }
    
    public List<TradeInfo> GetRecentExports(string regionName)
    {
        return recorder.GetRecentExports(regionName);
    }
    
    // Get color for heatmap visualization
    public Color GetRegionTradeColor(string regionName)
    {
        return recorder.GetRegionTradeColor(regionName, Color.blue, Color.red);
    }
    
    // Debug method to check resources available for trade
    public void DebugTradeSystem()
    {
        if (regions.Count == 0)
        {
            Debug.Log("TradeSystem: No regions available for debugging");
            return;
        }
        
        Debug.Log($"TradeSystem has {regions.Count} regions registered");
        
        // Check if regions have resources that can be traded
        foreach (var region in regions.Values)
        {
            if (region.resources == null)
            {
                Debug.LogWarning($"Region {region.regionName} has no resources component");
                continue;
            }
            
            var resources = region.resources.GetAllResources();
            var consumption = region.resources.GetAllConsumptionRates();
            
            Debug.Log($"Region {region.regionName}:");
            Debug.Log($"  Resources: {string.Join(", ", resources.Keys)}");
            Debug.Log($"  Consumption: {string.Join(", ", consumption.Keys)}");
            
            // Calculate potential surpluses
            foreach (var entry in resources)
            {
                string resourceName = entry.Key;
                float amount = entry.Value;
                float consumed = consumption.ContainsKey(resourceName) ? consumption[resourceName] : 0;
                
                if (amount > consumed * 1.2f)
                {
                    Debug.Log($"  SURPLUS: {resourceName} = {amount - (consumed * 1.2f):F1}");
                }
            }
            
            // Calculate deficits
            foreach (var entry in consumption)
            {
                string resourceName = entry.Key;
                float needed = entry.Value;
                float available = resources.ContainsKey(resourceName) ? resources[resourceName] : 0;
                
                if (needed > available)
                {
                    Debug.Log($"  DEFICIT: {resourceName} = {needed - available:F1}");
                }
            }
        }
    }
    
    #endregion
}