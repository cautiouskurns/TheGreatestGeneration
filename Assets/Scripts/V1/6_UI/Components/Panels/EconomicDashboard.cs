using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using V1.Utils;
using V1.Systems;
using V1.Data;
using V1.Core;
using V1.Managers;

namespace V1.UI
{   
    /// CLASS PURPOSE:
    /// EconomicDashboard manages the in-game economic overview panel, providing players
    /// with up-to-date statistics, trends, and trade data. It aggregates and visualizes
    /// resource production, consumption, population, and market behavior.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Display total wealth, production, population, and satisfaction metrics
    /// - Track and graph trends in key economic indicators
    /// - Show current market prices and recent price history
    /// - Filter and display resources, highlighting shortages
    /// - Present trade information including partners, top exports/imports, and volume
    ///
    /// KEY COLLABORATORS:
    /// - GameManager: Supplies regional data and simulation state
    /// - GameStateManager: Provides economic phase and turn tracking
    /// - ResourceMarket: Supplies current and historical pricing information
    /// - TradeSystem: Provides import/export activity for regional trade summaries
    /// - UI Components: Various graphs, dropdowns, toggles, and text elements
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Modular panel structure supports tabs for overview, resources, trends, and trade
    /// - Uses basic pooling to optimize UI list updates
    /// - Event-driven updates tied to turn processing and price changes
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Abstract panel logic into sub-classes or handlers for separation of concerns
    /// - Optimize graph and dropdown refresh logic for larger data sets
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add historical trend comparison or forecasting tools
    /// - Support region-specific views or filters
    /// - Enable export of economic data for post-game analysis
    /// 


    public class EconomicDashboard : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject dashboardPanel;
        public TabGroup tabGroup;
        
        [Header("Overview Panel")]
        public TextMeshProUGUI totalWealthText;
        public TextMeshProUGUI totalProductionText;
        public TextMeshProUGUI populationText;
        public TextMeshProUGUI satisfactionText;
        public TextMeshProUGUI economicPhaseText;
        public TextMeshProUGUI turnCountText;
        
        [Header("Resources Panel")]
        public Transform resourceListContainer;
        public GameObject resourceItemPrefab;
        public TMP_Dropdown resourceCategoryFilter;
        public Toggle showOnlyShortagesToggle;
        
        [Header("Trends Panel")]
        public SimpleLineGraph wealthGraph;
        public SimpleLineGraph productionGraph;
        public SimpleLineGraph populationGraph;
        public SimpleLineGraph marketPriceGraph;
        public TMP_Dropdown resourceForPriceGraph;
        
        [Header("Trade Panel")]
        public TextMeshProUGUI totalTradeVolumeText;
        public TextMeshProUGUI topExportText;
        public TextMeshProUGUI topImportText;
        public Transform tradePartnerContainer;
        public GameObject tradePartnerPrefab;
        
        // Data tracking for trends
        private List<int> wealthHistory = new List<int>();
        private List<int> productionHistory = new List<int>();
        private List<int> populationHistory = new List<int>();
        
        // References to other systems
        private GameManager gameManager;
        private ResourceMarket resourceMarket;
        private GameStateManager stateManager;
        
        void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();
            resourceMarket = ResourceMarket.Instance;
            stateManager = GameStateManager.Instance;
            
            // Initialize dropdown for resource categories
            if (resourceCategoryFilter != null)
            {
                resourceCategoryFilter.ClearOptions();
                
                List<string> options = new List<string>
                {
                    "All Resources",
                    "Primary",
                    "Secondary",
                    "Tertiary",
                    "Abstract"
                };
                
                resourceCategoryFilter.AddOptions(options);
                resourceCategoryFilter.onValueChanged.AddListener(OnResourceFilterChanged);
            }
            
            // Initialize toggle for shortages
            if (showOnlyShortagesToggle != null)
            {
                showOnlyShortagesToggle.onValueChanged.AddListener(OnResourceFilterChanged);
            }
            
