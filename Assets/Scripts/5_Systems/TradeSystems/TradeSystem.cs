using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// Refactored TradeSystem with a more modular design
public class TradeSystem : MonoBehaviour
{
    #region Configuration Properties
    
    // Trade calculation settings
    [Header("Trade Calculation")]
    [Range(1, 5)] public int tradeRadius = 2;
    [Range(0.1f, 1.0f)] public float tradeEfficiency = 0.8f;
    [Range(1, 10)] public int maxTradingPartnersPerRegion = 3;
    
    // Visualization settings
    [Header("Visualization")]
    public bool showTradeLines = true;
    public float tradeLineWidth = 0.1f;
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
        // Create the recorder first as it doesn't depend on other components
        recorder = new TradeRecorder();
        
        // Create the visualizer with configuration
        visualizer = new TradeVisualizer(
            importColor, exportColor, 
            tradeLineWidth, tradeLineDuration, 
            minimumTradeVolumeToShow,
            showTradeLines);
            
        // Initialize regions - calculator will be created after we get regions
        InitializeRegions();
    }
    
    private void OnEnable()
    {
        // Subscribe to events
        EventBus.Subscribe("TurnEnded", ProcessTrade);
        EventBus.Subscribe("RegionSelected", OnRegionSelected);
        EventBus.Subscribe("RegionEntitiesReady", OnRegionsReady);
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        EventBus.Unsubscribe("TurnEnded", ProcessTrade);
        EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
        EventBus.Unsubscribe("RegionEntitiesReady", OnRegionsReady);
    }
    
    private void Start()
    {
        // Perform a delayed initialization to ensure everything is set up
        StartCoroutine(InitializeWithDelay());
    }
    
    #endregion
    
    #region Initialization
    
    private void InitializeRegions()
    {
        // Try to get regions from GameManager
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            try
            {
                var allRegions = gameManager.GetAllRegions();
                if (allRegions != null && allRegions.Count > 0)
                {
                    regions = allRegions;
                    
                    // Now we can create the calculator
                    calculator = new TradeCalculator(
                        regions, tradeEfficiency, maxTradingPartnersPerRegion);
                    
                    Debug.Log($"TradeSystem: Initialized with {regions.Count} regions");
                    DebugTradeSystem();
                }
                else
                {
                    Debug.LogWarning("TradeSystem: No regions found from GameManager");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TradeSystem: Error initializing regions: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("TradeSystem: GameManager not found!");
        }
    }
    
    private System.Collections.IEnumerator InitializeWithDelay()
    {
        // Wait a couple of frames for everything to initialize
        yield return null;
        yield return null;
        
        // If we don't have regions yet, try again
        if (regions.Count == 0)
        {
            InitializeRegions();
        }
        
        // If we still don't have regions, warn but don't fail
        if (regions.Count == 0)
        {
            Debug.LogWarning("TradeSystem: Could not initialize regions after delay");
        }
    }
    
    private void OnRegionsReady(object regionsObj)
    {
        // Handle regions from the event system
        if (regionsObj is Dictionary<string, RegionEntity> regionDict)
        {
            regions = regionDict;
            
            // Create or update the calculator
            calculator = new TradeCalculator(
                regions, tradeEfficiency, maxTradingPartnersPerRegion);
                
            Debug.Log($"TradeSystem: Received {regions.Count} regions via event");
            DebugTradeSystem();
        }
    }
    
    #endregion
    
    #region Trade Processing
    
    private void ProcessTrade(object _)
    {
        // Clear previous trade data
        recorder.ClearTradeData();
        visualizer.ClearTradeLines();
        
        // Skip if we don't have the necessary components
        if (calculator == null || regions.Count == 0)
        {
            Debug.LogWarning("TradeSystem: Cannot process trades - missing components or regions");
            return;
        }
        
        // Calculate all trades for this turn
        List<TradeTransaction> trades = calculator.CalculateTrades();
        
        // Process each trade
        foreach (var trade in trades)
        {
            // Execute the actual resource transfer
            trade.Execute();
            
            // Record this trade for history and UI
            recorder.RecordTrade(trade);
            
            // Show visualization
            visualizer.ShowTradeLine(trade.Exporter, trade.Importer, exportColor, trade.Amount);
            
            // Log the trade
            Debug.Log($"Trade: {trade.Exporter.regionName} exported {trade.Amount} {trade.ResourceName} " +
                      $"to {trade.Importer.regionName} (received {trade.ReceivedAmount} after transport)");
        }
    }
    
    #endregion
    
    #region Event Handlers
    
    private void OnRegionSelected(object regionObj)
    {
        // Update selected region and refresh visualizations
        if (regionObj is RegionEntity region)
        {
            selectedRegionName = region.regionName;
        }
        else if (regionObj is string regionName)
        {
            selectedRegionName = regionName;
        }
        
        // Refresh the trade visualizations for the selected region
        RefreshTradeLines();
    }
    
    #endregion
    
    #region Visualization Methods
    
    // Refresh trade lines based on selection
    private void RefreshTradeLines()
    {
        visualizer.ClearTradeLines();
        
        // Skip if no trade recorder or no regions
        if (recorder == null || regions.Count == 0) return;
        
        if (!showSelectedRegionTradesOnly)
        {
            // Show all trade lines
            ShowAllTradeLines();
        }
        else if (!string.IsNullOrEmpty(selectedRegionName) && regions.ContainsKey(selectedRegionName))
        {
            // Show only trade lines for the selected region
            ShowSelectedRegionTradeLines();
        }
    }
    
    private void ShowAllTradeLines()
    {
        // Get all trades from the recorder
        Dictionary<string, List<TradeInfo>> allExports = recorder.GetAllExports();
        
        // Show each export as a line
        foreach (var exportPair in allExports)
        {
            string exporterName = exportPair.Key;
            
            if (!regions.ContainsKey(exporterName)) continue;
            
            RegionEntity exporter = regions[exporterName];
            
            foreach (var tradeInfo in exportPair.Value)
            {
                if (!regions.ContainsKey(tradeInfo.partnerName)) continue;
                
                RegionEntity importer = regions[tradeInfo.partnerName];
                
                visualizer.ShowTradeLine(exporter, importer, exportColor, tradeInfo.amount);
            }
        }
    }
    
    private void ShowSelectedRegionTradeLines()
    {
        // Get imports and exports for the selected region
        List<TradeInfo> imports = recorder.GetRecentImports(selectedRegionName);
        List<TradeInfo> exports = recorder.GetRecentExports(selectedRegionName);
        
        // Show import lines
        foreach (var tradeInfo in imports)
        {
            if (!regions.ContainsKey(tradeInfo.partnerName)) continue;
            
            RegionEntity exporter = regions[tradeInfo.partnerName];
            RegionEntity importer = regions[selectedRegionName];
            
            visualizer.ShowTradeLine(exporter, importer, importColor, tradeInfo.amount);
        }
        
        // Show export lines
        foreach (var tradeInfo in exports)
        {
            if (!regions.ContainsKey(tradeInfo.partnerName)) continue;
            
            RegionEntity exporter = regions[selectedRegionName];
            RegionEntity importer = regions[tradeInfo.partnerName];
            
            visualizer.ShowTradeLine(exporter, importer, exportColor, tradeInfo.amount);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    // Public methods for accessing trade data (for UI, etc.)
    public List<TradeInfo> GetRecentImports(string regionName)
    {
        return recorder?.GetRecentImports(regionName) ?? new List<TradeInfo>();
    }
    
    public List<TradeInfo> GetRecentExports(string regionName)
    {
        return recorder?.GetRecentExports(regionName) ?? new List<TradeInfo>();
    }
    
    // Debug method to check trade system state
    public void DebugTradeSystem()
    {
        if (regions.Count == 0)
        {
            Debug.Log("TradeSystem: No regions to debug");
            return;
        }
        
        Debug.Log($"TradeSystem: {regions.Count} regions available for trade");
        
        // Check each region for tradable resources
        foreach (var region in regions.Values)
        {
            if (region.resources == null)
            {
                Debug.LogWarning($"Region {region.regionName} has no resources component");
                continue;
            }
            
            var resources = region.resources.GetAllResources();
            var consumption = region.resources.GetAllConsumptionRates();
            
            // Calculate potential surpluses and deficits
            Dictionary<string, float> surpluses = new Dictionary<string, float>();
            Dictionary<string, float> deficits = new Dictionary<string, float>();
            
            foreach (var entry in resources)
            {
                string resourceName = entry.Key;
                float amount = entry.Value;
                float consumed = consumption.ContainsKey(resourceName) ? consumption[resourceName] : 0;
                
                if (amount > consumed * 1.2f)
                {
                    surpluses[resourceName] = amount - (consumed * 1.2f);
                }
            }
            
            foreach (var entry in consumption)
            {
                string resourceName = entry.Key;
                float needed = entry.Value;
                float available = resources.ContainsKey(resourceName) ? resources[resourceName] : 0;
                
                if (needed > available)
                {
                    deficits[resourceName] = needed - available;
                }
            }
            
            // Report on surpluses and deficits
            if (surpluses.Count > 0 || deficits.Count > 0)
            {
                Debug.Log($"Region {region.regionName}:");
                
                if (surpluses.Count > 0)
                {
                    Debug.Log("  Surpluses: " + string.Join(", ", FormatDictionary(surpluses)));
                }
                
                if (deficits.Count > 0)
                {
                    Debug.Log("  Deficits: " + string.Join(", ", FormatDictionary(deficits)));
                }
            }
        }
    }
    
    private List<string> FormatDictionary(Dictionary<string, float> dict)
    {
        List<string> result = new List<string>();
        
        foreach (var entry in dict)
        {
            result.Add($"{entry.Key}={entry.Value:F1}");
        }
        
        return result;
    }
    #endregion
}