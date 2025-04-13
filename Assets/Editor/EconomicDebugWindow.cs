using UnityEngine;
using UnityEditor;
using V2.Systems;
using V2.Entities;
using V2.Components;
using System.Collections.Generic;

namespace V2.Editor
{
    public class EconomicDebugWindow : EditorWindow
    {
        // SerializedObject for tracking editor changes
        private SerializedObject serializedEconomicSystem;
        
        // Reference to the economic system
        private EconomicSystem economicSystem;
        
        // Economic parameter values with default values
        [System.Serializable]
        private class EconomicParameters
        {
            public float productivityFactor = 1.0f;
            public float laborElasticity = 0.5f;
            public float capitalElasticity = 0.5f;
            public float cycleMultiplier = 1.05f;
            public float decayRate = 0.01f;
            public float maintenanceCostMultiplier = 0.5f;
            public float laborConsumptionRate = 1.5f;
            public float wealthGrowthRate = 5.0f;
            public float priceVolatility = 0.1f;
        }
        
        private EconomicParameters parameters = new EconomicParameters();
        
        // Region control values
        private int laborAvailable = 100;
        private int infrastructureLevel = 1;
        
        // Simulation settings
        private bool autoRunEnabled = false;
        private float autoRunInterval = 1.0f;
        private double lastAutoRunTime;
        private bool simulationActive = false;
        
        // History tracking
        private List<int> wealthHistory = new List<int>();
        private List<int> productionHistory = new List<int>();
        private List<float> satisfactionHistory = new List<float>();
        private const int maxHistoryPoints = 50;

        // Parameter history tracking
        private List<float> productivityHistory = new List<float>();
        private List<float> laborElasticityHistory = new List<float>();
        private List<float> capitalElasticityHistory = new List<float>();
        private List<float> cycleMultiplierHistory = new List<float>();
        private int selectedParameterIndex = 0;
        private string[] parameterNames = new string[] { 
            "Productivity", "Labor Elasticity", "Capital Elasticity", "Cycle Multiplier" 
        };
        private Color parameterColor = new Color(1f, 0.5f, 0.8f);
        private Rect parameterGraphRect = new Rect(10, 550, 580, 150);
        private bool showParameterGraph = true;
        
        // Graph settings
        private Rect graphRect = new Rect(10, 300, 580, 200);
        private bool showWealthGraph = true;
        private bool showProductionGraph = true;
        private bool showSatisfactionGraph = true;
        private Color wealthColor = new Color(0.2f, 0.8f, 0.2f);
        private Color productionColor = new Color(0.8f, 0.4f, 0.1f);
        private Color satisfactionColor = new Color(0.2f, 0.4f, 0.8f);
        
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
                RecordParameterHistory();
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
                
