using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;
using V2.Systems;
using V2.Entities;

namespace V2.Systems.DialogueSystem.Editor
{
    public class EventDisplayWindow : EditorWindow
    {
        // UI scroll position
        private Vector2 scrollPosition;
        
        // Styles
        private GUIStyle titleStyle;
        private GUIStyle descriptionStyle;
        private GUIStyle choiceButtonStyle;
        private GUIStyle selectedChoiceStyle;
        private GUIStyle categoryStyle;
        private GUIStyle effectPositiveStyle;
        private GUIStyle effectNegativeStyle;
        private GUIStyle effectNeutralStyle;
        private bool stylesInitialized = false;
        
        // State tracking
        private int selectedChoiceIndex = -1;
        private bool showEffects = true;
        private bool showCategories = true;
        
        // Event list
        private List<DialogueEvent> allEvents = new List<DialogueEvent>();
        private int selectedEventIndex = 0;
        
        // Event filters
        private Dictionary<EventCategory, bool> categoryFilters = new Dictionary<EventCategory, bool>();
        private List<DialogueEvent> filteredEvents = new List<DialogueEvent>();
        private string searchText = "";
        
        // System references
        private EconomicSystem economicSystem;
        private DialogueEventManager eventManager;
        
        // UI state
        private bool showEconomicIntegration = true;
        private bool showAvailableEvents = true;
        private bool showEventCreator = false;
        
        // Auto-refresh timer
        private double lastRefreshTime;
        private const double refreshInterval = 0.5; // 500ms
        
        // Track play mode state
        private bool wasInPlayMode = false;

        [MenuItem("Window/Narrative/Event Display")]
        public static void ShowWindow()
        {
            GetWindow<EventDisplayWindow>("Event Display");
        }

        private void OnEnable()
        {
            InitializeCategories();
            
            // Get system references
            RefreshSystemReferences();
            
            // Set up auto-refresh
            EditorApplication.update += OnEditorUpdate;
            
            // Refresh the event list
            RefreshEventList();
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
        
        // Auto-refresh logic
        private void OnEditorUpdate()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            
            // Check if we need to refresh based on time
            if (currentTime - lastRefreshTime >= refreshInterval)
            {
                if (Application.isPlaying && eventManager != null)
                {
                    // Check for changes in current event
                    DialogueEvent currentEvent = eventManager.CurrentEvent;
                    if (currentEvent != null)
                    {
                        // Find this event in our list
                        for (int i = 0; i < filteredEvents.Count; i++)
                        {
                            if (filteredEvents[i].id == currentEvent.id)
                            {
                                // Select this event if it's not already selected
                                if (selectedEventIndex != i)
                                {
                                    selectedEventIndex = i;
                                    selectedChoiceIndex = -1;
                                    Repaint();
                                }
                                break;
                            }
                        }
                    }
                }
                
                lastRefreshTime = currentTime;
            }
            
            // Check if play mode status changed
            if (wasInPlayMode != Application.isPlaying)
            {
                wasInPlayMode = Application.isPlaying;
                if (wasInPlayMode)
                {
                    // We just entered play mode, refresh after a short delay
                    EditorApplication.delayCall += () =>
                    {
                        RefreshSystemReferences();
                        RefreshEventList();
                        Repaint();
                    };
                }
            }
        }
        
        // Refresh system references
        private void RefreshSystemReferences()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            eventManager = FindFirstObjectByType<DialogueEventManager>();
            
            // Auto-create components if in play mode and missing
            if (Application.isPlaying && (economicSystem == null || eventManager == null))
            {
                CreateMissingComponents();
            }
        }
        
        // Create any missing components
        private void CreateMissingComponents()
        {
            GameObject economicObj = null;
            
            // Find or create the EconomicSystem
            if (economicSystem == null)
            {
                economicObj = GameObject.Find("EconomicSystem");
                if (economicObj == null)
                {
                    economicObj = new GameObject("EconomicSystem");
                }
                
                economicSystem = economicObj.AddComponent<EconomicSystem>();
                Debug.Log("Created EconomicSystem component");
            }
            else
            {
                economicObj = economicSystem.gameObject;
            }
            
            // Add event manager if missing
            if (eventManager == null && economicObj != null)
            {
                eventManager = economicObj.AddComponent<DialogueEventManager>();
                Debug.Log("Added DialogueEventManager component");
            }
        }
        
