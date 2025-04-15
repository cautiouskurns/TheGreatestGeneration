using UnityEngine;
using UnityEditor;
using V2.Systems;
using V2.Entities;
using V2.Components;
using System.Collections.Generic;

namespace V2.Editor
{
    /// <summary>
    /// Editor window for debugging and visualizing economic simulation parameters
    /// </summary>
    public class EconomicDebugWindow : EditorWindow
    {
        // Helper classes for different window functionality
        private EconomicParameters parameters;
        private EconomicDataHistory dataHistory;
        private EconomicDebugGraph graphHelper;
        private EconomicRegionController regionController;
        
        // References to components
        private SerializedObject serializedEconomicSystem;
        private EconomicSystem economicSystem;
        
        // Simulation settings
        private bool autoRunEnabled = false;
        private float autoRunInterval = 1.0f;
        private double lastAutoRunTime;
        private bool simulationActive = false;
        
        // Graph settings
        private Rect graphRect = new Rect(10, 300, 580, 200);
        private Rect parameterGraphRect = new Rect(10, 550, 580, 150);
        
        // Parameter group colors
        private Color productionParamsColor = new Color(0.9f, 0.7f, 0.3f, 0.2f);
        private Color cycleParamsColor = new Color(0.3f, 0.7f, 0.9f, 0.2f);
        private Color infrastructureParamsColor = new Color(0.7f, 0.3f, 0.9f, 0.2f);
        private Color populationParamsColor = new Color(0.3f, 0.9f, 0.4f, 0.2f);
        
        // Foldout states
        private bool showParameters = true;
        private bool showRegionControls = true;
        private bool showResults = true;
        private bool showGraphControls = true;
        private bool showAdvancedParameters = false;
        
        // Parameter selector
        private int selectedParameterIndex = 0;

        // Time tracking
        private int currentDay = 1;
        private int currentYear = 1;
        private double lastUIUpdateTime;
        private float uiUpdateInterval = 0.1f; // Update UI 10 times per second

        [MenuItem("Window/Economic Cycles/Debug Window")]
        public static void ShowWindow()
        {
            GetWindow<EconomicDebugWindow>("Economic Debug");
        }

        private void OnEnable()
        {
            // Initialize helper classes
            if (parameters == null) parameters = new EconomicParameters();
            if (dataHistory == null) dataHistory = new EconomicDataHistory();
            if (graphHelper == null) graphHelper = new EconomicDebugGraph();
            if (regionController == null) regionController = new EconomicRegionController();
            
            // Start editor update for auto-run
            EditorApplication.update += OnEditorUpdate;
            
            // Try to find the economic system
            FindEconomicSystem();
        }

        private void OnDisable()
        {
            // Clean up editor update
            EditorApplication.update -= OnEditorUpdate;
        }

        private void FindEconomicSystem()
        {
            // Try to find an economic system in the scene
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            
            if (economicSystem != null)
            {
                serializedEconomicSystem = new SerializedObject(economicSystem);
                SyncFromSystem();
                
                // Always record parameter history at startup
                dataHistory.RecordParameterHistory(parameters);
            }
        }

        private void OnEditorUpdate()
        {
            // Handle auto-run functionality
            if (simulationActive && autoRunEnabled && economicSystem != null)
            {
                double currentTime = EditorApplication.timeSinceStartup;
                if (currentTime - lastAutoRunTime >= autoRunInterval)
                {
                    lastAutoRunTime = currentTime;
                    RunSimulationTick();
                }
            }
            
            // Refresh UI at regular intervals regardless of simulation status
            double currentUpdateTime = EditorApplication.timeSinceStartup;
            if (currentUpdateTime - lastUIUpdateTime >= uiUpdateInterval)
            {
                lastUIUpdateTime = currentUpdateTime;
                UpdateTimeInfoFromTurnManager();
                Repaint();
            }
        }

