using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

// Add a new system to handle inter-region trade
public class TradeSystem : MonoBehaviour
{
    private MapModel mapModel;
    private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();

    private Dictionary<string, List<TradeInfo>> recentImports = new Dictionary<string, List<TradeInfo>>();
    private Dictionary<string, List<TradeInfo>> recentExports = new Dictionary<string, List<TradeInfo>>();

    
    // Maximum trade distance (in region hops)
    [Range(1, 5)] public int tradeRadius = 2;
    
    // Trade efficiency (0-1)
    [Range(0.1f, 1.0f)] public float tradeEfficiency = 0.8f;

    public bool showTradeLines = true;
    public float tradeLineWidth = 0.1f;
    public float tradeLineDuration = 2.0f;
    public Color importColor = new Color(0, 1, 0, 0.5f); // Semi-transparent green
    public Color exportColor = new Color(1, 0.5f, 0, 0.5f); // Semi-transparent orange

    public bool showTradeHeatmap = false;
    private Dictionary<string, int> regionTradeVolume = new Dictionary<string, int>();

    [Range(1, 10)] public int maxTradingPartnersPerRegion = 3;

    public bool showSelectedRegionTradesOnly = true;
    private string selectedRegionName = "";
    public float minimumTradeVolumeToShow = 5.0f;


private List<GameObject> activeTradeLines = new List<GameObject>();
    
    private void Awake()
    {
        // Get reference to GameManager
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            // We'll need to add a method to GameManager to get regions
//            Debug.Log("TradeSystem: Found GameManager");
        }
        else
        {
            Debug.LogError("TradeSystem: GameManager not found!");
        }

        // Clear any old trade data
        ClearTradeRecords();
        ClearTradeLines();
        ResetTradeVolumes();
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
                var allRegions = gameManager.GetAllRegions();
                if (allRegions != null)
                {
                    regions = allRegions;
//                    Debug.Log($"TradeSystem: Got {regions.Count} regions from GameManager");
                    
                    // Only call DebugTradeSystem if we have regions
                    if (regions.Count > 0)
                    {
                        DebugTradeSystem();
                    }
                }
                else
                {
                    Debug.LogWarning("TradeSystem: GameManager returned null regions dictionary");
                }
            }
            catch (System.Exception e)
            {
                // Catch any exceptions to prevent crashes
                Debug.LogError($"TradeSystem: Error getting regions from GameManager: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("TradeSystem: GameManager not found");
        }
    }

    private void OnEnable()
    {
        // Just subscribe to events
        EventBus.Subscribe("TurnEnded", ProcessTrade);
        
        // Initialize regions if needed
        if (regions == null)
        {
            regions = new Dictionary<string, RegionEntity>();
        }
        
        // Delayed initialization to avoid startup timing issues
        StartCoroutine(InitializeWithDelay());
        EventBus.Subscribe("RegionSelected", OnRegionSelected);
    }

    private System.Collections.IEnumerator InitializeWithDelay()
    {
        // Wait for two frames to ensure everything is properly initialized
        yield return null;
        yield return null;
        
//        Debug.Log("TradeSystem: Starting delayed initialization");
        
        // Get GameManager reference safely
        var gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("TradeSystem: GameManager not found during delayed initialization");
            yield break;
        }
        