        // Initialize styles for the UI
        private void InitializeStyles()
        {
            if (stylesInitialized)
                return;
                
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter,
                margin = new RectOffset(10, 10, 10, 10)
            };
            
            descriptionStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                fontSize = 12,
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            choiceButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                fixedHeight = 0,
                stretchHeight = true,
                margin = new RectOffset(5, 5, 5, 5),
                padding = new RectOffset(10, 10, 10, 10)
            };
            
            selectedChoiceStyle = new GUIStyle(choiceButtonStyle)
            {
                normal = { background = MakeTex(2, 2, new Color(0.6f, 0.8f, 1f, 0.5f)) }
            };
            
            categoryStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 8, 8),
                margin = new RectOffset(0, 0, 5, 5)
            };
            
            effectPositiveStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.2f, 0.7f, 0.2f) }
            };
            
            effectNegativeStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.7f, 0.2f, 0.2f) }
            };
            
            effectNeutralStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = new Color(0.5f, 0.5f, 0.7f) }
            };
            
            stylesInitialized = true;
        }
        
        // Initialize category filters
        private void InitializeCategories()
        {
            foreach (EventCategory category in Enum.GetValues(typeof(EventCategory)))
            {
                categoryFilters[category] = true;
            }
        }
        
        // Get the current event list (from manager if available, otherwise from code)
        private void RefreshEventList()
        {
            allEvents.Clear();
            
            if (Application.isPlaying && eventManager != null)
            {
                // Get events from the manager
                allEvents.AddRange(eventManager.GetAllEvents());
            }
            else
            {
                // Create hardcoded test events
                CreateHardcodedEvents();
            }
            
            // Apply filters
            FilterEvents();
        }
        
        // Create test events (when not in play mode)
        private void CreateHardcodedEvents()
        {
            // Event 1: Resource Shortage
            DialogueEvent resourceEvent = new DialogueEvent
            {
                id = "resource_shortage",
                title = "Resource Shortage",
                description = "Your economic advisor reports a serious shortage of essential resources. The manufacturing sector is at risk of a major slowdown if action is not taken.",
                category = EventCategory.Economic,
            };
            
            // Add a basic condition
            resourceEvent.conditions.Add(new EventCondition
            {
                parameter = EventCondition.ParameterType.Production,
                comparison = EventCondition.ComparisonType.LessThan,
                thresholdValue = 80f
            });
            
            // Add choices
            resourceEvent.choices.Add(new EventChoice
            {
                text = "Import resources from neighboring nations",
                response = "You negotiate favorable import terms with neighboring nations.",
                narrativeEffects = new List<string>
                {
                    "Money: -50 (Treasury)",
                    "Resources: +100 (Raw Materials)"
                },
                economicEffects = new List<ParameterEffect>
                {
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.Wealth,
                        effectType = ParameterEffect.EffectType.Add,
                        value = -50f,
                        description = "Wealth -50"
                    }
                },
                nextEventId = "economic_reform_proposal"
            });
            
            resourceEvent.choices.Add(new EventChoice
            {
                text = "Divert labor to resource extraction",
                response = "You order an emergency reallocation of labor to increase domestic resource production.",
                narrativeEffects = new List<string>
                {
                    "Production: -20 (Manufacturing)",
                    "Resources: +60 (Raw Materials)",
                    "Happiness: -10 (Population)"
                },
                economicEffects = new List<ParameterEffect>
                {
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.Production,
                        effectType = ParameterEffect.EffectType.Add,
                        value = -20f,
                        description = "Production -20"
                    },
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.PopulationSatisfaction,
                        effectType = ParameterEffect.EffectType.Add,
                        value = -0.1f,
                        description = "Population Satisfaction -0.1"
                    }
                }
            });
            
            resourceEvent.choices.Add(new EventChoice
            {
                text = "Do nothing and hope the market resolves the shortage",
                response = "You decide to let market forces handle the shortage naturally.",
                narrativeEffects = new List<string>
                {
                    "Economic Stability: -1"
                }
            });
            
            allEvents.Add(resourceEvent);
            
            // Event 2: Economic Reform
            DialogueEvent economicEvent = new DialogueEvent
            {
                id = "economic_reform_proposal",
                title = "Economic Reform Proposal",
                description = "Your finance minister has presented a series of possible economic reforms aimed at increasing long-term growth. Each approach has different implications for various sectors of society.",
                category = EventCategory.Economic
            };
            
            // Add choices
            economicEvent.choices.Add(new EventChoice
            {
                text = "Implement market liberalization reforms",
                response = "You begin a program of market liberalization, reducing regulations and trade barriers.",
                narrativeEffects = new List<string>
                {
                    "Economic Growth: +15",
                    "Worker Satisfaction: -10",
                    "Business Confidence: +25"
                },
                economicEffects = new List<ParameterEffect>
                {
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.ProductivityFactor,
                        effectType = ParameterEffect.EffectType.Add,
                        value = 0.3f,
                        description = "Productivity Factor +0.3"
                    },
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.PopulationSatisfaction,
                        effectType = ParameterEffect.EffectType.Add,
                        value = -0.1f,
                        description = "Population Satisfaction -0.1"
                    }
                },
                nextEventId = "national_arts_initiative"
            });
            
            economicEvent.choices.Add(new EventChoice
            {
                text = "Focus on industrial modernization",
                response = "You invest heavily in modernizing industrial infrastructure and production methods.",
                narrativeEffects = new List<string>
                {
                    "Money: -80 (Treasury)",
                    "Production: +30 (Long-term)",
                    "Technology: +15"
                },
                economicEffects = new List<ParameterEffect>
                {
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.Wealth,
                        effectType = ParameterEffect.EffectType.Add,
                        value = -80f,
                        description = "Wealth -80"
                    },
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.InfrastructureLevel,
                        effectType = ParameterEffect.EffectType.Add,
                        value = 2f,
                        description = "Infrastructure Level +2"
                    }
                }
            });
            
            allEvents.Add(economicEvent);
            
            // Add more sample events based on your original EventDisplayWindow code
            // This is just a starter set
        }
        
        private void OnGUI()
        {
            // Initialize styles here to ensure EditorStyles is ready
            InitializeStyles();
            
            // Add Economic Integration Section
            DrawEconomicIntegrationSection();
            
            // Event list/filters section
            DrawEventListSection();
            
            // Display the selected event
            if (filteredEvents.Count > 0 && selectedEventIndex >= 0 && selectedEventIndex < filteredEvents.Count)
            {
                DrawEventDetails(filteredEvents[selectedEventIndex]);
            }
            else
            {
                EditorGUILayout.HelpBox("No events available or selected.", MessageType.Info);
            }
        }
        
        // Draw the economic integration section
        private void DrawEconomicIntegrationSection()
        {
            showEconomicIntegration = EditorGUILayout.Foldout(showEconomicIntegration, "Economic Integration", true, EditorStyles.foldoutHeader);
            
            if (!showEconomicIntegration)
                return;
                
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Add refresh button
            if (GUILayout.Button("Refresh System References"))
            {
                RefreshSystemReferences();
                RefreshEventList();
                Repaint();
            }
            
            // Status display
            bool hasAllComponents = economicSystem != null && eventManager != null;
            
            if (!hasAllComponents)
            {
                EditorGUILayout.HelpBox("Economic integration requires EconomicSystem and DialogueEventManager components in the scene.", MessageType.Warning);
                
                if (GUILayout.Button("Create Missing Components"))
                {
                    CreateMissingComponents();
                }
            }
            else
            {
                // Display status
                EditorGUILayout.LabelField("Economic Integration Status: Active", EditorStyles.boldLabel);
                
                // Currently active event
                if (Application.isPlaying && eventManager.CurrentEvent != null)
                {
                    EditorGUILayout.LabelField("Current Active Event:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(eventManager.CurrentEvent.title, EditorStyles.helpBox);
                }
                else
                {
                    EditorGUILayout.LabelField("No active event", EditorStyles.miniLabel);
                }
                
                // Control buttons
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Check Event Conditions"))
                {
                    if (Application.isPlaying)
                    {
                        eventManager.CheckForEvents();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Play Mode Required", 
                            "Please enter Play Mode to check event conditions.", "OK");
                    }
                }
                
                if (GUILayout.Button("Reset All Events"))
                {
                    if (Application.isPlaying)
                    {
                        eventManager.ResetAllEvents();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Play Mode Required", 
                            "Please enter Play Mode to reset events.", "OK");
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Test economic effects
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Boost Economy"))
                {
                    if (Application.isPlaying && economicSystem != null && economicSystem.testRegion != null)
                    {
                        economicSystem.testRegion.Economy.Wealth += 100;
                        economicSystem.productivityFactor += 0.1f;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Play Mode Required", 
                            "Please enter Play Mode to test economic effects.", "OK");
                    }
                }
                
                if (GUILayout.Button("Economic Crisis"))
                {
                    if (Application.isPlaying && economicSystem != null && economicSystem.testRegion != null)
                    {
                        economicSystem.testRegion.Economy.Wealth = Mathf.Max(0, economicSystem.testRegion.Economy.Wealth - 50);
                        economicSystem.testRegion.Population.UpdateSatisfaction(
                            Mathf.Max(0, economicSystem.testRegion.Population.Satisfaction - 0.2f));
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Play Mode Required", 
                            "Please enter Play Mode to test economic effects.", "OK");
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
        }
        
        // Draw the event list and filter controls
        private void DrawEventListSection()
        {
            showAvailableEvents = EditorGUILayout.Foldout(showAvailableEvents, "Available Events", true, EditorStyles.foldoutHeader);
            
            if (!showAvailableEvents)
                return;
                
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Search bar
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            string newSearchText = EditorGUILayout.TextField(searchText);
            if (newSearchText != searchText)
            {
                searchText = newSearchText;
                FilterEvents();
            }
            EditorGUILayout.EndHorizontal();
            
            // Category filters
            showCategories = EditorGUILayout.Foldout(showCategories, "Event Categories", true);
            if (showCategories)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Select All", GUILayout.Width(100)))
                {
                    var categories = new List<EventCategory>(categoryFilters.Keys);
                    foreach (var category in categories)
                    {
                        categoryFilters[category] = true;
                    }
                    FilterEvents();
                }
                
                if (GUILayout.Button("Select None", GUILayout.Width(100)))
                {
                    var categories = new List<EventCategory>(categoryFilters.Keys);
                    foreach (var category in categories)
                    {
                        categoryFilters[category] = false;
                    }
                    FilterEvents();
                }
                
                EditorGUILayout.EndHorizontal();
                
                var categoryKeys = new List<EventCategory>(categoryFilters.Keys);
                int count = 0;
                bool changed = false;
                
                EditorGUILayout.BeginHorizontal();
                
                foreach (var category in categoryKeys)
                {
                    if (count > 0 && count % 3 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }
                    
                    bool newValue = EditorGUILayout.ToggleLeft(category.ToString(), categoryFilters[category], GUILayout.Width(120));
                    if (newValue != categoryFilters[category])
                    {
                        categoryFilters[category] = newValue;
                        changed = true;
                    }
                    
                    count++;
                }
                
                EditorGUILayout.EndHorizontal();
                
                if (changed)
                {
                    FilterEvents();
                }
            }
            
            // Event list
            if (filteredEvents.Count == 0)
            {
                EditorGUILayout.HelpBox("No events match your current filters.", MessageType.Info);
            }
            else
            {
                // Event selection dropdown
                string[] eventTitles = new string[filteredEvents.Count];
                for (int i = 0; i < filteredEvents.Count; i++)
                {
                    eventTitles[i] = filteredEvents[i].title;
                }
                
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Event:", GUILayout.Width(50));
                int newSelectedIndex = EditorGUILayout.Popup(selectedEventIndex, eventTitles);
                
                if (newSelectedIndex != selectedEventIndex)
                {
                    selectedEventIndex = newSelectedIndex;
                    selectedChoiceIndex = -1; // Reset selected choice
                }
                
                // Add buttons to trigger/reset the selected event
                if (Application.isPlaying && eventManager != null)
                {
                    if (GUILayout.Button("Trigger", GUILayout.Width(80)))
                    {
                        var selectedEvent = filteredEvents[selectedEventIndex];
                        eventManager.TriggerEvent(selectedEvent.id);
                    }
                    
                    if (GUILayout.Button("Reset", GUILayout.Width(80)))
                    {
                        var selectedEvent = filteredEvents[selectedEventIndex];
                        eventManager.ResetEvent(selectedEvent.id);
                    }
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Show category tag for the selected event
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Category:", GUILayout.Width(70));
                EditorGUILayout.LabelField(filteredEvents[selectedEventIndex].category.ToString(), categoryStyle);
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
        }
        
        // Draw details for the selected event
        private void DrawEventDetails(DialogueEvent currentEvent)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Event title
            GUILayout.Label(currentEvent.title, titleStyle);
            GUILayout.Space(10);
            
            // Event description
            EditorGUILayout.LabelField("Event Description:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(currentEvent.description, descriptionStyle, GUILayout.Height(80));
            
            // Event conditions
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Trigger Conditions:", EditorStyles.boldLabel);
            
            if (currentEvent.conditions != null && currentEvent.conditions.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var condition in currentEvent.conditions)
                {
                    EditorGUILayout.LabelField($"• {condition.ToString()}", EditorStyles.miniLabel);
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("No trigger conditions defined. Event must be triggered manually.", MessageType.Info);
            }
            
            GUILayout.Space(20);
            
            // Display choices
            EditorGUILayout.LabelField("Available Response Options:", EditorStyles.boldLabel);
            
            if (currentEvent.choices != null && currentEvent.choices.Count > 0)
            {
                for (int i = 0; i < currentEvent.choices.Count; i++)
                {
                    DisplayChoice(currentEvent.choices[i], i);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No choices available for this event.", MessageType.Warning);
            }
            
            // Show selected choice response
            if (selectedChoiceIndex >= 0 && selectedChoiceIndex < currentEvent.choices.Count)
            {
                EventChoice selectedChoice = currentEvent.choices[selectedChoiceIndex];
                
                GUILayout.Space(20);
                EditorGUILayout.LabelField("Response:", EditorStyles.boldLabel);
                
                if (!string.IsNullOrEmpty(selectedChoice.response))
                {
                    EditorGUILayout.TextArea(selectedChoice.response, descriptionStyle, GUILayout.Height(60));
                }
                else
                {
                    EditorGUILayout.HelpBox("No response text for this choice.", MessageType.Info);
                }
                
                // Show effects
                if ((selectedChoice.narrativeEffects != null && selectedChoice.narrativeEffects.Count > 0) ||
                    (selectedChoice.economicEffects != null && selectedChoice.economicEffects.Count > 0))
                {
                    GUILayout.Space(10);
                    showEffects = EditorGUILayout.Foldout(showEffects, "Effects", true);
                    
                    if (showEffects)
                    {
                        EditorGUI.indentLevel++;
                        
                        // Display narrative effects
                        foreach (var effect in selectedChoice.narrativeEffects)
                        {
                            DisplayEffect(effect);
                        }
                        
                        // Display economic effects
                        foreach (var effect in selectedChoice.economicEffects)
                        {
                            DisplayEffect(effect.description);
                        }
                        
                        EditorGUI.indentLevel--;
                    }
                }
                // Show next event if exists
                if (!string.IsNullOrEmpty(selectedChoice.nextEventId))
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("Next Event:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(selectedChoice.nextEventId, EditorStyles.helpBox);
                }
                
                // Add simulate choice button
                if (Application.isPlaying && eventManager != null && 
                    eventManager.CurrentEvent != null && 
                    eventManager.CurrentEvent.id == currentEvent.id)
                {
                    GUILayout.Space(15);
                    if (GUILayout.Button("Simulate This Choice", GUILayout.Height(30)))
                    {
                        eventManager.MakeChoice(selectedChoiceIndex);
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        // Display a choice button with proper styling and economic effect indicators
        private void DisplayChoice(EventChoice choice, int index)
        {
            GUIStyle style = (index == selectedChoiceIndex) ? selectedChoiceStyle : choiceButtonStyle;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Check if this choice has economic effects
            bool hasEconomicEffects = (choice.economicEffects != null && choice.economicEffects.Count > 0);
            
            // Create a label that includes an indicator for economic effects
            string choiceText = choice.text;
            if (hasEconomicEffects)
            {
                choiceText = "🔄 " + choiceText; // Add an icon to indicate economic effect
            }
            
            if (GUILayout.Button(choiceText, style, GUILayout.MinHeight(40)))
            {
                selectedChoiceIndex = index;
            }
            
            // Add a colored indicator if this choice has economic effects
            if (hasEconomicEffects)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                
                // Draw a more visible indicator
                Rect indicatorRect = new Rect(lastRect.x + 5, lastRect.y + 5, 16, 16);
                EditorGUI.DrawRect(indicatorRect, new Color(0.2f, 0.8f, 0.2f, 0.8f)); // Bright green indicator
                
                // Add a small label inside the indicator
                GUI.Label(indicatorRect, "€", new GUIStyle(EditorStyles.boldLabel) { 
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                });
                
                // Add a tooltip area
                EditorGUIUtility.AddCursorRect(indicatorRect, MouseCursor.Link);
                if (indicatorRect.Contains(Event.current.mousePosition))
                {
                    // Show a tooltip on hover
                    string tooltip = "This choice has economic effects";
                    if (choice.economicEffects.Count > 0)
                    {
                        tooltip += ":\n";
                        foreach (var effect in choice.economicEffects)
                        {
                            tooltip += $"• {effect.description}\n";
                        }
                    }
                    
                    GUI.Label(new Rect(Event.current.mousePosition.x + 15, 
                                      Event.current.mousePosition.y, 200, 50), 
                              tooltip, 
                              new GUIStyle(EditorStyles.helpBox));
                }
            }
            
            EditorGUILayout.EndVertical();
            GUILayout.Space(5);
        }

        private void DisplayEffect(string effect)
        {
            // Determine if this is a positive, negative, or neutral effect
            GUIStyle style = effectNeutralStyle;
            
            if (effect.Contains("+"))
            {
                style = effectPositiveStyle;
            }
            else if (effect.Contains("-"))
            {
                style = effectNegativeStyle;
            }
            
            EditorGUILayout.LabelField("• " + effect, style);
        }
        
        // Helper method to create a texture
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        
        // Apply event filters
        private void FilterEvents()
        {
            filteredEvents.Clear();
            
            foreach (var evt in allEvents)
            {
                // Apply category filter
                if (!categoryFilters[evt.category])
                    continue;
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchText))
                {
                    string searchLower = searchText.ToLower();
                    bool matchesSearch = evt.title.ToLower().Contains(searchLower) ||
                                        evt.description.ToLower().Contains(searchLower) ||
                                        evt.category.ToString().ToLower().Contains(searchLower);
                    
                    // Also search in choices
                    if (!matchesSearch && evt.choices != null)
                    {
                        foreach (var choice in evt.choices)
                        {
                            if (choice.text.ToLower().Contains(searchLower) ||
                                choice.response.ToLower().Contains(searchLower))
                            {
                                matchesSearch = true;
                                break;
                            }
                            
                            // Search in effects
                            if (!matchesSearch && choice.narrativeEffects != null)
                            {
                                foreach (var effect in choice.narrativeEffects)
                                {
                                    if (effect.ToLower().Contains(searchLower))
                                    {
                                        matchesSearch = true;
                                        break;
                                    }
                                }
                            }
                            
                            if (matchesSearch)
                                break;
                        }
                    }
                    
                    if (!matchesSearch)
                        continue;
                }
                
                filteredEvents.Add(evt);
            }
            
            // Reset selected index if it's out of bounds
            if (selectedEventIndex >= filteredEvents.Count)
            {
                selectedEventIndex = filteredEvents.Count > 0 ? 0 : -1;
            }
        }
    }
}