        private void UpdateTimeInfoFromTurnManager()
        {
            if (economicSystem != null && simulationActive)
            {
                // Find TurnManager
                var turnManager = FindFirstObjectByType<V2.Core.TurnManager>();
                if (turnManager != null)
                {
                    // Try to get day and year values using reflection
                    var dayField = typeof(V2.Core.TurnManager).GetField("currentDay", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var yearField = typeof(V2.Core.TurnManager).GetField("currentYear",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                    if (dayField != null)
                        currentDay = (int)dayField.GetValue(turnManager);
                        
                    if (yearField != null)
                        currentYear = (int)yearField.GetValue(turnManager);
                }
            }
        }
                
        private void OnGUI()
        {
            // Check if we have lost our reference (can happen when Play Mode changes)
            if (economicSystem == null)
            {
                FindEconomicSystem();
            }

            if (economicSystem == null)
            {
                EditorGUILayout.HelpBox("No EconomicSystem found in the scene. Open a scene with an EconomicSystem component or enter Play Mode.", MessageType.Warning);
                if (GUILayout.Button("Find EconomicSystem"))
                {
                    FindEconomicSystem();
                }
                return;
            }

            RegionEntity region = economicSystem.testRegion;
            if (region == null)
            {
                EditorGUILayout.HelpBox("EconomicSystem found, but no test region is assigned.", MessageType.Warning);
                
                // Show Play Mode control
                DrawPlayModeControls();
                return;
            }

            // Main sections
            DrawPlayModeControls();
            DrawControlsSection();
            DrawParametersSection();
            DrawRegionControlsSection();
            DrawResultsSection(region);
            DrawGraphSection(region);
            DrawParameterGraphSection();
        }

        private void DrawPlayModeControls()
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Play Mode Controls", EditorStyles.boldLabel);
            
            simulationActive = EditorApplication.isPlaying;
            
            // Display current state
            GUI.color = simulationActive ? Color.green : Color.red;
            EditorGUILayout.LabelField("Simulation Status: " + (simulationActive ? "ACTIVE" : "INACTIVE"));
            GUI.color = Color.white;
            
            EditorGUILayout.HelpBox(
                simulationActive 
                    ? "Simulation is running. You can test parameters in real-time." 
                    : "Enter Play Mode to run the simulation. You can still adjust parameters.",
                simulationActive ? MessageType.Info : MessageType.Warning);
            
            EditorGUILayout.Space(5);
            
            // Enter/Exit play mode buttons
            EditorGUILayout.BeginHorizontal();
            
            if (!simulationActive)
            {
                if (GUILayout.Button("Enter Play Mode", GUILayout.Height(30)))
                {
                    EditorApplication.isPlaying = true;
                }
            }
            else
            {
                if (GUILayout.Button("Exit Play Mode", GUILayout.Height(30)))
                {
                    EditorApplication.isPlaying = false;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }

        private void DrawControlsSection()
        {
            EditorGUILayout.LabelField("Simulation Controls", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = simulationActive;
            
            if (GUILayout.Button("Run Single Tick", GUILayout.Height(30)))
            {
                RunSimulationTick();
            }
            
            if (GUILayout.Button("Reset Simulation", GUILayout.Height(30)))
            {
                ResetSimulation();
            }
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            
            // Auto-run section (only enable if simulation is active)
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = simulationActive;
            
            // Auto-run toggle
            bool newAutoRunEnabled = EditorGUILayout.Toggle("Auto Run", autoRunEnabled, GUILayout.Width(100));
            if (newAutoRunEnabled != autoRunEnabled)
            {
                autoRunEnabled = newAutoRunEnabled;
                lastAutoRunTime = EditorApplication.timeSinceStartup;
            }
            
            // Auto-run interval
            EditorGUILayout.LabelField("Interval (sec):", GUILayout.Width(100));
            autoRunInterval = EditorGUILayout.Slider(autoRunInterval, 0.1f, 5.0f);
            
            GUI.enabled = true;
            
            EditorGUILayout.EndHorizontal();
            
            // Sync buttons
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Sync from System"))
            {
                SyncFromSystem();
            }
            
            if (GUILayout.Button("Apply to System"))
            {
                ApplyToSystem();
            }
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(10);
        }

        private void DrawParametersSection()
        {
            showParameters = EditorGUILayout.Foldout(showParameters, "Economic Parameters", true, EditorStyles.foldoutHeader);
            
            if (showParameters)
            {
                EditorGUI.indentLevel++;
                
                // Production Parameters
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = graphHelper.MakeTexture(1, 1, productionParamsColor) } });
                EditorGUILayout.LabelField("Production Parameters", EditorStyles.boldLabel);
                
                DrawParameterSlider("Productivity Factor", ref parameters.productivityFactor, 0.1f, 5.0f, 
                    "Controls the overall productivity multiplier of the economy");
                
                DrawParameterSlider("Labor Elasticity", ref parameters.laborElasticity, 0.1f, 1.0f,
                    "How much labor affects production (Cobb-Douglas function)");
                
                DrawParameterSlider("Capital Elasticity", ref parameters.capitalElasticity, 0.1f, 1.0f,
                    "How much capital/infrastructure affects production (Cobb-Douglas function)");
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Cycle Parameters
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = graphHelper.MakeTexture(1, 1, cycleParamsColor) } });
                EditorGUILayout.LabelField("Economic Cycle Parameters", EditorStyles.boldLabel);
                
