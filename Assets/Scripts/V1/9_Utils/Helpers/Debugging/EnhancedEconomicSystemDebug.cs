using UnityEngine;
using System.Collections.Generic;
using System.Text;
using TMPro;
using V1.Managers;
using V1.Core;
using V1.Entities;
using V1.Data;
using V1.Systems;

namespace V1.Utils
{  
    /// CLASS PURPOSE:
    /// EnhancedEconomicSystemDebug is a helper component used to verify the correctness
    /// of the EnhancedEconomicSystem during runtime by comparing economic state changes
    /// between turns and outputting debug information to the UI and console.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Track and compare regional economic states across turns
    /// - Detect and log significant economic changes (e.g., wealth, production, satisfaction)
    /// - Display debug output using TextMeshPro and log results to console
    /// - Support manual verification and turn-based update triggers
    ///
    /// KEY COLLABORATORS:
    /// - EnhancedEconomicSystem: The simulation component being debugged
    /// - GameManager: Provides access to the set of all regions
    /// - ResourceMarket: Supplies dynamic market pricing data
    /// - EventBus: Listens for "TurnEnded" events to trigger verification
    /// - TextMeshProUGUI: Renders the debug output to screen
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Captures snapshots of region state and compares on turn transitions
    /// - Allows configurable verbosity and detail level via debug toggles
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Extract state comparison logic into a utility or testing service
    /// - Introduce options for filtering which changes are shown
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Track per-resource or per-sector changes with configurable thresholds
    /// - Provide export/logging of historical debug data across multiple turns
    /// - Integrate with in-editor tools or visual overlays for easier inspection
    /// <summary>
    /// Debug component to verify the EnhancedEconomicSystem is working properly
    /// Attach this to the same GameObject as EnhancedEconomicSystem
    /// </summary>
    public class EnhancedEconomicSystemDebug : MonoBehaviour
    {
        [Header("Debug Settings")]
        public bool enableDebugLogs = true;
        public bool showDetailedLogs = false;
        public TextMeshProUGUI debugOutputText;

        [Header("Debug Tracking")]
        public int turnsSinceStart = 0;
        public List<string> recentChanges = new List<string>();
        private Dictionary<string, RegionDebugState> previousRegionStates = new Dictionary<string, RegionDebugState>();

        // Reference to the EnhancedEconomicSystem
        private EnhancedEconomicSystem economicSystem;
        private GameManager gameManager;

        // Class to track region state for comparison
        private class RegionDebugState
        {
            public string regionName;
            public int wealth;
            public int production;
            public int laborAvailable;
            public float satisfaction;
            public float infrastructureLevel;
            public Dictionary<string, float> resources = new Dictionary<string, float>();
        }

        private void Awake()
        {
            economicSystem = GetComponent<EnhancedEconomicSystem>();
            if (economicSystem == null)
            {
                Debug.LogError("EnhancedEconomicSystemDebug: No EnhancedEconomicSystem component found on this GameObject!");
                enabled = false;
            }
            
            // Add debug context menu
            gameObject.AddComponent<DebugContextMenu>();
        }

        private void Start()
        {
            gameManager = FindFirstObjectByType<GameManager>();

            // Create initial state snapshot
            CaptureCurrentState();
            
            Log("EnhancedEconomicSystemDebug initialized successfully");
            Log($"Economic Model assigned: {(economicSystem.economicModel != null ? "YES" : "NO")}");
        }