                // Production Parameters (yellow background)
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = MakeTexture(1, 1, productionParamsColor) } });
                EditorGUILayout.LabelField("Production Parameters", EditorStyles.boldLabel);
                
                DrawParameterSlider("Productivity Factor", ref parameters.productivityFactor, 0.1f, 5.0f, 
                    "Controls the overall productivity multiplier of the economy");
                
                DrawParameterSlider("Labor Elasticity", ref parameters.laborElasticity, 0.1f, 1.0f,
                    "How much labor affects production (Cobb-Douglas function)");
                
                DrawParameterSlider("Capital Elasticity", ref parameters.capitalElasticity, 0.1f, 1.0f,
                    "How much capital/infrastructure affects production (Cobb-Douglas function)");
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                // Cycle Parameters (blue background)
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = MakeTexture(1, 1, cycleParamsColor) } });
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
                    // Infrastructure Parameters (purple background)
                    EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = MakeTexture(1, 1, infrastructureParamsColor) } });
                    EditorGUILayout.LabelField("Infrastructure Parameters", EditorStyles.boldLabel);
                    
                    DrawParameterSlider("Decay Rate", ref parameters.decayRate, 0f, 0.05f,
                        "How quickly infrastructure deteriorates per tick");
                    
                    DrawParameterSlider("Maintenance Cost Multiplier", ref parameters.maintenanceCostMultiplier, 0f, 2f,
                        "Base cost of infrastructure maintenance");
                    
                    EditorGUILayout.EndVertical();
                    
                    EditorGUILayout.Space(5);
                    
                    // Population Parameters (green background)
                    EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = MakeTexture(1, 1, populationParamsColor) } });
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
                
                // Update the specific parameter history
                switch (label)
                {
                    case "Productivity Factor":
                        productivityHistory.Add(parameter);
                        if (productivityHistory.Count > maxHistoryPoints)
                            productivityHistory.RemoveAt(0);
                        break;
                    case "Labor Elasticity":
                        laborElasticityHistory.Add(parameter);
                        if (laborElasticityHistory.Count > maxHistoryPoints)
                            laborElasticityHistory.RemoveAt(0);
                        break;
                    case "Capital Elasticity":
                        capitalElasticityHistory.Add(parameter);
                        if (capitalElasticityHistory.Count > maxHistoryPoints)
                            capitalElasticityHistory.RemoveAt(0);
                        break;
                    case "Cycle Multiplier":
                        cycleMultiplierHistory.Add(parameter);
                        if (cycleMultiplierHistory.Count > maxHistoryPoints)
                            cycleMultiplierHistory.RemoveAt(0);
                        break;
                }
                
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
                
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = MakeTexture(1, 1, populationParamsColor) } });
                EditorGUILayout.LabelField("Population", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Labor Available:", "Amount of labor force available in the region"), GUILayout.Width(150));
                laborAvailable = EditorGUILayout.IntSlider(laborAvailable, 10, 500);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(5);
                
                EditorGUILayout.BeginVertical(new GUIStyle { normal = { background = MakeTexture(1, 1, infrastructureParamsColor) } });
                EditorGUILayout.LabelField("Infrastructure", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Infrastructure Level:", "Current level of infrastructure development"), GUILayout.Width(150));
                infrastructureLevel = EditorGUILayout.IntSlider(infrastructureLevel, 1, 10);
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
                GUI.color = wealthColor;
                GUILayout.Label(region.Economy.Wealth.ToString(), EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Production:", GUILayout.Width(100));
                GUI.color = productionColor;
                GUILayout.Label(region.Economy.Production.ToString(), EditorStyles.boldLabel);
                GUI.color = Color.white;
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Satisfaction:", GUILayout.Width(100));
                GUI.color = satisfactionColor;
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
            RecordHistory(region);
            
            // Graph controls
            showGraphControls = EditorGUILayout.Foldout(showGraphControls, "Graph Controls", true, EditorStyles.foldoutHeader);
            
            if (showGraphControls)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                showWealthGraph = EditorGUILayout.Toggle("Show Wealth", showWealthGraph);
                wealthColor = EditorGUILayout.ColorField(wealthColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                showProductionGraph = EditorGUILayout.Toggle("Show Production", showProductionGraph);
                productionColor = EditorGUILayout.ColorField(productionColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                showSatisfactionGraph = EditorGUILayout.Toggle("Show Satisfaction", showSatisfactionGraph);
                satisfactionColor = EditorGUILayout.ColorField(satisfactionColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // Calculate graph rect based on window size
            graphRect = new Rect(90, GUILayoutUtility.GetLastRect().yMax, position.width - 100, 200);
            
            // Draw graph background
            GUI.Box(graphRect, "");

            // Define max values for scaling
            int maxWealth = 100;
            int maxProduction = 50;

            DrawAxes(graphRect, "Time", "Value", Mathf.Max(maxWealth, maxProduction)); 
            
            // Draw graph data
            if (wealthHistory.Count > 1)
            {
                //float maxSatisfaction = 1.0f;
                
                foreach (int value in wealthHistory)
                    maxWealth = Mathf.Max(maxWealth, value);
                
                foreach (int value in productionHistory)
                    maxProduction = Mathf.Max(maxProduction, value);
                
                // Draw graph axes
                Handles.color = Color.gray;
                Handles.DrawLine(
                    new Vector3(graphRect.x, graphRect.y + graphRect.height, 0),
                    new Vector3(graphRect.x + graphRect.width, graphRect.y + graphRect.height, 0)
                );
                
                Handles.DrawLine(
                    new Vector3(graphRect.x, graphRect.y, 0),
                    new Vector3(graphRect.x, graphRect.y + graphRect.height, 0)
                );
                
                // Draw wealth graph
                if (showWealthGraph)
                    DrawLineGraph(wealthHistory, maxWealth, wealthColor);
                
                // Draw production graph
                if (showProductionGraph)
                    DrawLineGraph(productionHistory, maxProduction, productionColor);
                
                // Draw satisfaction graph
                if (showSatisfactionGraph)
                    DrawLineGraph(satisfactionHistory, 1.0f, satisfactionColor);
                
                // Draw legend in the top right corner instead of overlapping data
                float legendY = graphRect.y + 10;
                float legendX = graphRect.x + graphRect.width - 130;  // Right-aligned
                float legendWidth = 120;
                float legendHeight = 70;
                
                // Semi-transparent background
                EditorGUI.DrawRect(new Rect(legendX, legendY, legendWidth, legendHeight), new Color(0.1f, 0.1f, 0.1f, 0.7f));
                
                if (showWealthGraph)
                {
                    EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 10, 15, 5), wealthColor);
                    GUI.contentColor = wealthColor;
                    GUI.Label(new Rect(legendX + 25, legendY + 5, 90, 20), "Wealth: " + region.Economy.Wealth);
                    GUI.contentColor = Color.white;
                }
                
                if (showProductionGraph)
                {
                    EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 30, 15, 5), productionColor);
                    GUI.contentColor = productionColor;
                    GUI.Label(new Rect(legendX + 25, legendY + 25, 90, 20), "Prod: " + region.Economy.Production);
                    GUI.contentColor = Color.white;
                }
                
                if (showSatisfactionGraph)
                {
                    EditorGUI.DrawRect(new Rect(legendX + 5, legendY + 50, 15, 5), satisfactionColor);
                    GUI.contentColor = satisfactionColor;
                    GUI.Label(new Rect(legendX + 25, legendY + 45, 90, 20), "Sat: " + region.Population.Satisfaction.ToString("F2"));
                    GUI.contentColor = Color.white;
                }
            }
            
            // Reserve space for the graph in layout
            GUILayoutUtility.GetRect(position.width, 210);
        }

        private void DrawParameterGraphSection()
        {
            // Parameter graph controls
            showParameterGraph = EditorGUILayout.Foldout(showParameterGraph, "Parameter History Graph", true, EditorStyles.foldoutHeader);
            
            if (showParameterGraph)
            {
                EditorGUI.indentLevel++;
                
                // Parameter selector
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Parameter:", GUILayout.Width(80));
                selectedParameterIndex = EditorGUILayout.Popup(selectedParameterIndex, parameterNames);
                parameterColor = EditorGUILayout.ColorField(parameterColor, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                
                // Add a button to manually record current parameters
                if (GUILayout.Button("Record Current Parameters"))
                {
                    RecordParameterHistory();
                }
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(10);
            
            // Calculate graph rect based on window size
            parameterGraphRect = new Rect(90, GUILayoutUtility.GetLastRect().yMax, position.width - 100, 150);
            
            // Draw graph background regardless of data availability
            GUI.Box(parameterGraphRect, "");
            
            // Get the correct history list based on selection
            List<float> parameterHistory;
            float maxValue;
            float currentValue = 0;
            
            switch (selectedParameterIndex)
            {
                case 0:
                    parameterHistory = productivityHistory;
                    maxValue = 5.0f;
                    currentValue = parameters.productivityFactor;
                    break;
                case 1:
                    parameterHistory = laborElasticityHistory;
                    maxValue = 1.0f;
                    currentValue = parameters.laborElasticity;
                    break;
                case 2:
                    parameterHistory = capitalElasticityHistory;
                    maxValue = 1.0f;
                    currentValue = parameters.capitalElasticity;
                    break;
                case 3:
                    parameterHistory = cycleMultiplierHistory;
                    maxValue = 1.2f;
                    currentValue = parameters.cycleMultiplier;
                    break;
                default:
                    parameterHistory = productivityHistory;
                    maxValue = 5.0f;
                    currentValue = parameters.productivityFactor;
                    break;
            }
            
            // Draw axes with labels regardless of data
            DrawAxes(parameterGraphRect, "Time", parameterNames[selectedParameterIndex], maxValue);
            
            // Always show the current value
            float legendY = parameterGraphRect.y + 10;
            float legendX = parameterGraphRect.x + parameterGraphRect.width - 130;
            float legendWidth = 120;
            float legendHeight = 30;
            
            EditorGUI.DrawRect(new Rect(legendX, legendY, legendWidth, legendHeight), new Color(0.1f, 0.1f, 0.1f, 0.7f));
            
            GUI.contentColor = parameterColor;
            GUI.Label(new Rect(legendX + 10, legendY + 5, 110, 20), 
                $"{parameterNames[selectedParameterIndex]}: {currentValue:F2}");
            GUI.contentColor = Color.white;
            
            // Draw parameter graph if we have data
            if (parameterHistory != null && parameterHistory.Count > 1)
            {
                DrawLineGraph(parameterHistory, maxValue, parameterColor, parameterGraphRect);
            }
            else if (showParameterGraph)
            {
                // Show message if there's no data
                string message = "No parameter history available.\nRun simulation or change parameters to record data.";
                GUI.Label(new Rect(parameterGraphRect.x + 20, parameterGraphRect.y + 50, parameterGraphRect.width - 40, 60), message);
            }
            
            // Reserve space for the graph in layout
            GUILayoutUtility.GetRect(position.width, 160);
        }
        

        private void DrawLineGraph<T>(List<T> data, float maxValue, Color color, Rect customGraphRect = default)
        {
            if (data.Count < 2) return;
            
            Rect rect = customGraphRect.width > 0 ? customGraphRect : graphRect;
            
            Handles.color = color;
            
            float xStep = rect.width / (data.Count - 1);
            
            for (int i = 0; i < data.Count - 1; i++)
            {
                float value1 = System.Convert.ToSingle(data[i]) / maxValue;
                float value2 = System.Convert.ToSingle(data[i + 1]) / maxValue;
                
                // Clamp values to [0,1] range
                value1 = Mathf.Clamp01(value1);
                value2 = Mathf.Clamp01(value2);
                
                // Calculate points
                Vector3 p1 = new Vector3(
                    rect.x + i * xStep,
                    rect.y + rect.height - (value1 * rect.height),
                    0
                );
                
                Vector3 p2 = new Vector3(
                    rect.x + (i + 1) * xStep,
                    rect.y + rect.height - (value2 * rect.height),
                    0
                );
                
                Handles.DrawLine(p1, p2);
            }
        }

        private void DrawAxes(Rect graphRect, string xLabel, string yLabel, float maxYValue)
        {
            // Draw axes
            Handles.color = Color.gray;
            
            // X-axis
            Handles.DrawLine(
                new Vector3(graphRect.x, graphRect.y + graphRect.height, 0),
                new Vector3(graphRect.x + graphRect.width, graphRect.y + graphRect.height, 0)
            );
            
            // Y-axis
            Handles.DrawLine(
                new Vector3(graphRect.x, graphRect.y, 0),
                new Vector3(graphRect.x, graphRect.y + graphRect.height, 0)
            );
            
            // X-axis label - position it centered below x-axis
            GUI.contentColor = Color.white;
            float xLabelWidth = 60;
            float xLabelX = graphRect.x + (graphRect.width / 2) - (xLabelWidth / 2); // Center it
            GUI.Label(new Rect(xLabelX, graphRect.y + graphRect.height + 25, xLabelWidth, 20), xLabel);
            
            // Y-axis label without rotation - position it to the left of Y axis
            float yLabelWidth = 80;
            GUI.Label(new Rect(graphRect.x - yLabelWidth - 5, graphRect.y + (graphRect.height / 2) - 10, yLabelWidth, 20), yLabel);
            
            // Y-axis tick marks and values
            int tickCount = 5;
            for (int i = 0; i <= tickCount; i++)
            {
                float yPos = graphRect.y + graphRect.height - (i * (graphRect.height / tickCount));
                float value = (float)i * maxYValue / tickCount;
                
                // Draw tick
                Handles.DrawLine(
                    new Vector3(graphRect.x - 2, yPos, 0),
                    new Vector3(graphRect.x + 2, yPos, 0)
                );
                
                // Draw value - further left with right alignment
                var valueStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
                GUI.Label(new Rect(graphRect.x - 75, yPos - 10, 70, 20), value.ToString("F1"), valueStyle);
            }
            
            // Get the actual data length for x-axis (parameter or real data)
            int dataLength = 0;
            if (graphRect == parameterGraphRect)
            {
                // For parameter graph, use the selected parameter history length
                switch (selectedParameterIndex)
                {
                    case 0: dataLength = productivityHistory.Count; break;
                    case 1: dataLength = laborElasticityHistory.Count; break;
                    case 2: dataLength = capitalElasticityHistory.Count; break;
                    case 3: dataLength = cycleMultiplierHistory.Count; break;
                    default: dataLength = productivityHistory.Count; break;
                }
            }
            else
            {
                // For main graph, use wealth history length
                dataLength = wealthHistory.Count;
            }
            
            // Only draw x-axis labels if we have data
            if (dataLength > 0)
            {
                // Calculate day range
                int firstDay = currentDay - dataLength + 1;
                int lastDay = currentDay;
                
                // Determine optimal tick spacing based on data length
                int majorTickInterval;
                if (dataLength <= 10) 
                    majorTickInterval = 1;
                else if (dataLength <= 20) 
                    majorTickInterval = 5;
                else if (dataLength <= 50) 
                    majorTickInterval = 10;
                else 
                    majorTickInterval = 20;
                
                // Draw minor ticks for each data point
                for (int i = 0; i < dataLength; i++)
                {
                    // Calculate position (right-aligned, newest data on right)
                    float xPos = graphRect.x + ((float)i / (dataLength - 1)) * graphRect.width;
                    
                    // Skip if we only have one data point to avoid division by zero
                    if (dataLength == 1)
                        xPos = graphRect.x + graphRect.width / 2;
                        
                    // Draw minor tick
                    Handles.DrawLine(
                        new Vector3(xPos, graphRect.y + graphRect.height, 0),
                        new Vector3(xPos, graphRect.y + graphRect.height + 2, 0)
                    );
                }
                
                // Draw major ticks and labels
                for (int day = firstDay; day <= lastDay; day++)
                {
                    // Only draw major ticks at intervals or first/last day
                    if (day % majorTickInterval == 0 || day == firstDay || day == lastDay)
                    {
                        // Calculate position (map the day to graph position)
                        int dayIndex = day - firstDay;  // Index in our data array
                        float xPos;
                        
                        if (dataLength == 1)
                            xPos = graphRect.x + graphRect.width / 2;
                        else
                            xPos = graphRect.x + ((float)dayIndex / (dataLength - 1)) * graphRect.width;
                        
                        // Draw major tick
                        Handles.DrawLine(
                            new Vector3(xPos, graphRect.y + graphRect.height, 0),
                            new Vector3(xPos, graphRect.y + graphRect.height + 5, 0)
                        );
                        
                        // Draw day label - centered below tick
                        var valueStyle = new GUIStyle(GUI.skin.label) { 
                            alignment = TextAnchor.UpperCenter,
                            fontSize = 10
                        };
                        GUI.Label(new Rect(xPos - 20, graphRect.y + graphRect.height + 6, 40, 20), $"Day {day}", valueStyle);
                    }
                }
            }
        }


        private void RecordHistory(RegionEntity region)
        {
            if (region == null) return;
            
            // Check if values have changed to avoid duplicate records
            if (wealthHistory.Count > 0 && 
                productionHistory[productionHistory.Count - 1] == region.Economy.Production &&
                wealthHistory[wealthHistory.Count - 1] == region.Economy.Wealth &&
                Mathf.Approximately(satisfactionHistory[satisfactionHistory.Count - 1], region.Population.Satisfaction))
            {
                return;
            }
            
            // Add current values
            wealthHistory.Add(region.Economy.Wealth);
            productionHistory.Add(region.Economy.Production);
            satisfactionHistory.Add(region.Population.Satisfaction);
            
            // Keep lists at a reasonable size
            if (wealthHistory.Count > maxHistoryPoints)
            {
                wealthHistory.RemoveAt(0);
                productionHistory.RemoveAt(0);
                satisfactionHistory.RemoveAt(0);
            }
        }

        private void RecordParameterHistory()
        {
            // Add current parameter values to history
            productivityHistory.Add(parameters.productivityFactor);
            laborElasticityHistory.Add(parameters.laborElasticity);
            capitalElasticityHistory.Add(parameters.capitalElasticity);
            cycleMultiplierHistory.Add(parameters.cycleMultiplier);
            
            // Keep lists at a reasonable size
            if (productivityHistory.Count > maxHistoryPoints)
            {
                productivityHistory.RemoveAt(0);
                laborElasticityHistory.RemoveAt(0);
                capitalElasticityHistory.RemoveAt(0);
                cycleMultiplierHistory.RemoveAt(0);
            }
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

                RecordParameterHistory();
            }
        }

        private void ResetSimulation()
        {
            if (economicSystem != null && economicSystem.testRegion != null && simulationActive)
            {
                // Reset region values
                RegionEntity region = economicSystem.testRegion;
                
                // Reset component values
                region.Economy.Wealth = 100;
                region.Economy.Production = 50;
                region.Population.UpdateSatisfaction(1.0f);
                
                // Reset population and infrastructure
                region.Population.UpdateLabor(100 - region.Population.LaborAvailable);
                SetInfrastructureLevel(region.Infrastructure, 1);
                
                // Clear history
                wealthHistory.Clear();
                productionHistory.Clear();
                satisfactionHistory.Clear();
                
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
            parameters.productivityFactor = economicSystem.productivityFactor;
            parameters.laborElasticity = economicSystem.laborElasticity;
            parameters.capitalElasticity = economicSystem.capitalElasticity;
            parameters.cycleMultiplier = economicSystem.cycleMultiplier;
            
            // Sync region values
            RegionEntity region = economicSystem.testRegion;
            if (region != null)
            {
                laborAvailable = region.Population.LaborAvailable;
                infrastructureLevel = region.Infrastructure.Level;
                
                // Record initial history if empty
                if (wealthHistory.Count == 0)
                {
                    RecordHistory(region);
                    RecordParameterHistory(); // Add this line
                }
            }
        }

        private void ApplyToSystem()
        {
            if (economicSystem == null) return;
            
            // Apply economic parameters
            economicSystem.productivityFactor = parameters.productivityFactor;
            economicSystem.laborElasticity = parameters.laborElasticity;
            economicSystem.capitalElasticity = parameters.capitalElasticity;
            economicSystem.cycleMultiplier = parameters.cycleMultiplier;

            RecordParameterHistory(); // Add this line
            
            // Apply region values if in play mode
            if (simulationActive && economicSystem.testRegion != null)
            {
                RegionEntity region = economicSystem.testRegion;
                
                // Apply labor (if different)
                if (region.Population.LaborAvailable != laborAvailable)
                {
                    region.Population.UpdateLabor(laborAvailable - region.Population.LaborAvailable);
                }
                
                // Apply infrastructure (if different)
                if (region.Infrastructure.Level != infrastructureLevel)
                {
                    SetInfrastructureLevel(region.Infrastructure, infrastructureLevel);
                }
            }
            
            // Mark the object as dirty to ensure values persist
            if (serializedEconomicSystem != null)
            {
                serializedEconomicSystem.ApplyModifiedProperties();
                EditorUtility.SetDirty(economicSystem);
            }
        }

        // Helper method to set infrastructure level using reflection
        private void SetInfrastructureLevel(InfrastructureComponent component, int level)
        {
            // If we're not in play mode, we can't modify the component
            if (!simulationActive) return;
            
            // Use Upgrade method if available
            var method = typeof(InfrastructureComponent).GetMethod("Upgrade");
            if (method != null)
            {
                // Call Upgrade method until we reach desired level
                while (component.Level < level)
                {
                    method.Invoke(component, null);
                }
            }
        }

        // Utility method to create a colored texture
        private Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
    }
}