            // Initialize resource dropdown for price graph
            if (resourceForPriceGraph != null)
            {
                UpdateResourceDropdown();
                resourceForPriceGraph.onValueChanged.AddListener(OnResourceForPriceChanged);
            }
            
            // Initialize graphs
            if (wealthGraph != null)
                wealthGraph.SetTitle("Wealth");
                
            if (productionGraph != null)
                productionGraph.SetTitle("Production");
                
            if (populationGraph != null)
                populationGraph.SetTitle("Population");
                
            if (marketPriceGraph != null)
                marketPriceGraph.SetTitle("Market Price");
            
            // Initial update
            UpdateDashboard();
        }
        
        void OnEnable()
        {
            EventBus.Subscribe("TurnProcessed", OnTurnProcessed);
            EventBus.Subscribe("ResourcePricesUpdated", OnResourcePricesUpdated);
        }
        
        void OnDisable()
        {
            EventBus.Unsubscribe("TurnProcessed", OnTurnProcessed);
            EventBus.Unsubscribe("ResourcePricesUpdated", OnResourcePricesUpdated);
        }
        
        private void OnTurnProcessed(object _)
        {
            UpdateDashboard();
        }
        
        private void OnResourcePricesUpdated(object _)
        {
            UpdateMarketPriceGraph();
        }
        
        private void OnResourceFilterChanged(int _)
        {
            UpdateResourceList();
        }
        
        private void OnResourceFilterChanged(bool _)
        {
            UpdateResourceList();
        }
        
        private void OnResourceForPriceChanged(int index)
        {
            UpdateMarketPriceGraph();
        }
        
        public void UpdateDashboard()
        {
            UpdateOverview();
            UpdateResourceList();
            UpdateTrends();
            UpdateTradeInfo();
        }
        
        private void UpdateOverview()
        {
            if (gameManager == null) return;
            
            // Get all regions
            var regions = gameManager.GetAllRegions();
            
            // Calculate totals
            int totalWealth = 0;
            int totalProduction = 0;
            int totalPopulation = 0;
            float avgSatisfaction = 0;
            int regionCount = 0;
            
            foreach (var region in regions.Values)
            {
                totalWealth += region.wealth;
                totalProduction += region.production;
                totalPopulation += region.laborAvailable;
                avgSatisfaction += region.satisfaction;
                regionCount++;
            }
            
            // Calculate average satisfaction
            avgSatisfaction = regionCount > 0 ? avgSatisfaction / regionCount : 0;
            
            // Update UI texts
            if (totalWealthText != null)
                totalWealthText.text = totalWealth.ToString("N0");
                
            if (totalProductionText != null)
                totalProductionText.text = totalProduction.ToString("N0");
                
            if (populationText != null)
                populationText.text = totalPopulation.ToString("N0");
                
            if (satisfactionText != null)
            {
                satisfactionText.text = $"{avgSatisfaction:P0}";
                satisfactionText.color = GetSatisfactionColor(avgSatisfaction);
            }
            
            // Update economic phase if state manager exists
            if (economicPhaseText != null && stateManager != null)
            {
                economicPhaseText.text = stateManager.Economy.CurrentEconomicCyclePhase;
            }
            
            // Update turn count
            if (turnCountText != null && stateManager != null)
            {
                turnCountText.text = "Turn " + stateManager.GetCurrentTurn().ToString();
            }
            
            // Update history for trends
            wealthHistory.Add(totalWealth);
            productionHistory.Add(totalProduction);
            populationHistory.Add(totalPopulation);
            
            // Limit history length
            int maxHistory = 20;
            if (wealthHistory.Count > maxHistory) wealthHistory.RemoveAt(0);
            if (productionHistory.Count > maxHistory) productionHistory.RemoveAt(0);
            if (populationHistory.Count > maxHistory) populationHistory.RemoveAt(0);
        }
        
        private void UpdateResourceList()
        {
            if (gameManager == null) return;
            
            // Create a pool of existing items for reuse
            List<GameObject> pooledItems = new List<GameObject>();
            
            // Move all existing items to our pool
            if (resourceListContainer != null)
            {
                int childCount = resourceListContainer.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    Transform child = resourceListContainer.GetChild(i);
                    if (child != null && child.gameObject != null)
                    {
                        // Disable but keep in hierarchy
                        child.gameObject.SetActive(false);
                        pooledItems.Add(child.gameObject);
                    }
                }
            }
            
            // Get all regions
            var regions = gameManager.GetAllRegions();
            
            // Aggregate all resources
            Dictionary<string, float> totalResources = new Dictionary<string, float>();
            Dictionary<string, float> totalProduction = new Dictionary<string, float>();
            Dictionary<string, float> totalConsumption = new Dictionary<string, float>();
            
            foreach (var region in regions.Values)
            {
                if (region.resources == null) continue;
                
                // Resources
                var resources = region.resources.GetAllResources();
                foreach (var entry in resources)
                {
                    if (!totalResources.ContainsKey(entry.Key))
                        totalResources[entry.Key] = 0;
                        
                    totalResources[entry.Key] += entry.Value;
                }
                
                // Production
                var production = region.resources.GetAllProductionRates();
                foreach (var entry in production)
                {
                    if (!totalProduction.ContainsKey(entry.Key))
                        totalProduction[entry.Key] = 0;
                        
                    totalProduction[entry.Key] += entry.Value;
                }
                
                // Consumption
                var consumption = region.resources.GetAllConsumptionRates();
                foreach (var entry in consumption)
                {
                    if (!totalConsumption.ContainsKey(entry.Key))
                        totalConsumption[entry.Key] = 0;
                        
                    totalConsumption[entry.Key] += entry.Value;
                }
            }
            
            // Apply filter for resource category
            List<string> filteredResources = new List<string>(totalResources.Keys);
            
            // Category filter
            if (resourceCategoryFilter != null && resourceCategoryFilter.value > 0)
            {
                ResourceDataSO.ResourceCategory targetCategory = ResourceDataSO.ResourceCategory.Primary;
                
                switch (resourceCategoryFilter.value)
                {
                    case 1: targetCategory = ResourceDataSO.ResourceCategory.Primary; break;
                    case 2: targetCategory = ResourceDataSO.ResourceCategory.Secondary; break;
                    case 3: targetCategory = ResourceDataSO.ResourceCategory.Tertiary; break;
                    case 4: targetCategory = ResourceDataSO.ResourceCategory.Abstract; break;
                }
                
                // Filter by category
                filteredResources = new List<string>();
                ResourceRegistry registry = ResourceRegistry.Instance;
                
                if (registry != null)
                {
                    foreach (var resource in totalResources.Keys)
                    {
                        ResourceDataSO resourceData = registry.GetResourceDefinition(resource);
                        if (resourceData != null && resourceData.category == targetCategory)
                        {
                            filteredResources.Add(resource);
                        }
                    }
                }
            }
            
            // Shortage filter
            if (showOnlyShortagesToggle != null && showOnlyShortagesToggle.isOn)
            {
                List<string> shortages = new List<string>();
                
                foreach (var resource in filteredResources)
                {
                    float production = totalProduction.ContainsKey(resource) ? totalProduction[resource] : 0;
                    float consumption = totalConsumption.ContainsKey(resource) ? totalConsumption[resource] : 0;
                    
                    if (consumption > production)
                    {
                        shortages.Add(resource);
                    }
                }
                
                filteredResources = shortages;
            }
            
            // Create sorted list - show shortages first
            filteredResources.Sort((a, b) => {
                float balanceA = (totalProduction.ContainsKey(a) ? totalProduction[a] : 0) - 
                            (totalConsumption.ContainsKey(a) ? totalConsumption[a] : 0);
                            
                float balanceB = (totalProduction.ContainsKey(b) ? totalProduction[b] : 0) - 
                            (totalConsumption.ContainsKey(b) ? totalConsumption[b] : 0);
                            
                // Negative balance (shortage) comes first
                return balanceA.CompareTo(balanceB);
            });
            
            // Create or reuse list items
            int itemIndex = 0;
            foreach (var resource in filteredResources)
            {
                GameObject item;
                
                // Reuse an item from the pool if available
                if (itemIndex < pooledItems.Count)
                {
                    item = pooledItems[itemIndex];
                    item.SetActive(true);
                    itemIndex++;
                }
                else
                {
                    // Create new item if needed
                    item = Instantiate(resourceItemPrefab, resourceListContainer);
                }
                
                var resourceItem = item.GetComponent<ResourceListItem>();
                if (resourceItem != null)
                {
                    float amount = totalResources[resource];
                    float production = totalProduction.ContainsKey(resource) ? totalProduction[resource] : 0;
                    float consumption = totalConsumption.ContainsKey(resource) ? totalConsumption[resource] : 0;
                    float balance = production - consumption;
                    
                    resourceItem.Setup(
                        resource,
                        amount,
                        production,
                        consumption,
                        balance
                    );
                }
            }
            
            // Deactivate any remaining pooled items that weren't used
            for (int i = itemIndex; i < pooledItems.Count; i++)
            {
                pooledItems[i].SetActive(false);
            }
            
            // Update resource dropdown for price graph
            UpdateResourceDropdown();
        }
        
        private void UpdateResourceDropdown()
        {
            if (resourceForPriceGraph == null || resourceMarket == null) return;
            
            // Save current selection
            string currentResource = "";
            if (resourceForPriceGraph.options.Count > 0 && resourceForPriceGraph.value < resourceForPriceGraph.options.Count)
            {
                currentResource = resourceForPriceGraph.options[resourceForPriceGraph.value].text;
            }
            
            // Get all resources with market prices
            Dictionary<string, float> prices = resourceMarket.GetAllCurrentPrices();
            
            // Clear and repopulate dropdown
            resourceForPriceGraph.ClearOptions();
            List<string> options = new List<string>();
            
            foreach (var resource in prices.Keys)
            {
                options.Add(resource);
            }
            
            // Sort alphabetically
            options.Sort();
            
            resourceForPriceGraph.AddOptions(options);
            
            // Restore selection if possible
            if (!string.IsNullOrEmpty(currentResource))
            {
                for (int i = 0; i < resourceForPriceGraph.options.Count; i++)
                {
                    if (resourceForPriceGraph.options[i].text == currentResource)
                    {
                        resourceForPriceGraph.value = i;
                        break;
                    }
                }
            }
        }
        
        private void UpdateTrends()
        {
            // Update general trends
            if (wealthGraph != null)
                wealthGraph.UpdateData(wealthHistory);
                
            if (productionGraph != null)
                productionGraph.UpdateData(productionHistory);
                
            if (populationGraph != null)
                populationGraph.UpdateData(populationHistory);
                
            // Update market price graph
            UpdateMarketPriceGraph();
        }
        
        private void UpdateMarketPriceGraph()
        {
            if (marketPriceGraph == null || resourceForPriceGraph == null || 
                resourceMarket == null || resourceForPriceGraph.options.Count == 0) return;
            
            string selectedResource = resourceForPriceGraph.options[resourceForPriceGraph.value].text;
            List<float> priceHistory = resourceMarket.GetPriceHistory(selectedResource);
            
            if (priceHistory.Count > 0)
            {
                marketPriceGraph.SetTitle($"{selectedResource} Price");
                marketPriceGraph.UpdateData(priceHistory);
            }
        }
        
        private void UpdateTradeInfo()
        {
            var tradeSystem = FindFirstObjectByType<TradeSystem>();
            if (tradeSystem == null) return;
            
            // Get trade data
            Dictionary<string, float> totalExports = new Dictionary<string, float>();
            Dictionary<string, float> totalImports = new Dictionary<string, float>();
            Dictionary<string, float> tradingPartners = new Dictionary<string, float>();
            
            // Get all regions
            var regions = gameManager.GetAllRegions();
            
            // Total trade volume
            int totalTradeVolume = 0;
            
            foreach (var region in regions.Values)
            {
                var exports = tradeSystem.GetRecentExports(region.regionName);
                var imports = tradeSystem.GetRecentImports(region.regionName);
                
                foreach (var export in exports)
                {
                    // Add to total exports
                    if (!totalExports.ContainsKey(export.resourceName))
                        totalExports[export.resourceName] = 0;
                        
                    totalExports[export.resourceName] += export.amount;
                    
                    // Increment trade volume
                    totalTradeVolume++;
                    
                    // Add to trading partners (outgoing)
                    if (!tradingPartners.ContainsKey(export.partnerName))
                        tradingPartners[export.partnerName] = 0;
                        
                    tradingPartners[export.partnerName] += export.amount;
                }
                
                foreach (var import in imports)
                {
                    // Add to total imports
                    if (!totalImports.ContainsKey(import.resourceName))
                        totalImports[import.resourceName] = 0;
                        
                    totalImports[import.resourceName] += import.amount;
                    
                    // Increment trade volume (we don't double count)
                    
                    // Add to trading partners (incoming)
                    if (!tradingPartners.ContainsKey(import.partnerName))
                        tradingPartners[import.partnerName] = 0;
                        
                    tradingPartners[import.partnerName] += import.amount;
                }
            }
            
            // Update total trade volume
            if (totalTradeVolumeText != null)
            {
                totalTradeVolumeText.text = totalTradeVolume.ToString();
            }
            
            // Update top exports
            if (topExportText != null)
            {
                string topExport = GetTopResource(totalExports);
                topExportText.text = string.IsNullOrEmpty(topExport) ? "None" : topExport;
            }
            
            // Update top imports
            if (topImportText != null)
            {
                string topImport = GetTopResource(totalImports);
                topImportText.text = string.IsNullOrEmpty(topImport) ? "None" : topImport;
            }
            
            // Update trade partners list
            if (tradePartnerContainer != null && tradePartnerPrefab != null)
            {
                // Clear existing items
                foreach (Transform child in tradePartnerContainer)
                {
                    Destroy(child.gameObject);
                }
                
                // Sort partners by trade volume
                List<KeyValuePair<string, float>> sortedPartners = new List<KeyValuePair<string, float>>(tradingPartners);
                sortedPartners.Sort((a, b) => b.Value.CompareTo(a.Value));
                
                // Create list items for top 5 partners
                int count = Mathf.Min(sortedPartners.Count, 5);
                for (int i = 0; i < count; i++)
                {
                    GameObject item = Instantiate(tradePartnerPrefab, tradePartnerContainer);
                    var textComp = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                    {
                        textComp.text = $"{sortedPartners[i].Key}: {sortedPartners[i].Value:F1}";
                    }
                }
            }
        }
        
        private string GetTopResource(Dictionary<string, float> resources)
        {
            string topResource = "";
            float maxAmount = 0;
            
            foreach (var entry in resources)
            {
                if (entry.Value > maxAmount)
                {
                    maxAmount = entry.Value;
                    topResource = entry.Key;
                }
            }
            
            return topResource;
        }
        
        private Color GetSatisfactionColor(float satisfaction)
        {
            if (satisfaction >= 0.8f)
                return Color.green;
            else if (satisfaction >= 0.5f)
                return Color.yellow;
            else
                return Color.red;
        }
    }
}