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
        private Dictionary<string, Vector2> scrollPositions = new Dictionary<string, Vector2>();
        private Rect parameterGraphRect = new Rect(10, 550, 580, 150);
        private float graphHeight = 250f; // Increased default graph height
        private float graphSpacing = 40f;
        private float graphScrollViewHeight = 600f; // Explicit scroll view height for graphs
        private bool allowGraphResizing = true; // Allow the user to resize graphs
        
        // Parameter group colors
        private Color productionParamsColor = new Color(0.9f, 0.7f, 0.3f, 0.2f);
        private Color cycleParamsColor = new Color(0.3f, 0.7f, 0.9f, 0.2f);
        private Color infrastructureParamsColor = new Color(0.7f, 0.3f, 0.9f, 0.2f);
        private Color populationParamsColor = new Color(0.3f, 0.9f, 0.4f, 0.2f);
        
        // Foldout states
        private bool showParameters = true;
        private bool showRegionControls = true;
        private bool showResults = true;
        private bool showGraphSettings = true;
        private bool showAdvancedParameters = false;
        
        // Graph controls
        private Dictionary<string, bool> graphGroupFoldouts = new Dictionary<string, bool>();
        
        // Remove the old single parameter graph section data
        private int selectedParameterIndex = 0;

        // Parameter Graph settings
        private Dictionary<string, bool> parameterGroupFoldouts = new Dictionary<string, bool>();
        private Vector2 parameterGraphScrollPosition = Vector2.zero;
        private bool showParameterGraphs = true;
        private float parameterGraphHeight = 180f;
        private float parameterGraphAreaHeight = 400f;

        // Time tracking
        private int currentDay = 1;
        private int currentYear = 1;
        private double lastUIUpdateTime;
        private float uiUpdateInterval = 0.1f; // Update UI 10 times per second

        // Add this as a new field in the EconomicDebugWindow class
        private int selectedScenarioIndex = 0;
        private bool showScenarios = true;

        [MenuItem("Window/Economic Cycles/Debug Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<EconomicDebugWindow>("Economic Debug");
            // Set a larger default size so graphs are more visible
            window.minSize = new Vector2(750, 800);
        }

        private void OnEnable()
        {
            // Initialize helper classes
            if (parameters == null) parameters = new EconomicParameters();
            if (dataHistory == null) dataHistory = new EconomicDataHistory();
            if (graphHelper == null) graphHelper = new EconomicDebugGraph();
            if (regionController == null) regionController = new EconomicRegionController();
            
            // Initialize scroll positions
            if (!scrollPositions.ContainsKey("graphs"))
                scrollPositions["graphs"] = Vector2.zero;
                
            // Initialize graph foldouts
            foreach (var group in graphHelper.GetGraphGroups())
            {
                if (!graphGroupFoldouts.ContainsKey(group.Key))
                {
                    graphGroupFoldouts[group.Key] = true;
                }
            }
            
            // Initialize parameter graph foldouts
            foreach (var group in EconomicParameters.ParameterGroups)
            {
                if (!parameterGroupFoldouts.ContainsKey(group.name))
                {
                    parameterGroupFoldouts[group.name] = true;
                }
            }
            
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
                
                // Auto-record parameters at regular intervals to keep graphs in sync
                if (economicSystem != null)
                {
                    dataHistory.RecordParameterHistory(parameters);
                }
                
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
            DrawScenariosSection();
            
            // Add new dialogue testing section
            if (simulationActive)
            {
                DrawDialogueTestingSection();
            }
            
            DrawControlsSection();
            DrawParametersSection();
            DrawRegionControlsSection();
            DrawResultsSection(region);
            DrawGroupedGraphs(region);
            DrawParameterGraphsSection(); // New section for parameter graphs
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

        // Add this method after the DrawPlayModeControls method
        private void DrawScenariosSection()
        {
            showScenarios = EditorGUILayout.Foldout(showScenarios, "Economic Scenarios", true, EditorStyles.foldoutHeader);
            
            if (showScenarios)
            {
                EditorGUI.indentLevel++;
                
                // Draw scenario selector dropdown
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Select Scenario:", GUILayout.Width(120));
                int newScenario = EditorGUILayout.Popup(selectedScenarioIndex, EconomicScenarios.ScenarioNames);
                if (newScenario != selectedScenarioIndex)
                {
                    selectedScenarioIndex = newScenario;
                }
                EditorGUILayout.EndHorizontal();
                
                // Display scenario description
                EditorGUILayout.HelpBox(EconomicScenarios.GetScenarioDescription(selectedScenarioIndex), MessageType.Info);
                
                // Apply scenario button
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Apply Scenario", GUILayout.Height(30)))
                {
                    EconomicScenarios.ApplyScenario(selectedScenarioIndex, parameters, regionController);
                    
                    // Apply changes to the system and record history
                    ApplyToSystem();
                    
                    // If simulation is active, recalculate production immediately
                    if (simulationActive && economicSystem != null && economicSystem.testRegion != null)
                    {
                        RecalculateProduction(economicSystem.testRegion);
                        dataHistory.RecordHistory(economicSystem.testRegion);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUILayout.Space(5);
        }

        private void DrawDialogueTestingSection()
        {
            EditorGUILayout.Space(5);
            
            // Create a styled header for the dialogue section
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
            headerStyle.fontSize = 12;
            headerStyle.normal.textColor = new Color(0.3f, 0.5f, 0.8f);
            EditorGUILayout.LabelField("Dialogue Testing", headerStyle);
            
            // Get reference to event manager if we don't have one
            var dialogueManager = FindFirstObjectByType<V2.Systems.DialogueSystem.DialogueEventManager>();
            if (dialogueManager == null)
            {
                EditorGUILayout.HelpBox("DialogueEventManager not found in scene. Add one to test dialogues.", MessageType.Warning);
                
                if (GUILayout.Button("Create Dialogue Manager", GUILayout.Height(24)))
                {
                    // Find or create the EconomicSystem GameObject
                    GameObject systemObj = null;
                    if (economicSystem != null)
                        systemObj = economicSystem.gameObject;
                    else
                        systemObj = new GameObject("EconomicSystem");
                        
                    // Add dialogue manager
                    dialogueManager = systemObj.AddComponent<V2.Systems.DialogueSystem.DialogueEventManager>();
                    Debug.Log("Created DialogueEventManager");
                }
                
                return;
            }
            
            // Show current active dialogue
            if (dialogueManager.CurrentEvent != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Display event title with a larger, colored style
                GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel);
                titleStyle.fontSize = 12;
                titleStyle.normal.textColor = new Color(0.2f, 0.5f, 0.8f);
                EditorGUILayout.LabelField(dialogueManager.CurrentEvent.title, titleStyle);
                
                // Display description with a word-wrapped style
                GUIStyle descStyle = new GUIStyle(EditorStyles.textArea);
                descStyle.wordWrap = true;
                descStyle.fontSize = 10;
                descStyle.padding = new RectOffset(5, 5, 5, 5);
                descStyle.margin = new RectOffset(2, 2, 2, 2);
                EditorGUILayout.LabelField(dialogueManager.CurrentEvent.description, descStyle, GUILayout.Height(40));
                
                // Static variable to track the selected choice index
                int buttonSelectedIndex = -1;
                
                // Display choices section header
                EditorGUILayout.Space(3);
                EditorGUILayout.LabelField("Available Choices:", EditorStyles.miniBoldLabel);
                
                // Create a scroll view for the choices to prevent them from taking up too much space
                Vector2 choicesScrollPos = EditorGUILayout.BeginScrollView(
                    new Vector2(0, 0), 
                    GUILayout.Height(Mathf.Min(120, dialogueManager.CurrentEvent.choices.Count * 31)), 
                    GUILayout.ExpandWidth(true)
                );
                
                // Create choice buttons with improved styling
                for (int i = 0; i < dialogueManager.CurrentEvent.choices.Count; i++)
                {
                    var choice = dialogueManager.CurrentEvent.choices[i];
                    
                    // Create a custom button style
                    GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
                    buttonStyle.alignment = TextAnchor.MiddleLeft;
                    buttonStyle.wordWrap = true;
                    buttonStyle.padding = new RectOffset(8, 8, 4, 4);
                    buttonStyle.margin = new RectOffset(2, 2, 2, 2);
                    buttonStyle.fixedHeight = 28; // Keep buttons compact
                    
                    // Highlight the button if it's the one that would be selected
                    if (i == buttonSelectedIndex)
                    {
                        buttonStyle.normal.background = MakeColorTexture(new Color(0.6f, 0.8f, 1f, 0.5f));
                        buttonStyle.hover.background = MakeColorTexture(new Color(0.7f, 0.9f, 1f, 0.6f));
                    }
                    
                    // Add a visual indicator if this choice leads to another dialogue
                    string buttonText = choice.text;
                    if (!string.IsNullOrEmpty(choice.nextEventId))
                    {
                        buttonText = "» " + buttonText;
                    }
                    
                    // Truncate long button text with ellipsis
                    if (buttonText.Length > 70)
                    {
                        buttonText = buttonText.Substring(0, 70) + "...";
                    }
                    
                    // Create the button with the improved style
                    if (GUILayout.Button(buttonText, buttonStyle))
                    {
                        buttonSelectedIndex = i; // Mark this button as selected
                        
                        // Make the selection and close the GUI briefly to avoid errors
                        dialogueManager.MakeChoice(i);
                        
                        // Debug what's about to happen
                        string nextEventId = choice.nextEventId;
                        if (!string.IsNullOrEmpty(nextEventId))
                        {
                            Debug.Log($"Choice made with nextEventId: {nextEventId}");
                        }
                        else
                        {
                            Debug.Log("Choice made with no nextEventId");
                        }
                        
                        GUIUtility.ExitGUI(); // Exit the GUI to avoid errors when the event changes
                    }
                    
                    // Check for hover to show tooltip
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    if (lastRect.Contains(Event.current.mousePosition))
                    {
                        // Show tooltip on hover with effect info
                        if (choice.economicEffects != null && choice.economicEffects.Count > 0)
                        {
                            string tooltip = "Economic Effects:";
                            foreach (var effect in choice.economicEffects)
                            {
                                tooltip += $"\n• {effect.description}";
                            }
                            GUI.Label(new Rect(Event.current.mousePosition.x + 10, Event.current.mousePosition.y, 180, 80), 
                                      tooltip, EditorStyles.helpBox);
                        }
                    }
                }
                
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.HelpBox("No active dialogue event.", MessageType.Info);
            }
            
            // Test dialogue trigger buttons in a horizontal layout
            EditorGUILayout.Space(5);
            
            EditorGUILayout.LabelField("Trigger Test Events:", EditorStyles.miniBoldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            GUIStyle triggerButtonStyle = new GUIStyle(GUI.skin.button);
            triggerButtonStyle.padding = new RectOffset(8, 8, 4, 4);
            triggerButtonStyle.margin = new RectOffset(2, 2, 1, 1);
            triggerButtonStyle.fixedHeight = 22;
            
            if (GUILayout.Button("Resource Shortage", triggerButtonStyle, GUILayout.Width(130)))
            {
                dialogueManager.TriggerEvent("resource_shortage");
            }
            
            if (GUILayout.Button("Economic Reform", triggerButtonStyle, GUILayout.Width(130)))
            {
                dialogueManager.TriggerEvent("economic_reform_proposal");
            }
            
            EditorGUILayout.EndHorizontal();
            
            // Button to reset all events
            if (GUILayout.Button("Reset All Events", GUILayout.Height(22)))
            {
                dialogueManager.ResetAllEvents();
            }
            
            EditorGUILayout.Space(5);
        }

        // Helper method to create a colored texture for UI elements
        private Texture2D MakeColorTexture(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
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

        private void DrawGraphSettings()
        {
            showGraphSettings = EditorGUILayout.Foldout(showGraphSettings, "Graph Settings", true, EditorStyles.foldoutHeader);
            
            if (showGraphSettings)
            {
                EditorGUI.indentLevel++;
                
                // Graph size controls
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Graph Height:", GUILayout.Width(100));
                graphHeight = EditorGUILayout.Slider(graphHeight, 150f, 500f);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Graph Area Height:", GUILayout.Width(120));
                graphScrollViewHeight = EditorGUILayout.Slider(graphScrollViewHeight, 300f, 1000f);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space(10);
                
                // Configure groups
                foreach (var group in graphHelper.GetGraphGroups())
                {
                    string groupId = group.Key;
                    GraphGroup graphGroup = group.Value;
                    
                    // Group visibility and settings
                    EditorGUILayout.BeginHorizontal();
                    graphGroup.isVisible = EditorGUILayout.ToggleLeft(graphGroup.name, graphGroup.isVisible, EditorStyles.boldLabel);
                    EditorGUILayout.EndHorizontal();
                    
                    if (graphGroup.isVisible)
                    {
                        EditorGUI.indentLevel++;
                        
                        // Custom scale settings
                        EditorGUILayout.BeginHorizontal();
                        graphGroup.customScale = EditorGUILayout.ToggleLeft("Custom Scale", graphGroup.customScale);
                        if (graphGroup.customScale)
                        {
                            graphGroup.customMaxValue = EditorGUILayout.FloatField(graphGroup.customMaxValue);
                        }
                        EditorGUILayout.EndHorizontal();
                        
                        // Dual axis settings
                        EditorGUILayout.BeginHorizontal();
                        graphGroup.useDualAxis = EditorGUILayout.ToggleLeft("Use Dual Axis", graphGroup.useDualAxis);
                        
                        if (graphGroup.useDualAxis)
                        {
                            // Secondary axis label
                            graphGroup.secondaryAxisLabel = EditorGUILayout.TextField(
                                graphGroup.secondaryAxisLabel.Length > 0 ? graphGroup.secondaryAxisLabel : "Secondary Value");
                                
                            // Secondary max value field
                            EditorGUILayout.LabelField("Max Value:", GUILayout.Width(70));
                            graphGroup.secondaryMaxValue = EditorGUILayout.FloatField(graphGroup.secondaryMaxValue);
                        }
                        
                        EditorGUILayout.EndHorizontal();
                        
                        // If dual axis is enabled, show which metrics go on which axis
                        if (graphGroup.useDualAxis)
                        {
                            EditorGUILayout.Space(5);
                            EditorGUILayout.LabelField("Configure Metrics For Each Axis:", EditorStyles.boldLabel);
                            
                            foreach (var metric in graphGroup.metrics)
                            {
                                // Initialize if not exists
                                if (!graphGroup.secondaryAxisMetrics.ContainsKey(metric.Key))
                                {
                                    // Default certain metrics to secondary axis based on type
                                    bool useSecondaryByDefault = false;
                                    
                                    // Default satisfaction to secondary axis in economic group
                                    if (group.Key == "economic" && metric.Key == "satisfaction")
                                        useSecondaryByDefault = true;
                                    
                                    // Default imbalance to secondary axis in market group
                                    if (group.Key == "market" && metric.Key == "imbalance")
                                        useSecondaryByDefault = true;
                                    
                                    graphGroup.secondaryAxisMetrics[metric.Key] = useSecondaryByDefault;
                                }
                                
                                // Draw the secondary axis toggle for this metric
                                EditorGUILayout.BeginHorizontal();
                                GUI.color = metric.Value.color;
                                EditorGUILayout.LabelField(metric.Value.name + ":", GUILayout.Width(100));
                                GUI.color = Color.white;
                                
                                bool useSecondary = graphGroup.secondaryAxisMetrics[metric.Key];
                                string axisLabel = useSecondary ? "Secondary Axis" : "Primary Axis";
                                
                                if (GUILayout.Button(axisLabel, GUILayout.Width(120)))
                                {
                                    // Toggle between primary and secondary axis
                                    graphGroup.secondaryAxisMetrics[metric.Key] = !useSecondary;
                                }
                                
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                        
                        // Toggle individual metrics
                        foreach (var metric in graphGroup.metrics)
                        {
                            EditorGUILayout.BeginHorizontal();
                            metric.Value.isVisible = EditorGUILayout.ToggleLeft(metric.Value.name, metric.Value.isVisible);
                            metric.Value.color = EditorGUILayout.ColorField(metric.Value.color, GUILayout.Width(50));
                            EditorGUILayout.EndHorizontal();
                        }
                        
                        EditorGUI.indentLevel--;
                    }
                    
                    EditorGUILayout.Space(5);
                }
                
                EditorGUI.indentLevel--;
            }
        }

        private void DrawGroupedGraphs(RegionEntity region)
        {
            // Record history data
            dataHistory.RecordHistory(region);
            
            // Graph settings
            DrawGraphSettings();

            // Create a box around the graph area to visually distinguish it
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Begin scrollable area for all graphs with a fixed height
            scrollPositions["graphs"] = EditorGUILayout.BeginScrollView(
                scrollPositions["graphs"], 
                GUILayout.Height(graphScrollViewHeight), 
                GUILayout.ExpandWidth(true)
            );
            
            float totalGraphHeight = 0;
            
            // Create a dictionary to hold current values for each graph metric
            Dictionary<string, Dictionary<string, object>> currentValues = new Dictionary<string, Dictionary<string, object>>();
            
            // Populate current values for the legends
            currentValues["economic"] = new Dictionary<string, object>();
            currentValues["economic"]["wealth"] = region.Economy.Wealth;
            currentValues["economic"]["production"] = region.Economy.Production;
            
            currentValues["population"] = new Dictionary<string, object>();
            currentValues["population"]["satisfaction"] = region.Population.Satisfaction;
            
            currentValues["market"] = new Dictionary<string, object>();
            float supply = dataHistory.supplyHistory.Count > 0 ? dataHistory.supplyHistory[dataHistory.supplyHistory.Count - 1] : 0f;
            float demand = dataHistory.demandHistory.Count > 0 ? dataHistory.demandHistory[dataHistory.demandHistory.Count - 1] : 0f;
            float imbalance = dataHistory.imbalanceHistory.Count > 0 ? dataHistory.imbalanceHistory[dataHistory.imbalanceHistory.Count - 1] : 0f;
            currentValues["market"]["supply"] = supply;
            currentValues["market"]["demand"] = demand;
            currentValues["market"]["imbalance"] = imbalance;
            
            // Draw each graph group
            foreach (var group in graphHelper.GetGraphGroups())
            {
                GraphGroup graphGroup = group.Value;
                
                // Skip if not visible
                if (!graphGroup.isVisible) continue;
                
                // Add space between graphs with horizontal line
                if (totalGraphHeight > 0)
                {
                    EditorGUILayout.Space(5);
                    Rect lineRect = EditorGUILayout.GetControlRect(GUILayout.Height(2));
                    EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
                    EditorGUILayout.Space(5);
                }
                
                // Calculate appropriate max value
                float maxValue = graphGroup.GetEffectiveMaxValue();
                
                // Calculate dynamic max values based on data if not using custom scale
                if (!graphGroup.customScale)
                {
                    switch (group.Key)
                    {
                        case "economic":
                            int maxWealth = 100;
                            int maxProduction = 50;
                            foreach (int value in dataHistory.wealthHistory)
                                maxWealth = Mathf.Max(maxWealth, value);
                            foreach (int value in dataHistory.productionHistory)
                                maxProduction = Mathf.Max(maxProduction, value);
                            
                            maxValue = Mathf.Max(maxWealth, maxProduction) * 1.1f; // Add 10% headroom
                            break;
                            
                        case "population":
                            // Satisfaction is always between 0-1, so keep default
                            maxValue = 1.0f;
                            break;
                            
                        case "market":
                            float maxSupply = 50f;
                            float maxDemand = 50f;
                            float maxImbalance = 50f;
                            
                            foreach (float value in dataHistory.supplyHistory)
                                maxSupply = Mathf.Max(maxSupply, value);
                            foreach (float value in dataHistory.demandHistory)
                                maxDemand = Mathf.Max(maxDemand, value);
                            foreach (float value in dataHistory.imbalanceHistory)
                                maxImbalance = Mathf.Max(maxImbalance, Mathf.Abs(value));
                                
                            maxValue = Mathf.Max(maxSupply, maxDemand, maxImbalance) * 1.1f; // Add 10% headroom
                            break;
                    }
                }

                // Draw titled box for graph
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Draw graph title with its color in a styled header
                GUI.color = graphGroup.titleColor;
                EditorGUILayout.LabelField(graphGroup.name + " Graph", EditorStyles.boldLabel);
                GUI.color = Color.white;
                
                // Calculate graph rect - make it wider by using the full width minus margins
                float graphX = 90; // Left margin
                Rect lastRect = EditorGUILayout.GetControlRect(GUILayout.Height(graphHeight));
                graphGroup.graphRect = new Rect(
                    graphX,
                    lastRect.y,
                    position.width - graphX - 20, // Right margin
                    graphHeight
                );
                
                // Draw graph background
                GUI.Box(graphGroup.graphRect, "");
                
                // Draw axes - choose between regular or dual axes based on settings
                if (graphGroup.useDualAxis)
                {
                    graphHelper.DrawDualAxes(graphGroup.graphRect, "Time", 
                        graphGroup.yAxisLabel, 
                        graphGroup.secondaryAxisLabel.Length > 0 ? graphGroup.secondaryAxisLabel : "Secondary Value", 
                        maxValue, 
                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                        dataHistory.wealthHistory.Count);
                }
                else
                {
                    graphHelper.DrawAxes(graphGroup.graphRect, "Time", graphGroup.yAxisLabel, maxValue, dataHistory.wealthHistory.Count);
                }
                
                // Draw data if we have at least two points
                if (dataHistory.wealthHistory.Count > 1)
                {
                    // Draw the appropriate data for this group
                    switch (group.Key)
                    {
                        case "economic":
                            // Economic data with dual axis option (large values vs percentages)
                            if (graphGroup.metrics["wealth"].isVisible)
                            {
                                bool useSecondary = graphGroup.useDualAxis && 
                                    graphGroup.secondaryAxisMetrics.ContainsKey("wealth") && 
                                    graphGroup.secondaryAxisMetrics["wealth"];
                                    
                                if (useSecondary)
                                    graphHelper.DrawLineGraphSecondaryAxis(dataHistory.wealthHistory, 
                                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                                        graphGroup.metrics["wealth"].color, graphGroup.graphRect);
                                else
                                    graphHelper.DrawLineGraph(dataHistory.wealthHistory, maxValue, 
                                        graphGroup.metrics["wealth"].color, graphGroup.graphRect);
                            }
                                
                            if (graphGroup.metrics["production"].isVisible)
                            {
                                bool useSecondary = graphGroup.useDualAxis && 
                                    graphGroup.secondaryAxisMetrics.ContainsKey("production") && 
                                    graphGroup.secondaryAxisMetrics["production"];
                                    
                                if (useSecondary)
                                    graphHelper.DrawLineGraphSecondaryAxis(dataHistory.productionHistory, 
                                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                                        graphGroup.metrics["production"].color, graphGroup.graphRect);
                                else
                                    graphHelper.DrawLineGraph(dataHistory.productionHistory, maxValue, 
                                        graphGroup.metrics["production"].color, graphGroup.graphRect);
                            }
                            
                            // Include satisfaction in economic graph if configured
                            if (graphGroup.metrics.ContainsKey("satisfaction") && graphGroup.metrics["satisfaction"].isVisible)
                            {
                                bool useSecondary = graphGroup.useDualAxis && 
                                    (!graphGroup.secondaryAxisMetrics.ContainsKey("satisfaction") || 
                                    graphGroup.secondaryAxisMetrics["satisfaction"]);
                                    
                                if (useSecondary)
                                    graphHelper.DrawLineGraphSecondaryAxis(dataHistory.satisfactionHistory, 
                                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                                        graphGroup.metrics["satisfaction"].color, graphGroup.graphRect);
                                else
                                    graphHelper.DrawLineGraph(dataHistory.satisfactionHistory, maxValue, 
                                        graphGroup.metrics["satisfaction"].color, graphGroup.graphRect);
                            }
                            break;
                            
                        case "population":
                            // Population data
                            if (graphGroup.metrics["satisfaction"].isVisible)
                            {
                                bool useSecondary = graphGroup.useDualAxis && 
                                    graphGroup.secondaryAxisMetrics.ContainsKey("satisfaction") && 
                                    graphGroup.secondaryAxisMetrics["satisfaction"];
                                    
                                if (useSecondary)
                                    graphHelper.DrawLineGraphSecondaryAxis(dataHistory.satisfactionHistory, 
                                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                                        graphGroup.metrics["satisfaction"].color, graphGroup.graphRect);
                                else
                                    graphHelper.DrawLineGraph(dataHistory.satisfactionHistory, maxValue, 
                                        graphGroup.metrics["satisfaction"].color, graphGroup.graphRect);
                            }
                            break;
                            
                        case "market":
                            // Market data with dual axis option (supply/demand vs imbalance)
                            if (graphGroup.metrics["supply"].isVisible)
                            {
                                bool useSecondary = graphGroup.useDualAxis && 
                                    graphGroup.secondaryAxisMetrics.ContainsKey("supply") && 
                                    graphGroup.secondaryAxisMetrics["supply"];
                                    
                                if (useSecondary)
                                    graphHelper.DrawLineGraphSecondaryAxis(dataHistory.supplyHistory, 
                                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                                        graphGroup.metrics["supply"].color, graphGroup.graphRect);
                                else
                                    graphHelper.DrawLineGraph(dataHistory.supplyHistory, maxValue, 
                                        graphGroup.metrics["supply"].color, graphGroup.graphRect);
                            }
                                
                            if (graphGroup.metrics["demand"].isVisible)
                            {
                                bool useSecondary = graphGroup.useDualAxis && 
                                    graphGroup.secondaryAxisMetrics.ContainsKey("demand") && 
                                    graphGroup.secondaryAxisMetrics["demand"];
                                    
                                if (useSecondary)
                                    graphHelper.DrawLineGraphSecondaryAxis(dataHistory.demandHistory, 
                                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                                        graphGroup.metrics["demand"].color, graphGroup.graphRect);
                                else
                                    graphHelper.DrawLineGraph(dataHistory.demandHistory, maxValue, 
                                        graphGroup.metrics["demand"].color, graphGroup.graphRect);
                            }
                                
                            if (graphGroup.metrics["imbalance"].isVisible)
                            {
                                bool useSecondary = graphGroup.useDualAxis && 
                                    (!graphGroup.secondaryAxisMetrics.ContainsKey("imbalance") || 
                                    graphGroup.secondaryAxisMetrics["imbalance"]);
                                    
                                if (useSecondary)
                                    graphHelper.DrawLineGraphSecondaryAxis(dataHistory.imbalanceHistory, 
                                        graphGroup.GetEffectiveSecondaryMaxValue(), 
                                        graphGroup.metrics["imbalance"].color, graphGroup.graphRect);
                                else
                                    graphHelper.DrawLineGraph(dataHistory.imbalanceHistory, maxValue, 
                                        graphGroup.metrics["imbalance"].color, graphGroup.graphRect);
                            }
                            break;
                    }
                    
                    // Draw legend for this graph
                    graphHelper.DrawLegend(graphGroup.graphRect, 
                        currentValues.ContainsKey(group.Key) ? currentValues[group.Key] : new Dictionary<string, object>(), 
                        graphGroup.metrics);
                }
                else
                {
                    // Show message if there's no data
                    string message = "No data available.\nRun simulation to record data.";
                    GUI.Label(new Rect(graphGroup.graphRect.x + 20, graphGroup.graphRect.y + 50, graphGroup.graphRect.width - 40, 60), message);
                }
                
                // End this graph's vertical group
                EditorGUILayout.EndVertical();
                
                // Calculate total height used
                totalGraphHeight += graphHeight + 50; // graph height + spacing and headers
            }
            
            // Add some padding at the bottom
            EditorGUILayout.Space(20);
            
            // End scroll view and outer box
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            
            // Add helpful note about scrolling
            EditorGUILayout.HelpBox("Scroll to see all graphs. Adjust graph height and area size using the controls in Graph Settings.", MessageType.Info);
            
            EditorGUILayout.Space(10);
        }

        private void DrawParameterGraphsSection()
        {
            // Always record parameter history to keep in sync with other graphs
            if (economicSystem != null && parameters != null)
            {
                dataHistory.RecordParameterHistory(parameters);
            }
            
            // Parameter graph controls
            showParameterGraphs = EditorGUILayout.Foldout(showParameterGraphs, "Parameter History Graphs", true, EditorStyles.foldoutHeader);
            
            if (showParameterGraphs)
            {
                EditorGUI.indentLevel++;
                
                // Graph size controls
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Graph Height:", GUILayout.Width(100));
                parameterGraphHeight = EditorGUILayout.Slider(parameterGraphHeight, 120f, 300f);
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Graph Area Height:", GUILayout.Width(120));
                parameterGraphAreaHeight = EditorGUILayout.Slider(parameterGraphAreaHeight, 200f, 800f);
                EditorGUILayout.EndHorizontal();
                
                // Button to manually record current parameters
                if (GUILayout.Button("Record Current Parameters"))
                {
                    dataHistory.RecordParameterHistory(parameters);
                }
                
                EditorGUI.indentLevel--;
                
                // Create a box around the parameter graph area
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                // Begin scrollable area for parameter graphs
                parameterGraphScrollPosition = EditorGUILayout.BeginScrollView(
                    parameterGraphScrollPosition, 
                    GUILayout.Height(parameterGraphAreaHeight), 
                    GUILayout.ExpandWidth(true)
                );
                
                // Get current parameter values for legend display
                Dictionary<string, float> currentParameterValues = parameters.GetAllParameters();
                
                // Get the common x-axis point count - use the same count as the main graphs
                // This ensures all graphs have the same time scale
                int pointCount = dataHistory.wealthHistory.Count;
                
                // Create custom parameter groupings for better scale compatibility
                Dictionary<string, ParameterGraphGroup> customGroups = CreateCustomParameterGroups();
                
                // Draw custom parameter groups
                foreach (var groupEntry in customGroups)
                {
                    string groupId = groupEntry.Key;
                    ParameterGraphGroup group = groupEntry.Value;
                    
                    // Handle foldout state for this group
                    if (!parameterGroupFoldouts.ContainsKey(groupId))
                    {
                        parameterGroupFoldouts[groupId] = true;
                    }
                    
                    parameterGroupFoldouts[groupId] = EditorGUILayout.Foldout(
                        parameterGroupFoldouts[groupId], 
                        group.name, 
                        true
                    );
                    
                    if (!parameterGroupFoldouts[groupId])
                        continue;
                    
                    // Draw titled box for this parameter group
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    // Draw group header with its color
                    GUI.color = group.color;
                    EditorGUILayout.LabelField(group.name, EditorStyles.boldLabel);
                    GUI.color = Color.white;
                    
                    // Calculate graph rect
                    float graphX = 90; // Left margin
                    Rect lastRect = EditorGUILayout.GetControlRect(GUILayout.Height(parameterGraphHeight));
                    Rect graphRect = new Rect(
                        graphX,
                        lastRect.y,
                        position.width - graphX - 20, // Right margin
                        parameterGraphHeight
                    );
                    
                    // Draw graph background
                    GUI.Box(graphRect, "");
                    
                    // Determine if we should use dual axis
                    bool useDualAxis = group.useDualAxis;
                    float primaryMaxValue = group.primaryMaxValue;
                    float secondaryMaxValue = group.secondaryMaxValue;
                    
                    // Draw appropriate axes based on group settings
                    if (useDualAxis)
                    {
                        graphHelper.DrawDualAxes(graphRect, "Time", 
                            group.primaryAxisLabel, 
                            group.secondaryAxisLabel, 
                            primaryMaxValue, 
                            secondaryMaxValue, 
                            pointCount);
                    }
                    else
                    {
                        graphHelper.DrawAxes(graphRect, "Time", group.primaryAxisLabel, primaryMaxValue, pointCount);
                    }
                    
                    // Prepare metrics for legend
                    Dictionary<string, GraphMetric> metrics = new Dictionary<string, GraphMetric>();
                    Dictionary<string, object> currentValues = new Dictionary<string, object>();
                    
                    // Draw each parameter in this group
                    for (int i = 0; i < group.parameters.Count; i++)
                    {
                        string paramName = group.parameters[i];
                        bool useSecondaryAxis = group.secondaryAxisParams.Contains(paramName);
                        List<float> history = dataHistory.GetParameterHistory(paramName);
                        
                        // Assign a color to this parameter
                        Color paramColor = group.parameterColors.ContainsKey(paramName) ? 
                            group.parameterColors[paramName] : 
                            DefaultParameterColor(i);
                        
                        if (history != null && history.Count > 1)
                        {
                            // Draw the graph line with appropriate axis
                            if (useDualAxis && useSecondaryAxis)
                            {
                                graphHelper.DrawLineGraphSecondaryAxis(history, secondaryMaxValue, paramColor, graphRect);
                            }
                            else
                            {
                                graphHelper.DrawLineGraph(history, primaryMaxValue, paramColor, graphRect);
                            }
                        }
                        
                        // Add to metrics for legend
                        metrics[paramName] = new GraphMetric { name = paramName, color = paramColor, isVisible = true };
                        
                        // Add current value for legend
                        if (currentParameterValues.TryGetValue(paramName, out float value))
                        {
                            currentValues[paramName] = value;
                        }
                    }
                    
                    // Draw legend for this group
                    graphHelper.DrawLegend(graphRect, currentValues, metrics);
                    
                    // End this parameter group
                    EditorGUILayout.EndVertical();
                    
                    // Add space between groups
                    EditorGUILayout.Space(10);
                }
                
                // Add some padding at the bottom
                EditorGUILayout.Space(20);
                
                // End scroll view and outer box
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                
                // Add helpful note about dual-axis
                EditorGUILayout.HelpBox("Parameters are grouped based on compatible scale ranges, with dual-axis graphs when necessary.", MessageType.Info);
            }
            
            EditorGUILayout.Space(10);
        }
        
        /// <summary>
        /// Create custom parameter groups with appropriate scaling for better visualization
        /// </summary>
        private Dictionary<string, ParameterGraphGroup> CreateCustomParameterGroups()
        {
            Dictionary<string, ParameterGraphGroup> groups = new Dictionary<string, ParameterGraphGroup>();
            
            // Growth & Productivity parameters (ranges differ significantly)
            var growthGroup = new ParameterGraphGroup
            {
                name = "Growth & Productivity Parameters",
                color = new Color(0.9f, 0.7f, 0.3f, 0.8f),
                primaryAxisLabel = "Productivity",
                primaryMaxValue = 5.0f,
                secondaryAxisLabel = "Rate",
                secondaryMaxValue = 20.0f,
                useDualAxis = true
            };
            
            // Add parameters to growth group
            growthGroup.parameters.Add("productivity factor");
            growthGroup.parameterColors["productivity factor"] = new Color(0.9f, 0.4f, 0.2f);
            
            growthGroup.parameters.Add("wealth growth rate");
            growthGroup.parameterColors["wealth growth rate"] = new Color(0.2f, 0.8f, 0.4f);
            growthGroup.secondaryAxisParams.Add("wealth growth rate"); // Different scale
            
            groups["growth"] = growthGroup;
            
            // Elasticity parameters (all in 0-1 range)
            var elasticityGroup = new ParameterGraphGroup
            {
                name = "Elasticity Parameters",
                color = new Color(0.3f, 0.7f, 0.9f, 0.8f),
                primaryAxisLabel = "Elasticity",
                primaryMaxValue = 1.0f,
                useDualAxis = false
            };
            
            // Add parameters to elasticity group
            elasticityGroup.parameters.Add("labor elasticity");
            elasticityGroup.parameterColors["labor elasticity"] = new Color(0.2f, 0.4f, 0.9f);
            
            elasticityGroup.parameters.Add("capital elasticity");
            elasticityGroup.parameterColors["capital elasticity"] = new Color(0.9f, 0.4f, 0.8f);
            
            groups["elasticity"] = elasticityGroup;
            
            // Cycle & Volatility parameters (mixed ranges)
            var cycleGroup = new ParameterGraphGroup
            {
                name = "Cycle & Volatility Parameters",
                color = new Color(0.4f, 0.6f, 0.4f, 0.8f),
                primaryAxisLabel = "Cycle",
                primaryMaxValue = 1.2f,
                secondaryAxisLabel = "Volatility",
                secondaryMaxValue = 0.5f,
                useDualAxis = true
            };
            
            // Add parameters to cycle group
            cycleGroup.parameters.Add("cycle multiplier");
            cycleGroup.parameterColors["cycle multiplier"] = new Color(0.3f, 0.7f, 0.3f);
            
            cycleGroup.parameters.Add("price volatility");
            cycleGroup.parameterColors["price volatility"] = new Color(0.9f, 0.6f, 0.2f);
            cycleGroup.secondaryAxisParams.Add("price volatility"); // Different scale
            
            groups["cycle"] = cycleGroup;
            
            // Infrastructure parameters (similar small rates)
            var infraGroup = new ParameterGraphGroup
            {
                name = "Infrastructure Parameters",
                color = new Color(0.7f, 0.3f, 0.9f, 0.8f),
                primaryAxisLabel = "Rate",
                primaryMaxValue = 2.0f,
                useDualAxis = false
            };
            
            // Add parameters to infrastructure group
            infraGroup.parameters.Add("decay rate");
            infraGroup.parameterColors["decay rate"] = new Color(0.8f, 0.3f, 0.3f);
            
            infraGroup.parameters.Add("maintenance cost multiplier");
            infraGroup.parameterColors["maintenance cost multiplier"] = new Color(0.6f, 0.3f, 0.8f);
            
            // Labor consumption in the infrastructure group since its scale is more compatible
            infraGroup.parameters.Add("labor consumption rate");
            infraGroup.parameterColors["labor consumption rate"] = new Color(0.3f, 0.8f, 0.8f);
            
            groups["infrastructure"] = infraGroup;
            
            return groups;
        }
        
        /// <summary>
        /// Generate a default color for a parameter based on index
        /// </summary>
        private Color DefaultParameterColor(int index)
        {
            switch (index % 5)
            {
                case 0: return new Color(0.9f, 0.4f, 0.4f); // Red
                case 1: return new Color(0.4f, 0.9f, 0.4f); // Green
                case 2: return new Color(0.4f, 0.4f, 0.9f); // Blue
                case 3: return new Color(0.9f, 0.9f, 0.4f); // Yellow
                case 4: return new Color(0.9f, 0.4f, 0.9f); // Purple
                default: return Color.white;
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
                
                // Force recalculation and update of production
                if (economicSystem.testRegion != null)
                {
                    RegionEntity region = economicSystem.testRegion;
                    
                    // Recalculate production using Cobb-Douglas function
                    float labor = region.Population.LaborAvailable;
                    float capital = region.Infrastructure.Level;
                    float productivityFactor = parameters.productivityFactor;
                    float laborElasticity = parameters.laborElasticity;
                    float capitalElasticity = parameters.capitalElasticity;
                    
                    // Use the same calculation as in ProductionCalculator
                    float calculatedProduction = productivityFactor * 
                        Mathf.Pow(Mathf.Max(1f, labor), laborElasticity) * 
                        Mathf.Pow(Mathf.Max(1f, capital), capitalElasticity);
                    int productionOutput = Mathf.RoundToInt(calculatedProduction);
                    
                    // Set the production values directly
                    region.Production.SetOutput(productionOutput);
                    region.Economy.Production = productionOutput;
                    
                    // Record history after the tick and production recalculation
                    dataHistory.RecordHistory(region);
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

        // Add helper method for recalculating production
        private void RecalculateProduction(RegionEntity region)
        {
            // Recalculate production using Cobb-Douglas function
            float labor = region.Population.LaborAvailable;
            float capital = region.Infrastructure.Level;
            float productivityFactor = parameters.productivityFactor;
            float laborElasticity = parameters.laborElasticity;
            float capitalElasticity = parameters.capitalElasticity;
            
            // Use the same calculation as in ProductionCalculator
            float calculatedProduction = productivityFactor * 
                Mathf.Pow(Mathf.Max(1f, labor), laborElasticity) * 
                Mathf.Pow(Mathf.Max(1f, capital), capitalElasticity);
            int productionOutput = Mathf.RoundToInt(calculatedProduction);
            
            // Set the production values directly
            region.Production.SetOutput(productionOutput);
            region.Economy.Production = productionOutput;
        }
    }

    /// <summary>
    /// Helper class for parameter graph groupings with appropriate scale settings
    /// </summary>
    public class ParameterGraphGroup
    {
        public string name;
        public Color color;
        public string primaryAxisLabel;
        public float primaryMaxValue;
        public string secondaryAxisLabel;
        public float secondaryMaxValue;
        public bool useDualAxis;
        public List<string> parameters = new List<string>();
        public List<string> secondaryAxisParams = new List<string>();
        public Dictionary<string, Color> parameterColors = new Dictionary<string, Color>();
    }
}