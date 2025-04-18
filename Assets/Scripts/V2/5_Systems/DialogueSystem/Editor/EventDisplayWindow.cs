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
        
        // Hardcoded sample events
        private List<SampleEvent> sampleEvents = new List<SampleEvent>();
        private int selectedEventIndex = 0;
        
        // Event categories
        private Dictionary<EventCategory, bool> categoryFilters = new Dictionary<EventCategory, bool>();
        private List<SampleEvent> filteredEvents = new List<SampleEvent>();
        private string searchText = "";
        
        // References to economic system components
        private EconomicSystem economicSystem;
        private EconomicEventTrigger eventTrigger;
        private EconomicEventEffect eventEffect;
        
        // Economic integration UI state
        private bool showEconomicIntegration = true;
        private bool showTriggeredEvents = false;
        private bool showTestEffects = false;
        
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
            
            // Get references to economic components
            RefreshEconomicComponents();
            
            CreateHardcodedEvents();
            FilterEvents();
            
            // Check if we need to create sample economic events
            CreateEconomicEvents();
            
            // Register for play mode state change events
            EditorApplication.playModeStateChanged += PlayModeStateChanged;
        }

        private void OnDisable()
        {
            // Unregister from play mode state change events
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
        }
        
        // Handle play mode state changes
        private void PlayModeStateChanged(PlayModeStateChange state)
        {
            // When entering play mode
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                // Wait a frame to make sure all components are initialized
                EditorApplication.delayCall += () =>
                {
                    RefreshEconomicComponents();
                    CreateEconomicEvents();
                    Repaint();
                    Debug.Log("EventDisplayWindow: Refreshed economic events for play mode");
                };
            }
        }
        
        // New method to refresh all economic components
        private void RefreshEconomicComponents()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            eventTrigger = FindFirstObjectByType<EconomicEventTrigger>();
            eventEffect = FindFirstObjectByType<EconomicEventEffect>();
            
            // Auto-create components if in play mode
            if (Application.isPlaying && (economicSystem == null || eventTrigger == null || eventEffect == null))
            {
                CreateMissingEconomicComponents();
            }
        }
        
        // Add a manual refresh button
        private void DrawRefreshButton()
        {
            if (GUILayout.Button("Refresh Economic Integration"))
            {
                RefreshEconomicComponents();
                CreateEconomicEvents();
                Repaint();
            }
        }
        
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
        
        private void InitializeCategories()
        {
            foreach (EventCategory category in Enum.GetValues(typeof(EventCategory)))
            {
                categoryFilters[category] = true;
            }
        }
        
        private void CreateHardcodedEvents()
        {
            // Create sample events
            sampleEvents.Clear();
            
            // Event 1: Resource Shortage
            SampleEvent resourceEvent = new SampleEvent
            {
                title = "Resource Shortage",
                description = "Your economic advisor reports a serious shortage of essential resources. The manufacturing sector is at risk of a major slowdown if action is not taken.",
                category = EventCategory.Economic,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Import resources from neighboring nations",
                        response = "You negotiate favorable import terms with neighboring nations.",
                        effects = new List<string>
                        {
                            "Money: -50 (Treasury)",
                            "Resources: +100 (Raw Materials)"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Divert labor to resource extraction",
                        response = "You order an emergency reallocation of labor to increase domestic resource production.",
                        effects = new List<string>
                        {
                            "Production: -20 (Manufacturing)",
                            "Resources: +60 (Raw Materials)",
                            "Happiness: -10 (Population)"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Do nothing and hope the market resolves the shortage",
                        response = "You decide to let market forces handle the shortage naturally.",
                        effects = new List<string>
                        {
                            "Economic Stability: -1"
                        }
                    }
                }
            };
            sampleEvents.Add(resourceEvent);
            
            // Event 2: Diplomatic Incident
            SampleEvent diplomaticEvent = new SampleEvent
            {
                title = "Diplomatic Incident",
                description = "An envoy from a neighboring nation has been found with confidential documents. They claim they were planted, but your security chief insists they were caught in the act of espionage.",
                category = EventCategory.Diplomatic,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Expel the envoy and issue a formal protest",
                        response = "You expel the envoy and issue a stern diplomatic protest. Relations with their nation cool significantly.",
                        effects = new List<string>
                        {
                            "Diplomatic Relations: -30 (Neighboring Nation)",
                            "National Security: +10"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Accept their explanation and increase security",
                        response = "You accept their explanation but quietly strengthen your security protocols. The neighboring nation appreciates your discretion.",
                        effects = new List<string>
                        {
                            "Diplomatic Relations: +15 (Neighboring Nation)",
                            "National Security: +5"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Detain the envoy for questioning",
                        response = "You detain the envoy for further questioning, causing an international incident.",
                        effects = new List<string>
                        {
                            "Diplomatic Relations: -50 (Neighboring Nation)",
                            "National Security: +15",
                            "Political Stability: -5"
                        }
                    }
                }
            };
            sampleEvents.Add(diplomaticEvent);
            
            // Event 3: Economic Reform
            SampleEvent economicEvent = new SampleEvent
            {
                title = "Economic Reform Proposal",
                description = "Your finance minister has presented a series of possible economic reforms aimed at increasing long-term growth. Each approach has different implications for various sectors of society.",
                category = EventCategory.Economic,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Implement market liberalization reforms",
                        response = "You begin a program of market liberalization, reducing regulations and trade barriers.",
                        effects = new List<string>
                        {
                            "Economic Growth: +15",
                            "Worker Satisfaction: -10",
                            "Business Confidence: +25"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Focus on industrial modernization",
                        response = "You invest heavily in modernizing industrial infrastructure and production methods.",
                        effects = new List<string>
                        {
                            "Money: -80 (Treasury)",
                            "Production: +30 (Long-term)",
                            "Technology: +15"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Implement worker protection and welfare programs",
                        response = "You strengthen worker protections and expand social safety nets.",
                        effects = new List<string>
                        {
                            "Money: -60 (Treasury)",
                            "Population Happiness: +25",
                            "Business Confidence: -15",
                            "Political Support: +20"
                        }
                    }
                }
            };
            sampleEvents.Add(economicEvent);
            
            // Event 4: Military Modernization
            SampleEvent militaryEvent = new SampleEvent
            {
                title = "Military Modernization",
                description = "Your generals have presented plans for military modernization. With tensions rising in the region, they argue that strengthening defenses is prudent, though the costs are significant.",
                category = EventCategory.Military,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Invest heavily in new military technology",
                        response = "You approve a major military modernization program focusing on advanced technology.",
                        effects = new List<string>
                        {
                            "Military Strength: +25",
                            "Money: -120 (Treasury)",
                            "Technology: +15",
                            "Diplomatic Relations: -10 (Neighboring Nations)"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Focus on expanding troop numbers",
                        response = "You authorize a recruitment drive to expand the size of your armed forces.",
                        effects = new List<string>
                        {
                            "Military Strength: +15",
                            "Money: -60 (Treasury)",
                            "Population Happiness: -5 (Conscription concerns)"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Pursue diplomatic solutions instead",
                        response = "You redirect funds toward diplomatic initiatives to reduce tensions in the region.",
                        effects = new List<string>
                        {
                            "Diplomatic Relations: +20 (All Nations)",
                            "Money: -30 (Treasury)",
                            "Military Approval: -15"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Maintain current military spending levels",
                        response = "You decide to maintain the status quo while monitoring the situation.",
                        effects = new List<string>
                        {
                            "No immediate effects"
                        }
                    }
                }
            };
            sampleEvents.Add(militaryEvent);
            
            // Event 5: Natural Disaster
            SampleEvent disasterEvent = new SampleEvent
            {
                title = "Devastating Floods",
                description = "Heavy rainfall has caused severe flooding in the eastern provinces. Infrastructure is damaged, crops are ruined, and many citizens are displaced. Your response will determine how quickly the region recovers.",
                category = EventCategory.Disaster,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Immediate large-scale emergency relief",
                        response = "You mobilize all available resources for an immediate and comprehensive relief effort.",
                        effects = new List<string>
                        {
                            "Money: -100 (Treasury)",
                            "Population Happiness: +20 (Eastern Provinces)",
                            "Infrastructure: +10 (Recovery)",
                            "Political Support: +15"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Limited government intervention with private sector support",
                        response = "You provide modest government aid and incentivize businesses to assist in recovery efforts.",
                        effects = new List<string>
                        {
                            "Money: -40 (Treasury)",
                            "Population Happiness: +5 (Eastern Provinces)",
                            "Business Relations: +15",
                            "Recovery Speed: Medium"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Minimal emergency response, focus on long-term prevention",
                        response = "You provide only essential emergency aid while directing resources toward future flood prevention.",
                        effects = new List<string>
                        {
                            "Money: -30 (Treasury) now, -60 later",
                            "Population Happiness: -15 (Eastern Provinces)",
                            "Future Disaster Risk: -30%",
                            "Political Support: -10"
                        }
                    }
                }
            };
            sampleEvents.Add(disasterEvent);
            
            // Event 6: Cultural Initiative
            SampleEvent culturalEvent = new SampleEvent
            {
                title = "National Arts Initiative",
                description = "The Minister of Culture proposes a comprehensive program to promote the arts and cultural heritage. It could enhance national identity and tourism, but requires significant funding.",
                category = EventCategory.Social,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Full funding for nationwide cultural programs",
                        response = "You authorize a comprehensive cultural initiative spanning museums, festivals, and artistic grants.",
                        effects = new List<string>
                        {
                            "Money: -70 (Treasury)",
                            "Cultural Development: +25",
                            "Tourism: +15 (Long-term)",
                            "Population Happiness: +10"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Focus on preserving heritage sites only",
                        response = "You direct funding primarily to preserving historical sites and national monuments.",
                        effects = new List<string>
                        {
                            "Money: -30 (Treasury)",
                            "Cultural Preservation: +20",
                            "Tourism: +10 (Long-term)",
                            "Artist Community Satisfaction: -5"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Reject the initiative as non-essential",
                        response = "You politely decline the proposal, citing more pressing economic priorities.",
                        effects = new List<string>
                        {
                            "Money: +0 (Treasury)",
                            "Cultural Community Relations: -15",
                            "Population Happiness: -5"
                        }
                    }
                }
            };
            sampleEvents.Add(culturalEvent);
            
            // Event 7: Scientific Breakthrough
            SampleEvent scientificEvent = new SampleEvent
            {
                title = "Research Breakthrough",
                description = "Scientists in your nation have made a significant breakthrough that could revolutionize several industries. However, further development requires substantial government support.",
                category = EventCategory.Technology,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Provide full government backing",
                        response = "You allocate substantial resources to develop the breakthrough into practical applications.",
                        effects = new List<string>
                        {
                            "Money: -90 (Treasury)",
                            "Technology: +30",
                            "Industrial Productivity: +20 (Long-term)",
                            "Scientific Community Relations: +25"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Partner with private industry",
                        response = "You facilitate partnerships between the researchers and major corporations to commercialize the discovery.",
                        effects = new List<string>
                        {
                            "Money: -30 (Treasury)",
                            "Technology: +15",
                            "Business Relations: +20",
                            "Scientific Community Relations: -5"
                        }
                    },
                    new SampleChoice
                    {
                        text = "Patent the technology internationally",
                        response = "You secure international patents and license the technology to generate revenue.",
                        effects = new List<string>
                        {
                            "Money: +50 (Treasury, long-term)",
                            "Technology Access: -10",
                            "International Relations: -5",
                            "Scientific Development: +5"
                        }
                    }
                }
            };
            sampleEvents.Add(scientificEvent);
            
            // Apply the initial filtering
            FilterEvents();
        }

        // Add economic versions of sample events
        private void CreateEconomicEvents()
        {
            // Only create economic events if we have the integration components
            if (eventEffect == null) return;
            
            // Find the "Economic Reform Proposal" event and add economic effects
            foreach (var evt in sampleEvents)
            {
                if (evt.title == "Economic Reform Proposal")
                {
                    // Add economic effects to the first choice: market liberalization
                    if (evt.choices.Count > 0)
                    {
                        evt.choices[0].economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.ProductivityFactor,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 0.3f,
                                description = "Productivity Factor +0.3"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.PopulationSatisfaction,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -0.1f,
                                description = "Population Satisfaction -0.1"
                            }
                        };
                    }
                    
                    // Add economic effects to the second choice: industrial modernization
                    if (evt.choices.Count > 1)
                    {
                        evt.choices[1].economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Wealth,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -80f,
                                description = "Wealth -80"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.InfrastructureLevel,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 2f,
                                description = "Infrastructure Level +2"
                            }
                        };
                    }
                    
                    // Add economic effects to the third choice: worker protection
                    if (evt.choices.Count > 2)
                    {
                        evt.choices[2].economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Wealth,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -60f,
                                description = "Wealth -60"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.PopulationSatisfaction,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 0.25f,
                                description = "Population Satisfaction +0.25"
                            }
                        };
                    }
                }
                
                // Add economic effects to Resource Shortage event
                if (evt.title == "Resource Shortage")
                {
                    // 1st choice: Import resources
                    if (evt.choices.Count > 0)
                    {
                        evt.choices[0].economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Wealth,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -50f,
                                description = "Wealth -50"
                            }
                        };
                    }
                    
                    // 2nd choice: Divert labor
                    if (evt.choices.Count > 1)
                    {
                        evt.choices[1].economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Production,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -20f,
                                description = "Production -20"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.PopulationSatisfaction,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -0.1f,
                                description = "Population Satisfaction -0.1"
                            }
                        };
                    }
                }
                
                // Add economic effects to Military Modernization event
                if (evt.title == "Military Modernization")
                {
                    // 1st choice: Invest in military tech
                    if (evt.choices.Count > 0)
                    {
                        evt.choices[0].economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Wealth,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -120f,
                                description = "Wealth -120"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.ProductivityFactor,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 0.1f,
                                description = "Productivity Factor +0.1 (from technology)"
                            }
                        };
                    }
                }
            }
            
            // Add new economic-focused events
            SampleEvent laborEvent = new SampleEvent
            {
                title = "Labor Shortage Crisis",
                description = "The economy is experiencing a severe labor shortage, affecting production across all sectors. Your advisors have prepared several options to address the crisis.",
                category = EventCategory.Economic,
                choices = new List<SampleChoice>
                {
                    new SampleChoice
                    {
                        text = "Increase automation and technological investment",
                        response = "You approve funding for automation and technological improvements to reduce labor dependence.",
                        effects = new List<string>
                        {
                            "Wealth: -100",
                            "Productivity: +15%",
                            "Labor Elasticity: -0.1",
                            "Capital Elasticity: +0.1"
                        },
                        economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Wealth,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -100f,
                                description = "Wealth -100"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.ProductivityFactor,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 0.15f,
                                description = "Productivity +15%"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.LaborElasticity,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -0.1f,
                                description = "Labor Elasticity -0.1"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.CapitalElasticity,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 0.1f,
                                description = "Capital Elasticity +0.1"
                            }
                        }
                    },
                    new SampleChoice
                    {
                        text = "Implement incentives for workforce expansion",
                        response = "You implement a program of incentives to expand the workforce through immigration and increased participation.",
                        effects = new List<string>
                        {
                            "Wealth: -50",
                            "Labor Available: +40",
                            "Population Satisfaction: +0.05"
                        },
                        economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Wealth,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -50f,
                                description = "Wealth -50"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.LaborAvailable,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 40f,
                                description = "Labor Available +40"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.PopulationSatisfaction,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 0.05f,
                                description = "Population Satisfaction +0.05"
                            }
                        }
                    },
                    new SampleChoice
                    {
                        text = "Restructure economy to be less labor-intensive",
                        response = "You initiate fundamental economic restructuring to prioritize sectors that require less labor.",
                        effects = new List<string>
                        {
                            "Labor Elasticity: -0.15",
                            "Capital Elasticity: +0.15",
                            "Production: -10% (short-term)"
                        },
                        economicEffects = new List<EconomicEventEffect.ParameterEffect>
                        {
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.LaborElasticity,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = -0.15f,
                                description = "Labor Elasticity -0.15"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.CapitalElasticity,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Add,
                                value = 0.15f,
                                description = "Capital Elasticity +0.15"
                            },
                            new EconomicEventEffect.ParameterEffect
                            {
                                target = EconomicEventEffect.ParameterEffect.EffectTarget.Production,
                                effectType = EconomicEventEffect.ParameterEffect.EffectType.Multiply,
                                value = 0.9f,
                                description = "Production -10%"
                            }
                        }
                    }
                }
            };
            sampleEvents.Add(laborEvent);
            
            FilterEvents(); // Refresh the filtered list
        }
        
        private void OnGUI()
        {
            // Check if play mode status changed
            if (wasInPlayMode != Application.isPlaying)
            {
                wasInPlayMode = Application.isPlaying;
                if (wasInPlayMode)
                {
                    // We just entered play mode, refresh economic components after a short delay
                    EditorApplication.delayCall += () =>
                    {
                        RefreshEconomicComponents();
                        CreateEconomicEvents();
                        Repaint();
                    };
                }
            }
            
            // Initialize styles here to ensure EditorStyles is ready
            InitializeStyles();
            
            // Add Economic Integration Section
            DrawEconomicIntegrationSection();
            
            if (sampleEvents.Count == 0)
            {
                EditorGUILayout.HelpBox("No events defined. Please create some events.", MessageType.Info);
                return;
            }
            
            GUILayout.Space(10);
            
            // Search and filter area
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
            
            // Category filters - FIX: Make a copy of the keys to avoid enumeration issues
            showCategories = EditorGUILayout.Foldout(showCategories, "Event Categories", true);
            if (showCategories)
            {
                EditorGUILayout.BeginHorizontal();
                
                if (GUILayout.Button("Select All", GUILayout.Width(100)))
                {
                    // Fix: Avoid "collection modified during enumeration" error
                    var keys = new List<EventCategory>(categoryFilters.Keys);
                    foreach (var key in keys)
                    {
                        categoryFilters[key] = true;
                    }
                    FilterEvents();
                }
                
                if (GUILayout.Button("Select None", GUILayout.Width(100)))
                {
                    // Fix: Avoid "collection modified during enumeration" error
                    var keys = new List<EventCategory>(categoryFilters.Keys);
                    foreach (var key in keys)
                    {
                        categoryFilters[key] = false;
                    }
                    FilterEvents();
                }
                
                EditorGUILayout.EndHorizontal();
                
                // Fix: Avoid "collection modified during enumeration" error
                var categories = new List<EventCategory>(categoryFilters.Keys);
                int count = 0;
                bool changed = false;
                
                EditorGUILayout.BeginHorizontal();
                
                foreach (var category in categories)
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
            
            EditorGUILayout.EndVertical();
            
            // Display no events message
            if (filteredEvents.Count == 0)
            {
                EditorGUILayout.HelpBox("No events match your current filters.", MessageType.Info);
                return;
            }
            
            GUILayout.Space(5);
            
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
            EditorGUILayout.EndHorizontal();
            
            // Show category tag
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Category:", GUILayout.Width(70));
            EditorGUILayout.LabelField(filteredEvents[selectedEventIndex].category.ToString(), categoryStyle);
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Space(5);
            
            // Show current event
            DisplayEvent(filteredEvents[selectedEventIndex]);
        }

        private void DisplayEvent(SampleEvent currentEvent)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Event title
            GUILayout.Label(currentEvent.title, titleStyle);
            GUILayout.Space(10);
            
            // Event description
            EditorGUILayout.LabelField("Event Description:", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(currentEvent.description, descriptionStyle, GUILayout.Height(80));
            
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
                SampleChoice selectedChoice = currentEvent.choices[selectedChoiceIndex];
                
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
                if (selectedChoice.effects != null && selectedChoice.effects.Count > 0)
                {
                    GUILayout.Space(10);
                    showEffects = EditorGUILayout.Foldout(showEffects, "Effects", true);
                    
                    if (showEffects)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var effect in selectedChoice.effects)
                        {
                            DisplayEffect(effect);
                        }
                        EditorGUI.indentLevel--;
                    }
                }
            }
            
            EditorGUILayout.EndScrollView();
        }

        // Display a choice button with proper styling and economic effect indicators
        private void DisplayChoice(SampleChoice choice, int index)
        {
            GUIStyle style = (index == selectedChoiceIndex) ? selectedChoiceStyle : choiceButtonStyle;
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            bool wasSelected = (index == selectedChoiceIndex);
            
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
                
                // Apply economic effects when a choice is selected
                if (!wasSelected && economicSystem != null && eventEffect != null)
                {
                    // Get the current event
                    SampleEvent currentEvent = filteredEvents[selectedEventIndex];
                    
                    // Apply economic effects if available
                    if (hasEconomicEffects)
                    {
                        eventEffect.ApplyEffects(currentEvent.title, index, choice.economicEffects);
                    }
                    else 
                    {
                        // Even without economic effects, we should still check for event chaining
                        string nextEventId = eventEffect.GetNextEvent(currentEvent.title, index);
                        if (!string.IsNullOrEmpty(nextEventId) && eventTrigger != null)
                        {
                            Debug.Log($"Progressing to next event: {nextEventId}");
                            eventTrigger.TriggerEvent(nextEventId, true);
                            
                            // Find and select the new event in the UI
                            ProgressToNextEvent(nextEventId);
                        }
                    }
                    
                    // Mark current event as completed
                    if (eventTrigger != null)
                    {
                        eventTrigger.CompleteEvent(currentEvent.title);
                    }
                }
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
        
        // Helper method to find and select the next event in the UI
        private void ProgressToNextEvent(string nextEventId)
        {
            // Find the event in our filtered list
            for (int i = 0; i < filteredEvents.Count; i++)
            {
                if (filteredEvents[i].title == nextEventId)
                {
                    // Select this event and reset choice selection
                    selectedEventIndex = i;
                    selectedChoiceIndex = -1;
                    break;
                }
            }
            
            // If we couldn't find it in filtered events, maybe it's filtered out
            // Let's temporarily disable filters to find it
            if (selectedEventIndex < 0 || filteredEvents[selectedEventIndex].title != nextEventId)
            {
                // Save current filter state
                Dictionary<EventCategory, bool> savedFilters = new Dictionary<EventCategory, bool>(categoryFilters);
                string savedSearch = searchText;
                
                // Enable all filters and clear search
                foreach (var category in categoryFilters.Keys.ToList())
                {
                    categoryFilters[category] = true;
                }
                searchText = "";
                FilterEvents();
                
                // Now try to find the event again
                for (int i = 0; i < filteredEvents.Count; i++)
                {
                    if (filteredEvents[i].title == nextEventId)
                    {
                        selectedEventIndex = i;
                        selectedChoiceIndex = -1;
                        break;
                    }
                }
                
                // Restore original filters but keep the new event visible
                // (no need to reset filters if we found the event)
            }
            
            Repaint(); // Refresh the UI
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
        
        private void FilterEvents()
        {
            filteredEvents.Clear();
            
            foreach (var evt in sampleEvents)
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
                            if (!matchesSearch && choice.effects != null)
                            {
                                foreach (var effect in choice.effects)
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
        
        // Draw the economic integration section
        private void DrawEconomicIntegrationSection()
        {
            showEconomicIntegration = EditorGUILayout.Foldout(showEconomicIntegration, "Economic Integration", true, EditorStyles.foldoutHeader);
            
            if (!showEconomicIntegration)
                return;
                
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Add refresh button at the top
            DrawRefreshButton();
            
            if (economicSystem == null || eventTrigger == null || eventEffect == null)
            {
                EditorGUILayout.HelpBox("Economic integration requires EconomicSystem, EconomicEventTrigger, and EconomicEventEffect components in the scene.", MessageType.Warning);
                
                if (GUILayout.Button("Create Missing Components"))
                {
                    CreateMissingEconomicComponents();
                }
            }
            else
            {
                // Display status
                EditorGUILayout.LabelField("Economic Integration Status: Active", EditorStyles.boldLabel);
                
                // Show triggered events
                showTriggeredEvents = EditorGUILayout.Foldout(showTriggeredEvents, "Triggered Events", true);
                if (showTriggeredEvents && eventTrigger != null) // Fixed: null check for eventTrigger
                {
                    EditorGUI.indentLevel++;
                    
                    if (eventTrigger.triggeredEvents.Count == 0)
                    {
                        EditorGUILayout.HelpBox("No events have been triggered yet.", MessageType.Info);
                    }
                    else
                    {
                        // Fix: Make a copy of the collection to avoid enumeration issues
                        var triggeredEventsCopy = new List<string>(eventTrigger.triggeredEvents);
                        
                        foreach (string eventId in triggeredEventsCopy)
                        {
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(eventId);
                            if (GUILayout.Button("Reset", GUILayout.Width(80)))
                            {
                                eventTrigger.ResetTriggerCondition(eventId);
                            }
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    
                    if (GUILayout.Button("Reset All Triggers"))
                    {
                        eventTrigger.ResetAllTriggerConditions();
                    }
                    
                    EditorGUI.indentLevel--;
                }
                
                // Test trigger conditions
                if (GUILayout.Button("Check Trigger Conditions"))
                {
                    eventTrigger.CheckTriggerConditions();
                    Repaint(); // Refresh the window
                }
                
                // Test economic effects
                showTestEffects = EditorGUILayout.Foldout(showTestEffects, "Test Economic Effects", true);
                if (showTestEffects && eventEffect != null) // Fixed: null check for eventEffect
                {
                    EditorGUI.indentLevel++;
                    
                    // Simple buttons to test economic effects
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Increase Wealth (+100)"))
                    {
                        eventEffect.ApplySingleEffect(EconomicEventEffect.ParameterEffect.EffectTarget.Wealth, 
                                                     EconomicEventEffect.ParameterEffect.EffectType.Add, 
                                                     100f);
                    }
                    
                    if (GUILayout.Button("Decrease Wealth (-50)"))
                    {
                        eventEffect.ApplySingleEffect(EconomicEventEffect.ParameterEffect.EffectTarget.Wealth, 
                                                     EconomicEventEffect.ParameterEffect.EffectType.Add, 
                                                     -50f);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Boost Productivity (+0.2)"))
                    {
                        eventEffect.ApplySingleEffect(EconomicEventEffect.ParameterEffect.EffectTarget.ProductivityFactor, 
                                                     EconomicEventEffect.ParameterEffect.EffectType.Add, 
                                                     0.2f);
                    }
                    
                    if (GUILayout.Button("Add Labor (+20)"))
                    {
                        eventEffect.ApplySingleEffect(EconomicEventEffect.ParameterEffect.EffectTarget.LaborAvailable, 
                                                     EconomicEventEffect.ParameterEffect.EffectType.Add, 
                                                     20f);
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    EditorGUI.indentLevel--;
                }
            }
            
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);
        }
        
        // Create any missing economic components - Fixed: Handling null references properly
        private void CreateMissingEconomicComponents()
        {
            GameObject economicObj = null;
            
            // Find or create the EconomicSystem
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            
            if (economicSystem == null)
            {
                // Check if we need to create a GameObject
                economicObj = GameObject.Find("EconomicSystem");
                if (economicObj == null)
                {
                    economicObj = new GameObject("EconomicSystem");
                }
                
                // Add the EconomicSystem component
                economicSystem = economicObj.AddComponent<EconomicSystem>();
                Debug.Log("Created EconomicSystem component");
            }
            else
            {
                economicObj = economicSystem.gameObject;
            }
            
            // Add event trigger if missing
            eventTrigger = FindFirstObjectByType<EconomicEventTrigger>();
            if (eventTrigger == null && economicObj != null)
            {
                eventTrigger = economicObj.AddComponent<EconomicEventTrigger>();
                Debug.Log("Added EconomicEventTrigger component");
            }
            
            // Add event effect if missing
            eventEffect = FindFirstObjectByType<EconomicEventEffect>();
            if (eventEffect == null && economicObj != null)
            {
                eventEffect = economicObj.AddComponent<EconomicEventEffect>();
                Debug.Log("Added EconomicEventEffect component");
            }
        }
        
        // Event categories enum
        private enum EventCategory
        {
            Economic,
            Diplomatic,
            Military,
            Social,
            Technology,
            Disaster
        }
        
        // Simple classes for hardcoded events
        [System.Serializable]
        private class SampleEvent
        {
            public string title;
            public string description;
            public EventCategory category;
            public List<SampleChoice> choices = new List<SampleChoice>();
        }
        
        // Make sure our SampleChoice class includes economic effects
        [System.Serializable]
        private class SampleChoice
        {
            public string text;
            public string response;
            public List<string> effects = new List<string>();
            public List<EconomicEventEffect.ParameterEffect> economicEffects;
        }
    }
}