//        Debug.Log("TradeSystem: Found GameManager, attempting to get regions");
        
        try
        {
            // Use reflection to check if GetAllRegions exists
            var method = gameManager.GetType().GetMethod("GetAllRegions");
            if (method == null)
            {
                Debug.LogError("TradeSystem: GameManager does not have a GetAllRegions method");
                yield break;
            }
            
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
//            Debug.Log($"TradeSystem: Successfully retrieved {regions.Count} regions");
            
            // Debug the trade system
            DebugTradeSystem();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TradeSystem: Detailed error: {e.Message}\nStack trace: {e.StackTrace}");
        }
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("RegionEntitiesReady", OnRegionsReady);
        EventBus.Unsubscribe("TurnEnded", ProcessTrade);
        EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
    }
    
    private void OnRegionsReady(object regionsObj)
    {
        // Store all regions for trade calculations
        regions = (Dictionary<string, RegionEntity>)regionsObj;
        Debug.Log($"TradeSystem: Received {regions.Count} regions via RegionEntitiesReady");
    
        // Force call DebugTradeSystem to check what resources are available
        DebugTradeSystem();
    }
    
    private void ProcessTrade(object _)
    {
        // Clear old trade data
        ClearTradeRecords();
        ClearTradeLines();
        ResetTradeVolumes();
        
        // Skip if no regions
        if (regions.Count == 0) return;
        
        // For each region, find deficits and try to import
        foreach (var region in regions.Values)
        {
            // Calculate surpluses and deficits
            Dictionary<string, float> deficits = CalculateDeficits(region);
            
            // For each deficit, try to find a trading partner
            foreach (var entry in deficits)
            {
                string resourceName = entry.Key;
                float deficitAmount = entry.Value;
                
                // Find potential trade partners within radius
                var tradingPartners = FindTradingPartners(region, resourceName);
                
                // Import from partners until deficit is met or no more partners
                ImportFromPartners(region, resourceName, deficitAmount, tradingPartners);
            }
        }
    }
        
    private Dictionary<string, float> CalculateDeficits(RegionEntity region)
    {
        Dictionary<string, float> deficits = new Dictionary<string, float>();
        
        // Get consumption rates
        var consumption = region.resources.GetAllConsumptionRates();
        
        // Get current resources
        var currentResources = region.resources.GetAllResources();
        
        // Calculate deficits (consumption > current resources)
        foreach (var entry in consumption)
        {
            string resourceName = entry.Key;
            float consumptionRate = entry.Value;
            
            float currentAmount = 0;
            if (currentResources.ContainsKey(resourceName))
                currentAmount = currentResources[resourceName];
            
            // If consumption exceeds current resource, it's a deficit
            if (consumptionRate > currentAmount)
            {
                deficits[resourceName] = consumptionRate - currentAmount;
            }
        }
        
        return deficits;
    }
    
    private List<RegionEntity> FindTradingPartners(RegionEntity region, string resourceName)
    {
        List<RegionEntity> partners = new List<RegionEntity>();
        
        // In a simple implementation, just check all regions
        // In a more complex version, you'd use a region adjacency graph
        foreach (var potentialPartner in regions.Values)
        {
            // Skip self
            if (potentialPartner.regionName == region.regionName)
                continue;
                
            // Check if partner has a surplus of this resource
            var partnerResources = potentialPartner.resources.GetAllResources();
            var partnerConsumption = potentialPartner.resources.GetAllConsumptionRates();
            
            if (partnerResources.ContainsKey(resourceName))
            {
                float available = partnerResources[resourceName];
                float needed = 0;
                
                if (partnerConsumption.ContainsKey(resourceName))
                    needed = partnerConsumption[resourceName];
                
                // If partner has more than it needs, it can trade
                if (available > needed * 1.2f) // 20% buffer
                {
                    partners.Add(potentialPartner);
                }
            }
        }
        
        return partners;
    }
    
    private void ImportFromPartners(RegionEntity importer, string resourceName, 
                                float deficitAmount, List<RegionEntity> partners)
    {
            // Sort partners by surplus amount (most surplus first)
        partners.Sort((a, b) => {
            float surplusA = CalculateSurplus(a, resourceName);
            float surplusB = CalculateSurplus(b, resourceName);
            return surplusB.CompareTo(surplusA);
        });
        
        // Limit number of partners
        if (partners.Count > maxTradingPartnersPerRegion)
            partners = partners.GetRange(0, maxTradingPartnersPerRegion);
            
        float remainingDeficit = deficitAmount;
        
        foreach (var partner in partners)
        {
            if (remainingDeficit <= 0)
                break;
                
            // Calculate how much this partner can provide
            var partnerResources = partner.resources.GetAllResources();
            var partnerConsumption = partner.resources.GetAllConsumptionRates();
            
            float partnerAvailable = partnerResources.ContainsKey(resourceName) ? 
                                    partnerResources[resourceName] : 0;
            float partnerNeeded = partnerConsumption.ContainsKey(resourceName) ? 
                                partnerConsumption[resourceName] : 0;
            
            // Skip if partner doesn't have this resource
            if (partnerAvailable <= 0) continue;
            
            float surplus = partnerAvailable - partnerNeeded * 1.2f; // Keep 20% buffer
            
            // Only trade if there's an actual surplus
            if (surplus <= 0) continue;
            
            float tradeAmount = Mathf.Min(surplus, remainingDeficit);
            
            // Apply trade efficiency (representing logistics/transport cost)
            float actualTradeAmount = tradeAmount * tradeEfficiency;
            
            // Skip meaningless trades
            if (actualTradeAmount < 0.1f) continue;
            
            // Execute the trade
            partner.resources.RemoveResource(resourceName, tradeAmount);
            importer.resources.AddResource(resourceName, actualTradeAmount);
            
            // Record this trade
            RecordTrade(partner, importer, resourceName, actualTradeAmount);
            
            // Call the visualization methods
            ShowTradeVisualization(partner, importer);
            IncrementTradeVolume(partner.regionName);
            IncrementTradeVolume(importer.regionName);
            
            Debug.Log($"Trade: {partner.regionName} exported {tradeAmount} {resourceName} to {importer.regionName} (received {actualTradeAmount} after transport)");
            
            // Generate wealth for both parties based on trade
            float tradeValue = actualTradeAmount * 0.5f; // Simple wealth generation from trade
            partner.wealth += Mathf.RoundToInt(tradeValue);
            importer.wealth += Mathf.RoundToInt(tradeValue * 0.5f); // Importer gets some wealth too
            
            // Update remaining deficit
            remainingDeficit -= actualTradeAmount;
        }
    }

    private float CalculateSurplus(RegionEntity region, string resourceName)
    {
        var resources = region.resources.GetAllResources();
        var consumption = region.resources.GetAllConsumptionRates();
        
        float available = resources.ContainsKey(resourceName) ? resources[resourceName] : 0;
        float needed = consumption.ContainsKey(resourceName) ? consumption[resourceName] : 0;
        
        return available - needed * 1.2f;
    }

    private void RecordTrade(RegionEntity exporter, RegionEntity importer, string resourceName, float amount)
    {
        // Add to importer's record
        if (!recentImports.ContainsKey(importer.regionName))
            recentImports[importer.regionName] = new List<TradeInfo>();
        
        recentImports[importer.regionName].Add(new TradeInfo { 
            partnerName = exporter.regionName,
            resourceName = resourceName,
            amount = amount
        });
        
        // Add to exporter's record
        if (!recentExports.ContainsKey(exporter.regionName))
            recentExports[exporter.regionName] = new List<TradeInfo>();
        
        recentExports[exporter.regionName].Add(new TradeInfo { 
            partnerName = importer.regionName,
            resourceName = resourceName,
            amount = amount
        });
    }

    // Clear trade records at start of each turn
    private void ClearTradeRecords()
    {
        recentImports.Clear();
        recentExports.Clear();
    }

    // Public methods to access trade data
    public List<TradeInfo> GetRecentImports(string regionName)
    {
        if (recentImports.ContainsKey(regionName))
            return recentImports[regionName];
        return new List<TradeInfo>();
    }

    public List<TradeInfo> GetRecentExports(string regionName)
    {
        if (recentExports.ContainsKey(regionName))
            return recentExports[regionName];
        return new List<TradeInfo>();
    }

    // Display a trade line between regions
    private void ShowTradeLine(RegionEntity from, RegionEntity to, Color color, float tradeAmount)
    {
            // Only show lines for significant trades
        if (tradeAmount < minimumTradeVolumeToShow)
            return;
            
        // Scale line width based on trade volume
        float lineWidth = Mathf.Clamp(tradeAmount / 20f, 0.1f, 2.0f);

        if (!showTradeLines) return;
        
        // Get region positions from GameObjects (or use stored positions)
        // You'll need to adapt this based on how your regions are positioned
        GameObject fromObj = GameObject.Find(from.regionName);
        GameObject toObj = GameObject.Find(to.regionName);
        
        if (fromObj == null || toObj == null) return;
        
        Vector3 fromPos = fromObj.transform.position;
        Vector3 toPos = toObj.transform.position;
        
        // Create line between the regions
        GameObject lineObj = new GameObject($"TradeLine_{from.regionName}_{to.regionName}");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        
        // Set line properties
        line.startWidth = tradeLineWidth;
        line.endWidth = tradeLineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.positionCount = 2;
        line.SetPosition(0, fromPos);
        line.SetPosition(1, toPos);
        
        // Elevate slightly above map
        line.sortingOrder = 10;
        
        // Add to active lines list
        activeTradeLines.Add(lineObj);

            // Set width based on volume
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        
        // Destroy after duration
        Destroy(lineObj, tradeLineDuration);
    }

    // Call this after executing a trade
    private void ShowTradeVisualization(RegionEntity exporter, RegionEntity importer)
    {
        // Find the actual amount traded from the records
        float tradeAmount = 0f;
        if (recentExports.ContainsKey(exporter.regionName))
        {
            foreach (var trade in recentExports[exporter.regionName])
            {
                if (trade.partnerName == importer.regionName)
                {
                    tradeAmount += trade.amount;
                }
            }
        }
        
        // Make sure amount is at least 1.0 to ensure visibility
        tradeAmount = Mathf.Max(1.0f, tradeAmount);
        ShowTradeLine(exporter, importer, exportColor, tradeAmount);
    }

    // Clean up any leftover lines
    private void ClearTradeLines()
    {
        foreach (var line in activeTradeLines)
        {
            if (line != null)
                Destroy(line);
        }
        activeTradeLines.Clear();
    }

    private void IncrementTradeVolume(string regionName)
    {
        if (!regionTradeVolume.ContainsKey(regionName))
            regionTradeVolume[regionName] = 0;
        
        regionTradeVolume[regionName]++;
    }

    // Get color based on trade volume
    public Color GetRegionTradeColor(string regionName)
    {
        if (!showTradeHeatmap || !regionTradeVolume.ContainsKey(regionName))
            return Color.white;
        
        // Find max trade volume for normalization
        int maxVolume = 1;
        foreach (var volume in regionTradeVolume.Values)
        {
            if (volume > maxVolume)
                maxVolume = volume;
        }
        
        // Calculate normalized intensity
        float intensity = (float)regionTradeVolume[regionName] / maxVolume;
        
        // Return color from blue (low) to red (high)
        return Color.Lerp(Color.blue, Color.red, intensity);
    }

    // Clear trade volumes each turn
    private void ResetTradeVolumes()
    {
        regionTradeVolume.Clear();
    }

    // Add to TradeSystem - call this from the Unity Editor if needed
    public void DebugTradeSystem()
    {
//        Debug.Log($"Trade System has {regions.Count} regions registered");
        
        // Check if regions have resources that can be traded
        foreach (var region in regions.Values)
        {
            var resources = region.resources.GetAllResources();
            var consumption = region.resources.GetAllConsumptionRates();
            
//            Debug.Log($"Region {region.regionName}:");
//            Debug.Log($"  Resources: {string.Join(", ", resources.Keys)}");
//            Debug.Log($"  Consumption: {string.Join(", ", consumption.Keys)}");
            
            // Calculate potential surpluses
            foreach (var entry in resources)
            {
                string resourceName = entry.Key;
                float amount = entry.Value;
                float consumed = consumption.ContainsKey(resourceName) ? consumption[resourceName] : 0;
                
                if (amount > consumed * 1.2f)
                {
//                    Debug.Log($"  SURPLUS: {resourceName} = {amount - (consumed * 1.2f):F1}");
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
//                    Debug.Log($"  DEFICIT: {resourceName} = {needed - available:F1}");
                }
            }
        }
    }

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

    private void RefreshTradeLines()
    {
        // Clear existing lines
        ClearTradeLines();
        
        // If we're showing all trade lines, redraw them all
        if (!showSelectedRegionTradesOnly)
        {
            // Draw all trade lines
            foreach (var exports in recentExports)
            {
                string exporterName = exports.Key;
                foreach (var tradeInfo in exports.Value)
                {
                    RegionEntity exporter = regions[exporterName];
                    RegionEntity importer = regions[tradeInfo.partnerName];
                    ShowTradeLine(exporter, importer, exportColor, tradeInfo.amount);
                }
            }
        }
        // Otherwise just show lines for the selected region
        else if (!string.IsNullOrEmpty(selectedRegionName) && regions.ContainsKey(selectedRegionName))
        {
            // Draw trade lines for selected region's imports
            if (recentImports.ContainsKey(selectedRegionName))
            {
                foreach (var tradeInfo in recentImports[selectedRegionName])
                {
                    RegionEntity exporter = regions[tradeInfo.partnerName];
                    RegionEntity importer = regions[selectedRegionName];
                    ShowTradeLine(exporter, importer, importColor, tradeInfo.amount);
                }
            }
            
            // Draw trade lines for selected region's exports
            if (recentExports.ContainsKey(selectedRegionName))
            {
                foreach (var tradeInfo in recentExports[selectedRegionName])
                {
                    RegionEntity exporter = regions[selectedRegionName];
                    RegionEntity importer = regions[tradeInfo.partnerName];
                    ShowTradeLine(exporter, importer, exportColor, tradeInfo.amount);
                }
            }
        }
    }
}

