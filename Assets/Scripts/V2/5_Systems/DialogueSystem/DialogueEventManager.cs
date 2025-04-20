using UnityEngine;
using System.Collections.Generic;
using V2.Managers;
using V2.Systems;
using V2.Entities;

namespace V2.Systems.DialogueSystem
{
    public class DialogueEventManager : MonoBehaviour
    {
        // Singleton pattern
        public static DialogueEventManager Instance { get; private set; }
        
        // Event registry - stores all available events
        [SerializeField] private List<DialogueEvent> availableEvents = new List<DialogueEvent>();
        
        // Currently active/triggered events queue
        private Queue<DialogueEvent> pendingEvents = new Queue<DialogueEvent>();
        
        // Currently displayed event
        private DialogueEvent currentEvent;
        public DialogueEvent CurrentEvent => currentEvent;
        
        // Economic system reference
        private EconomicSystem economicSystem;
        
        // Event refresh timer
        private float checkTimer = 0f;
        [SerializeField] private float checkInterval = 2.0f; // Check every 2 seconds
        
        // Flag to enable fallback event generation for event chains
        [SerializeField] private bool generateFallbackEvents = true;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            LoadEvents();
        }
        
        private void Update()
        {
            // Regular checking for new events
            checkTimer += Time.deltaTime;
            if (checkTimer >= checkInterval)
            {
                CheckForEvents();
                checkTimer = 0f;
            }
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
            EventBus.Subscribe("EventCompleted", OnEventCompleted);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
            EventBus.Unsubscribe("EventCompleted", OnEventCompleted);
        }
        
        // Check conditions on economic tick
        private void OnEconomicTick(object data)
        {
            CheckForEvents();
        }
        
        // Handle event completion
        private void OnEventCompleted(object data)
        {
            string eventId = data as string;
            if (currentEvent != null && currentEvent.id == eventId)
            {
                currentEvent = null;
                
                // Check if we have pending events to display next
                if (pendingEvents.Count > 0)
                {
                    DisplayNextEvent();
                }
            }
        }
        
        // Check all event conditions
        public void CheckForEvents()
        {
            if (economicSystem == null || economicSystem.testRegion == null)
                return;
                
            foreach (var evt in availableEvents)
            {
                if (!evt.hasTriggered && evt.CheckConditions(economicSystem, economicSystem.testRegion))
                {
                    QueueEvent(evt);
                    evt.hasTriggered = true;
                }
            }
            
            // If no current event is displayed but we have pending events, show the next one
            if (currentEvent == null && pendingEvents.Count > 0)
            {
                DisplayNextEvent();
            }
        }
        
        // Queue an event to be displayed
        public void QueueEvent(DialogueEvent evt)
        {
            pendingEvents.Enqueue(evt);
            Debug.Log($"Event queued: {evt.title}");
        }
        
        // Display the next event in the queue
    // Display the next event in the queue
    private void DisplayNextEvent()
    {
        if (pendingEvents.Count == 0)
        {
            Debug.Log("No pending events to display");
            return;
        }
            
        currentEvent = pendingEvents.Dequeue();
        
        // Notify UI to display this event
        EventBus.Trigger("DisplayEvent", currentEvent);
        Debug.Log($"Displaying event: {currentEvent.title}");
    }
        
        // Handle player choice for current event
    // Handle player choice for current event
    public void MakeChoice(int choiceIndex)
    {
        if (currentEvent == null)
        {
            Debug.LogError("Cannot make choice: No current event");
            return;
        }

        if (choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count)
        {
            Debug.LogError($"Invalid choice index {choiceIndex} for event {currentEvent.title}");
            return;
        }
            
        // Log what's happening for debugging
        Debug.Log($"Processing choice {choiceIndex} for event {currentEvent.title}");
            
        // Apply effects
        currentEvent.ApplyChoice(choiceIndex, economicSystem, economicSystem.testRegion);
        
        // Check for follow-up event BEFORE clearing current event
        string nextEventId = currentEvent.GetNextEventId(choiceIndex);
        Debug.Log($"Next event ID: {nextEventId}");
        
        // Clear current event first
        DialogueEvent processedEvent = currentEvent;
        currentEvent = null;
        
        // Trigger event completed event
        EventBus.Trigger("EventCompleted", processedEvent.id);
        
        // If we have a next event, immediately queue and display it
        if (!string.IsNullOrEmpty(nextEventId))
        {
            Debug.Log($"Looking for next event with ID: {nextEventId}");
            DialogueEvent nextEvent = availableEvents.Find(e => e.id == nextEventId);
            
            if (nextEvent != null)
            {
                Debug.Log($"Found next event: {nextEvent.title}");
                QueueEvent(nextEvent);
                
                // Force immediate display of the next event
                DisplayNextEvent();
            }
            else if (generateFallbackEvents)
            {
                Debug.Log($"Generating fallback event for: {nextEventId}");
                GenerateFallbackEvent(nextEventId);
                
                // Display next event immediately after generating fallback
                DisplayNextEvent();
            }
            else
            {
                Debug.LogWarning($"Next event with ID {nextEventId} not found and fallback generation is disabled");
            }
        }
        else
        {
            Debug.Log("No next event specified");
            
            // Check if we have any pending events anyway
            if (pendingEvents.Count > 0)
            {
                Debug.Log($"Found {pendingEvents.Count} pending events, displaying next");
                DisplayNextEvent();
            }
        }
    }
        