        private void OnEnable()
        {
            EventBus.Subscribe("TurnEnded", OnTurnEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
        }

        private void OnTurnEnded(object _)
        {
            // Wait a frame to let economic systems process
            Invoke("VerifyEconomicChanges", 0.1f);
        }

        // Method to verify changes have been made by the economic system
        private void VerifyEconomicChanges()
        {
            turnsSinceStart++;
            
            // Clear recent changes list
            recentChanges.Clear();
            recentChanges.Add($"=== TURN {turnsSinceStart} VERIFICATION ===");
            
            // Get current state
            Dictionary<string, RegionDebugState> currentState = CaptureCurrentState();
            
            // Check for differences since last turn
            bool changesDetected = false;
            bool regionValuesChanged = false;
            bool marketPricesChanged = false;
            
            // Skip comparison on first turn
            if (previousRegionStates.Count > 0)
            {
                // Check regions for changes
                foreach (var entry in currentState)
                {
                    string regionName = entry.Key;
                    RegionDebugState current = entry.Value;
                    
                    if (previousRegionStates.ContainsKey(regionName))
                    {
                        RegionDebugState previous = previousRegionStates[regionName];
                        
                        // Compare values
                        if (current.wealth != previous.wealth)
                        {
                            recentChanges.Add($"{regionName} Wealth: {previous.wealth} → {current.wealth} ({current.wealth - previous.wealth:+0;-#})");
                            regionValuesChanged = true;
                        }
                        
                        if (current.production != previous.production)
                        {
                            recentChanges.Add($"{regionName} Production: {previous.production} → {current.production} ({current.production - previous.production:+0;-#})");
                            regionValuesChanged = true;
                        }
                        
                        if (current.laborAvailable != previous.laborAvailable)
                        {
                            recentChanges.Add($"{regionName} Population: {previous.laborAvailable} → {current.laborAvailable} ({current.laborAvailable - previous.laborAvailable:+0;-#})");
                            regionValuesChanged = true;
                        }
                        
                        if (Mathf.Abs(current.satisfaction - previous.satisfaction) > 0.01f)
                        {
                            recentChanges.Add($"{regionName} Satisfaction: {previous.satisfaction:P0} → {current.satisfaction:P0}");
                            regionValuesChanged = true;
                        }
                        
                        if (Mathf.Abs(current.infrastructureLevel - previous.infrastructureLevel) > 0.01f)
                        {
                            recentChanges.Add($"{regionName} Infrastructure: {previous.infrastructureLevel:F2} → {current.infrastructureLevel:F2}");
                            regionValuesChanged = true;
                        }
                        
                        // Check resources if detailed logs enabled
                        if (showDetailedLogs)
                        {
                            foreach (var resourceEntry in current.resources)
                            {
                                string resourceName = resourceEntry.Key;
                                float currentAmount = resourceEntry.Value;
                                
                                if (previous.resources.ContainsKey(resourceName))
                                {
                                    float previousAmount = previous.resources[resourceName];
                                    if (Mathf.Abs(currentAmount - previousAmount) > 0.1f)
                                    {
                                        recentChanges.Add($"{regionName} {resourceName}: {previousAmount:F1} → {currentAmount:F1} ({currentAmount - previousAmount:+0.0;-0.0})");
                                        regionValuesChanged = true;
                                    }
                                }
                                else
                                {
                                    recentChanges.Add($"{regionName} New Resource {resourceName}: {currentAmount:F1}");
                                    regionValuesChanged = true;
                                }
                            }
                        }
                    }
                }
                
                // Check market prices
                ResourceMarket market = ResourceMarket.Instance;
                if (market != null)
                {
                    Dictionary<string, float> currentPrices = market.GetAllCurrentPrices();
                    foreach (var entry in currentPrices)
                    {
                        string resourceName = entry.Key;
                        float currentPrice = entry.Value;
                        float basePrice = market.GetBasePrice(resourceName);
                        
                        recentChanges.Add($"Market Price {resourceName}: {currentPrice:F2} (Base: {basePrice:F2}, Ratio: {currentPrice/basePrice:P0})");
                        marketPricesChanged = true;
                    }
                }
                
                changesDetected = regionValuesChanged || marketPricesChanged;
            }
            
            // Store current state for next comparison
            previousRegionStates = currentState;
            
            // Overall verification result
            if (changesDetected)
            {
                recentChanges.Add("✅ ENHANCED ECONOMIC SYSTEM IS WORKING");
                Log("Enhanced Economic Turn Processed - Changes Detected", true);
            }
            else
            {
                recentChanges.Add("❌ NO ECONOMIC CHANGES DETECTED - System may not be working!");
                Log("Warning: No economic changes detected after turn processing!", true);
            }
            
            // Update UI text if available
            UpdateDebugText();
        }

        // Method to capture current state of all regions
        private Dictionary<string, RegionDebugState> CaptureCurrentState()
        {
            Dictionary<string, RegionDebugState> state = new Dictionary<string, RegionDebugState>();
            
            if (gameManager == null) return state;
            
            var regions = gameManager.GetAllRegions();
            foreach (var entry in regions)
            {
                RegionEntity region = entry.Value;
                
                RegionDebugState regionState = new RegionDebugState
                {
                    regionName = region.regionName,
                    wealth = region.wealth,
                    production = region.production,
                    laborAvailable = region.laborAvailable,
                    satisfaction = region.satisfaction,
                    infrastructureLevel = region.infrastructureLevel
                };
                
                // Capture resources if available
                if (region.resources != null)
                {
                    var resources = region.resources.GetAllResources();
                    foreach (var resourceEntry in resources)
                    {
                        regionState.resources[resourceEntry.Key] = resourceEntry.Value;
                    }
                }
                
                state[region.regionName] = regionState;
            }
            
            return state;
        }

        // Method to update debug text UI
        private void UpdateDebugText()
        {
            if (debugOutputText != null)
            {
                StringBuilder sb = new StringBuilder();
                
                foreach (var line in recentChanges)
                {
                    sb.AppendLine(line);
                }
                
                debugOutputText.text = sb.ToString();
            }
        }

        // Helper method for logging
        private void Log(string message, bool forceLog = false)
        {
            if (enableDebugLogs || forceLog)
            {
                Debug.Log($"[EconomicDebug] {message}");
            }
        }

        // Class to add Debug menu option
        private class DebugContextMenu : MonoBehaviour
        {
            [ContextMenu("Debug Economic State")]
            private void DebugEconomicState()
            {
                EnhancedEconomicSystem economicSystem = GetComponent<EnhancedEconomicSystem>();
                if (economicSystem != null)
                {
                    economicSystem.DebugEconomicState();
                }
                else
                {
                    Debug.LogError("EnhancedEconomicSystem component not found!");
                }
            }
        }

        // Method for UI button to manually verify state
        public void ManualVerifyState()
        {
            VerifyEconomicChanges();
        }
    }
}