                DrawParameterSlider("Cycle Multiplier", ref parameters.cycleMultiplier, 0.8f, 1.2f,
                    "Economic cycle effect on production and wealth growth");
                
                DrawParameterSlider("Wealth Growth Rate", ref parameters.wealthGrowthRate, 0f, 20f,
                    "Base rate of wealth accumulation per tick");
                
                DrawParameterSlider("Price Volatility", ref parameters.priceVolatility, 0f, 0.5f,
                    "How much prices fluctuate randomly");
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Show/Hide Advanced Parameters
                showAdvancedParameters = EditorGUILayout.Foldout(showAdvancedParameters, "Advanced Parameters", true);
                
                if (showAdvancedParameters)
                {
                    // Infrastructure Parameters
                    EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = graphHelper.MakeTexture(1, 1, infrastructureParamsColor) } });
                    EditorGUILayout.LabelField("Infrastructure Parameters", EditorStyles.boldLabel);
                    
                    DrawParameterSlider("Decay Rate", ref parameters.decayRate, 0f, 0.05f,
                        "How quickly infrastructure deteriorates per tick");
                    
                    DrawParameterSlider("Maintenance Cost Multiplier", ref parameters.maintenanceCostMultiplier, 0f, 2f,
                        "Base cost of infrastructure maintenance");
                    
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space(5);
                    
                    // Population Parameters
                    EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = graphHelper.MakeTexture(1, 1, populationParamsColor) } });
                    EditorGUILayout.LabelField("Population Parameters", EditorStyles.boldLabel);
                    
                    DrawParameterSlider("Labor Consumption Rate", ref parameters.laborConsumptionRate, 0.5f, 3f,
                        "How much each unit of labor consumes from production");
                    
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }

        private void DrawParameterSlider(string label, ref float parameter, float min, float max, string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            
            // Add tooltip to label
            GUIContent labelContent = new GUIContent(label, tooltip);
            EditorGUILayout.LabelField(labelContent, GUILayout.Width(180));
            
            // Slider for the parameter
            float newValue = EditorGUILayout.Slider(parameter, min, max);
            if (newValue != parameter)
            {
                parameter = newValue;
                
                // Apply to system immediately to update the visualization
                ApplyToSystem();
                
                // Force a repaint to update the graph immediately
                Repaint();
            }
            
            // Display current value
            EditorGUILayout.LabelField(parameter.ToString("F2"), GUILayout.Width(40));
            
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRegionControlsSection()
        {
            showRegionControls = EditorGUILayout.Foldout(showRegionControls, "Region Controls", true, EditorStyles.foldoutHeader);
            
            if (showRegionControls)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = graphHelper.MakeTexture(1, 1, populationParamsColor) } });
                EditorGUILayout.LabelField("Population", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Labor Available:", "Amount of labor force available in the region"), GUILayout.Width(150));
                regionController.laborAvailable = EditorGUILayout.IntSlider(regionController.laborAvailable, 10, 500);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = graphHelper.MakeTexture(1, 1, infrastructureParamsColor) } });
                EditorGUILayout.LabelField("Infrastructure", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Infrastructure Level:", "Current level of infrastructure development"), GUILayout.Width(150));
                regionController.infrastructureLevel = EditorGUILayout.IntSlider(regionController.infrastructureLevel, 1, 10);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }

        private void DrawResultsSection(RegionEntity region)
        {
            showResults = EditorGUILayout.Foldout(showResults, "Simulation Results", true, EditorStyles.foldoutHeader);
            
            if (showResults)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Region: " + region.Name);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Date:", GUILayout.Width(100));
                GUIStyle dateStyle = new GUIStyle(EditorStyles.boldLabel);
                dateStyle.normal.textColor = new Color(1f, 0.7f, 0.2f);
                GUILayout.Label($"Day {currentDay}, Year {currentYear}", dateStyle);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);
                
                // Economic outputs
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Wealth:", GUILayout.Width(100));
                GUI.color = graphHelper.wealthColor;
                GUILayout.Label(region.Economy.Wealth.ToString(), EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Production:", GUILayout.Width(100));
                GUI.color = graphHelper.productionColor;
                GUILayout.Label(region.Economy.Production.ToString(), EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Satisfaction:", GUILayout.Width(100));
                GUI.color = graphHelper.satisfactionColor;
                GUILayout.Label(region.Population.Satisfaction.ToString("F2"), EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                
                // Resources
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Resources:");
                EditorGUI.indentLevel++;
                
                var resources = region.Resources.GetAllResources();
                foreach (var resource in resources)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(resource.Key + ":", GUILayout.Width(100));
                    EditorGUILayout.LabelField(resource.Value.ToString("F1"));
                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }

        private void DrawGraphSection(RegionEntity region)
        {
            // Record history data
            dataHistory.RecordHistory(region);
            
            // Graph controls
            showGraphControls = EditorGUILayout.Foldout(showGraphControls, "Graph Controls", true, EditorStyles.foldoutHeader);
            
            if (showGraphControls)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                graphHelper.showWealthGraph = EditorGUILayout.Toggle("Show Wealth", graphHelper.showWealthGraph);
                graphHelper.wealthColor = EditorGUILayout.ColorField(graphHelper.wealthColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                graphHelper.showProductionGraph = EditorGUILayout.Toggle("Show Production", graphHelper.showProductionGraph);
                graphHelper.productionColor = EditorGUILayout.ColorField(graphHelper.productionColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                graphHelper.showSatisfactionGraph = EditorGUILayout.Toggle("Show Satisfaction", graphHelper.showSatisfactionGraph);
                graphHelper.satisfactionColor = EditorGUILayout.ColorField(graphHelper.satisfactionColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                // Graph toggles for supply/demand
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Market Dynamics", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                graphHelper.showSupplyGraph = EditorGUILayout.Toggle("Show Supply", graphHelper.showSupplyGraph);
                graphHelper.supplyColor = EditorGUILayout.ColorField(graphHelper.supplyColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                graphHelper.showDemandGraph = EditorGUILayout.Toggle("Show Demand", graphHelper.showDemandGraph);
                graphHelper.demandColor = EditorGUILayout.ColorField(graphHelper.demandColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                graphHelper.showImbalanceGraph = EditorGUILayout.Toggle("Show Imbalance", graphHelper.showImbalanceGraph);
                graphHelper.imbalanceColor = EditorGUILayout.ColorField(graphHelper.imbalanceColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // Calculate graph rect based on window size
            graphRect = new Rect(90, GUILayoutUtility.GetLastRect().yMax, position.width - 100, 200);
            
            // Draw graph background
            GUI.Box(graphRect, "");
            
            // Get maximum values for graph scaling
            dataHistory.GetMaxValues(out int maxWealth, out int maxProduction, 
                out float maxSupply, out float maxDemand, out float maxImbalance);
            
            // Calculate overall max value for scaling
            float maxYValue = Mathf.Max(maxWealth, maxProduction, maxSupply, maxDemand, maxImbalance);
            
            // Draw axes
            graphHelper.DrawAxes(graphRect, "Time", "Value", maxYValue, dataHistory.wealthHistory.Count);
            
            // Draw graph data if we have at least two points
            if (dataHistory.wealthHistory.Count > 1)
            {
                // Draw wealth graph
                if (graphHelper.showWealthGraph)
                    graphHelper.DrawLineGraph(dataHistory.wealthHistory, maxYValue, graphHelper.wealthColor, graphRect);
                
                // Draw production graph
                if (graphHelper.showProductionGraph)
                    graphHelper.DrawLineGraph(dataHistory.productionHistory, maxYValue, graphHelper.productionColor, graphRect);
                
                // Draw satisfaction graph
                if (graphHelper.showSatisfactionGraph)
                    graphHelper.DrawLineGraph(dataHistory.satisfactionHistory, 1.0f, graphHelper.satisfactionColor, graphRect);
                    
                // Draw market dynamics graphs
                if (graphHelper.showSupplyGraph)
                    graphHelper.DrawLineGraph(dataHistory.supplyHistory, maxYValue, graphHelper.supplyColor, graphRect);
                    
                if (graphHelper.showDemandGraph)
                    graphHelper.DrawLineGraph(dataHistory.demandHistory, maxYValue, graphHelper.demandColor, graphRect);
                    
                if (graphHelper.showImbalanceGraph)
                    graphHelper.DrawLineGraph(dataHistory.imbalanceHistory, maxYValue, graphHelper.imbalanceColor, graphRect);
                
                // Draw legend
                DrawGraphLegend(region, graphRect);
            }
            
            // Reserve space for the graph in layout
            GUILayoutUtility.GetRect(position.width, 210);
        }
        
        private void DrawGraphLegend(RegionEntity region, Rect graphRect)
        {
            // Legend positioning and background
            float legendY = graphRect.y + 10;
            float legendX = graphRect.x + graphRect.width - 130;
            float legendWidth = 120;
            float legendHeight = 70;
            
            // Adjust height based on which graphs are shown
            if (graphHelper.showSupplyGraph) legendHeight += 20;
            if (graphHelper.showDemandGraph) legendHeight += 20;
            if (graphHelper.showImbalanceGraph) legendHeight += 20;
            
            // Semi-transparent background
            EditorGUI.DrawRect(new Rect(legendX, legendY, legendWidth, legendHeight), new Color(0.1f, 0.1f, 0.1f, 0.7f));
            
            int legendOffset = 0;
            
            if (graphHelper.showWealthGraph)
            {
                EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 10 + legendOffset, 15, 5), graphHelper.wealthColor);
                GUI.contentColor = graphHelper.wealthColor;
                GUI.Label(new Rect(legendX + 25, legendY + 5 + legendOffset, 90, 20), "Wealth: " + region.Economy.Wealth);
                GUI.contentColor = Color.white;
                legendOffset += 20;
            }
            
            if (graphHelper.showProductionGraph)
            {
                EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 10 + legendOffset, 15, 5), graphHelper.productionColor);
                GUI.contentColor = graphHelper.productionColor;
                GUI.Label(new Rect(legendX + 25, legendY + 5 + legendOffset, 90, 20), "Prod: " + region.Economy.Production);
                GUI.contentColor = Color.white;
                legendOffset += 20;
            }
            
            if (graphHelper.showSatisfactionGraph)
            {
                EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 10 + legendOffset, 15, 5), graphHelper.satisfactionColor);
                GUI.contentColor = graphHelper.satisfactionColor;
                GUI.Label(new Rect(legendX + 25, legendY + 5 + legendOffset, 90, 20), "Sat: " + region.Population.Satisfaction.ToString("F2"));
                GUI.contentColor = Color.white;
                legendOffset += 20;
            }
            
            if (graphHelper.showSupplyGraph)
            {
                EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 10 + legendOffset, 15, 5), graphHelper.supplyColor);
                GUI.contentColor = graphHelper.supplyColor;
                GUI.Label(new Rect(legendX + 25, legendY + 5 + legendOffset, 90, 20), 
                    "Supply: " + (dataHistory.supplyHistory.Count > 0 ? dataHistory.supplyHistory[dataHistory.supplyHistory.Count-1].ToString("F1") : "0"));
                GUI.contentColor = Color.white;
                legendOffset += 20;
            }
            
            if (graphHelper.showDemandGraph)
            {
                EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 10 + legendOffset, 15, 5), graphHelper.demandColor);
                GUI.contentColor = graphHelper.demandColor;
                GUI.Label(new Rect(legendX + 25, legendY + 5 + legendOffset, 90, 20), 
                    "Demand: " + (dataHistory.demandHistory.Count > 0 ? dataHistory.demandHistory[dataHistory.demandHistory.Count-1].ToString("F1") : "0"));
                GUI.contentColor = Color.white;
                legendOffset += 20;
            }
            
            if (graphHelper.showImbalanceGraph)
            {
                EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 10 + legendOffset, 15, 5), graphHelper.imbalanceColor);
                GUI.contentColor = graphHelper.imbalanceColor;
                float currentImbalance = dataHistory.imbalanceHistory.Count > 0 ? dataHistory.imbalanceHistory[dataHistory.imbalanceHistory.Count-1] : 0;
                string imbalancePrefix = currentImbalance >= 0 ? "+" : "";
                GUI.Label(new Rect(legendX + 25, legendY + 5 + legendOffset, 90, 20), 
                    "Imb: " + imbalancePrefix + currentImbalance.ToString("F1"));
                GUI.contentColor = Color.white;
            }
        }

        private void DrawParameterGraphSection()
        {
            // Parameter graph controls
            graphHelper.showParameterGraph = EditorGUILayout.Foldout(graphHelper.showParameterGraph, "Parameter History Graph", true, EditorStyles.foldoutHeader);
            
            if (graphHelper.showParameterGraph)
            {
                EditorGUI.indentLevel++;
                
                // Parameter selector
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Parameter:", GUILayout.Width(80));
                selectedParameterIndex = EditorGUILayout.Popup(selectedParameterIndex, EconomicParameters.ParameterNames);
                graphHelper.parameterColor = EditorGUILayout.ColorField(graphHelper.parameterColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                // Add a button to manually record current parameters
                if (GUILayout.Button("Record Current Parameters"))
                {
                    dataHistory.RecordParameterHistory(parameters);
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // Calculate graph rect based on window size
            parameterGraphRect = new Rect(90, GUILayoutUtility.GetLastRect().yMax, position.width - 100, 150);
            
            // Draw graph background
            GUI.Box(parameterGraphRect, "");
            
            // Get the correct history list based on selection
            List<float> parameterHistory = dataHistory.GetParameterHistoryByIndex(selectedParameterIndex);
            float maxValue = 0;
            float currentValue = 0;
            
            // Get appropriate max value based on parameter type
            switch (selectedParameterIndex)
            {
                case 0: // Productivity
                    maxValue = 5.0f;
                    currentValue = parameters.productivityFactor;
                    break;
                case 1: // Labor Elasticity
                case 2: // Capital Elasticity
                    maxValue = 1.0f;
                    currentValue = selectedParameterIndex == 1 ? parameters.laborElasticity : parameters.capitalElasticity;
                    break;
                case 3: // Cycle Multiplier
                    maxValue = 1.2f;
                    currentValue = parameters.cycleMultiplier;
                    break;
            }
            
            // Draw axes
            graphHelper.DrawAxes(parameterGraphRect, "Time", EconomicParameters.ParameterNames[selectedParameterIndex], 
                maxValue, parameterHistory.Count);
            
            // Draw parameter graph legend with current value
            float legendY = parameterGraphRect.y + 10;
            float legendX = parameterGraphRect.x + parameterGraphRect.width - 130;
            float legendWidth = 120;
            float legendHeight = 30;
            
            EditorGUI.DrawRect(new Rect(legendX, legendY, legendWidth, legendHeight), new Color(0.1f, 0.1f, 0.1f, 0.7f));
            
            GUI.contentColor = graphHelper.parameterColor;
            GUI.Label(new Rect(legendX + 10, legendY + 5, 110, 20), 
                $"{EconomicParameters.ParameterNames[selectedParameterIndex]}: {currentValue:F2}");
            GUI.contentColor = Color.white;
            
            // Draw parameter graph if we have data
            if (parameterHistory != null && parameterHistory.Count > 1)
            {
                graphHelper.DrawLineGraph(parameterHistory, maxValue, graphHelper.parameterColor, parameterGraphRect);
            }
            else if (graphHelper.showParameterGraph)
            {
                // Show message if there's no data
                string message = "No parameter history available.\nRun simulation or change parameters to record data.";
                GUI.Label(new Rect(parameterGraphRect.x + 20, parameterGraphRect.y + 50, parameterGraphRect.width - 40, 60), message);
            }
            
            // Reserve space for the graph in layout
            GUILayoutUtility.GetRect(position.width, 160);
        }

        private void RunSimulationTick()
        {
            if (economicSystem != null && simulationActive)
            {
                // Apply current values before running
                ApplyToSystem();
                
                // Run the simulation
                economicSystem.ManualTick();
                
                // Resync values
                SyncFromSystem();
                
                // Record history after the tick
                if (economicSystem.testRegion != null)
                {
                    dataHistory.RecordHistory(economicSystem.testRegion);
                    dataHistory.RecordParameterHistory(parameters);
                }
                
                // Force repaint to update graphs immediately
                Repaint();
            }
        }

        private void ResetSimulation()
        {
            if (economicSystem != null && economicSystem.testRegion != null && simulationActive)
            {
                // Use region controller to reset the region
                regionController.ResetRegion(economicSystem.testRegion);
                
                // Clear history
                dataHistory.ClearAll();
                
                // Resync values
                SyncFromSystem();
            }
        }

        private void SyncFromSystem()
        {
            if (economicSystem == null) return;
            
            // Update the serialized object
            if (serializedEconomicSystem != null)
            {
                serializedEconomicSystem.Update();
            }
            
            // Sync economic parameters
            parameters.SyncFromSystem(economicSystem);
            
            // Sync region values
            RegionEntity region = economicSystem.testRegion;
            if (region != null)
            {
                regionController.SyncFromRegion(region);
                
                // Record initial history if empty
                if (dataHistory.wealthHistory.Count == 0)
                {
                    dataHistory.RecordHistory(region);
                    dataHistory.RecordParameterHistory(parameters);
                }
            }
        }

        private void ApplyToSystem()
        {
            if (economicSystem == null) return;
            
            // Apply economic parameters
            parameters.ApplyToSystem(economicSystem);
            
            // Record parameter history when applying changes
            dataHistory.RecordParameterHistory(parameters);
            
            // Apply region values if in play mode
            if (simulationActive && economicSystem.testRegion != null)
            {
                RegionEntity region = economicSystem.testRegion;
                regionController.ApplyToRegion(region, simulationActive);
                
                // Record region history when applying changes in play mode
                dataHistory.RecordHistory(region);
            }
            
            // Mark the object as dirty to ensure values persist
            if (serializedEconomicSystem != null)
            {
                serializedEconomicSystem.ApplyModifiedProperties();
                EditorUtility.SetDirty(economicSystem);
            }
        }
    }
}