        // Generate a fallback event if the requested ID doesn't exist
        private void GenerateFallbackEvent(string eventId)
        {
            // Find a random event to use as a fallback
            if (availableEvents.Count > 0)
            {
                var randomEvent = availableEvents[Random.Range(0, availableEvents.Count)];
                // Clone it and rename
                var fallbackEvent = randomEvent.Clone();
                fallbackEvent.id = eventId;
                fallbackEvent.title = eventId;
                fallbackEvent.description = $"This is a fallback event for ID '{eventId}' which could not be found.";
                
                // Add to available events
                availableEvents.Add(fallbackEvent);
                
                // Queue it
                QueueEvent(fallbackEvent);
                
                Debug.Log($"Generated fallback event for ID: {eventId}");
            }
        }
        
        // Load events from configuration (could be ScriptableObjects in the future)
        private void LoadEvents()
        {
            // For now, just create sample events programmatically
            // This would be replaced with loading from ScriptableObjects or JSON
            CreateSampleEvents();
        }
        
        // Create sample events for testing
        private void CreateSampleEvents()
        {
            // Sample implementation - would be replaced with actual loading logic
            // This matches the content from your existing EventDisplayWindow
            
            // Event 1: Resource Shortage
            DialogueEvent resourceEvent = new DialogueEvent
            {
                id = "resource_shortage",
                title = "Resource Shortage",
                description = "Your economic advisor reports a serious shortage of essential resources. The manufacturing sector is at risk of a major slowdown if action is not taken.",
                category = EventCategory.Economic,
                conditions = new List<EventCondition>
                {
                    new EventCondition
                    {
                        parameter = EventCondition.ParameterType.Production,
                        comparison = EventCondition.ComparisonType.LessThan,
                        thresholdValue = 80f
                    }
                }
            };
            
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
            
            availableEvents.Add(resourceEvent);
            
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
            
            economicEvent.choices.Add(new EventChoice
            {
                text = "Implement worker protection and welfare programs",
                response = "You strengthen worker protections and expand social safety nets.",
                narrativeEffects = new List<string>
                {
                    "Money: -60 (Treasury)",
                    "Population Happiness: +25",
                    "Business Confidence: -15",
                    "Political Support: +20"
                },
                economicEffects = new List<ParameterEffect>
                {
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.Wealth,
                        effectType = ParameterEffect.EffectType.Add,
                        value = -60f,
                        description = "Wealth -60"
                    },
                    new ParameterEffect
                    {
                        target = ParameterEffect.EffectTarget.PopulationSatisfaction,
                        effectType = ParameterEffect.EffectType.Add,
                        value = 0.25f,
                        description = "Population Satisfaction +0.25"
                    }
                }
            });
            
            availableEvents.Add(economicEvent);
            
            // Add a few more sample events similar to the ones in your existing system
            // (Cultural events, military, diplomatic incidents, etc.)
            // For brevity, I'm including only the two main examples above
        }
        
        // Reset a specific event to allow it to trigger again
        public void ResetEvent(string eventId)
        {
            DialogueEvent evt = availableEvents.Find(e => e.id == eventId);
            if (evt != null)
            {
                evt.hasTriggered = false;
                Debug.Log($"Reset event: {eventId}");
            }
        }
        
        // Reset all events
        public void ResetAllEvents()
        {
            foreach (var evt in availableEvents)
            {
                evt.hasTriggered = false;
            }
            pendingEvents.Clear();
            currentEvent = null;
            Debug.Log("All events reset");
        }
        
        // Add an event programmatically (for testing or dynamic content)
        public void AddEvent(DialogueEvent evt)
        {
            if (!availableEvents.Exists(e => e.id == evt.id))
            {
                availableEvents.Add(evt);
                Debug.Log($"Added new event: {evt.title}");
            }
        }
        
        // Get all available events (for editor)
        public List<DialogueEvent> GetAllEvents()
        {
            return availableEvents;
        }
        
        // Manually trigger a specific event
        public void TriggerEvent(string eventId)
        {
            DialogueEvent evt = availableEvents.Find(e => e.id == eventId);
            if (evt != null)
            {
                QueueEvent(evt);
                Debug.Log($"Manually triggered event: {eventId}");
            }
        }
    }
}