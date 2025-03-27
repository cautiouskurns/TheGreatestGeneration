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
            
        // Log initialization to debug
        Debug.Log("TradeSystem initialized with visualizer configured for import/export visualization");
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
        string newRegionName = "";
        
        if (regionObj is RegionEntity region)
        {
            newRegionName = region.regionName;
        }
        else if (regionObj is string regionName)
        {
            newRegionName = regionName;
        }
        
        // If clicking on the already selected region, deselect it
        if (newRegionName == selectedRegionName)
        {
            selectedRegionName = "";
            Debug.Log("TradeSystem: Deselected region, showing all trades");
        }
        else
        {
            selectedRegionName = newRegionName;
            Debug.Log($"TradeSystem: Selected region {selectedRegionName}");
        }
        
        // Refresh trade visualization
        RefreshTradeLines();
    }
        
    #endregion
    
    #region Visualization
    
    private void RefreshTradeLines()
    {
        // Clear existing lines
        visualizer.ClearTradeLines();
        
        // Skip if no visualization needed
        if (!showTradeLines) return;
        
        if (showSelectedRegionTradesOnly && !string.IsNullOrEmpty(selectedRegionName) && regions.ContainsKey(selectedRegionName))
        {
            // Show only trades for selected region
            ShowSelectedRegionTradeLines();
        }
        else
        {
            // Show all trade lines
            ShowAllTradeLines();
            
            // If we're here because the region doesn't exist, clear the selection
            if (!string.IsNullOrEmpty(selectedRegionName) && !regions.ContainsKey(selectedRegionName))
            {
                selectedRegionName = "";
            }
        }
    }

    // Add this to the #region Public Methods section
    public void ClearSelectedRegion()
    {
        // Clear the selected region
        selectedRegionName = "";
        
        // Refresh the trade lines to show all trades
        RefreshTradeLines();
        
        // Optionally log this action
        Debug.Log("TradeSystem: Cleared selected region, showing all trades");
    }
    
    private void ShowSelectedRegionTradeLines()
    {
        // Skip if selected region doesn't exist
        if (!regions.ContainsKey(selectedRegionName)) return;
        
        RegionEntity selectedRegion = regions[selectedRegionName];
        
        // Get all trade data for the selected region
        var imports = recorder.GetRecentImports(selectedRegionName);
        var exports = recorder.GetRecentExports(selectedRegionName);
        
        // Create lookup for bidirectional trades to enhance visualization
        Dictionary<string, HashSet<string>> bidirectionalTrades = new Dictionary<string, HashSet<string>>();
        
        // Find all bidirectional trade relationships (resources traded both ways)
        foreach (var import in imports)
        {
            string partnerName = import.partnerName;
            string resourceName = import.resourceName;
            
            // Initialize set if needed
            if (!bidirectionalTrades.ContainsKey(partnerName))
                bidirectionalTrades[partnerName] = new HashSet<string>();
            
            // Add this trade relationship
            bidirectionalTrades[partnerName].Add(resourceName);
        }
        
        // Show imports first
        foreach (var trade in imports)
        {
            if (!regions.ContainsKey(trade.partnerName)) continue;
            
            RegionEntity exporter = regions[trade.partnerName];
            visualizer.ShowTradeLine(exporter, selectedRegion, importColor, trade.amount);
        }
        
        // Show exports next so they appear on top of imports
        foreach (var trade in exports)
        {
            if (!regions.ContainsKey(trade.partnerName)) continue;
            
            RegionEntity importer = regions[trade.partnerName];
            visualizer.ShowTradeLine(selectedRegion, importer, exportColor, trade.amount);
        }
    }

    
    private void ShowAllTradeLines()
    {
        // Get all exports and imports
        var allExports = recorder.GetAllExports();
        var allImports = recorder.GetAllImports();
        
        // Create a set to track which trade relationships we've already visualized
        HashSet<string> visualizedTrades = new HashSet<string>();
        
        // First, show ALL imports (green lines) for a clear base layer
        foreach (var importPair in allImports)
        {
            string importerName = importPair.Key;
            
            if (!regions.ContainsKey(importerName)) continue;
            
            RegionEntity importer = regions[importerName];
            
            foreach (var trade in importPair.Value)
            {
                if (!regions.ContainsKey(trade.partnerName)) continue;
                
                RegionEntity exporter = regions[trade.partnerName];
                
                // Create a unique key for this trade relationship
                string tradeKey = $"{exporter.regionName}_{importer.regionName}_{trade.resourceName}";
                
                // Add this relationship to our tracking set
                visualizedTrades.Add(tradeKey);
                
                // Show this as an import line
                visualizer.ShowTradeLine(exporter, importer, importColor, trade.amount);
            }
        }
        
        // Then, show ALL exports (orange lines) - they'll be rendered on top
        foreach (var exportPair in allExports)
        {
            string exporterName = exportPair.Key;
            
            if (!regions.ContainsKey(exporterName)) continue;
            
            RegionEntity exporter = regions[exporterName];
            
            foreach (var trade in exportPair.Value)
            {
                if (!regions.ContainsKey(trade.partnerName)) continue;
                
                RegionEntity importer = regions[trade.partnerName];
                
                // Create a unique key for this trade relationship
                string tradeKey = $"{exporter.regionName}_{importer.regionName}_{trade.resourceName}";
                
                // Show this as an export line regardless of whether we've shown the import
                // This ensures we see both directions visually
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