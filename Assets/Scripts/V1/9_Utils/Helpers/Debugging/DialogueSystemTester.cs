using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using V1.Core;
using V1.UI;
using V1.Data;

namespace V1.Utils
{  
    /// CLASS PURPOSE:
    /// DialogueSystemTester is a development/debugging utility for testing different types of dialogue interactions
    /// in the game. It provides test buttons for basic and state-based dialogue events, including conditional logic
    /// and game state integration.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Trigger basic dialogue formats: single-line, multi-line, and choice-based.
    /// - Trigger state-based events with embedded outcomes and game state effects.
    /// - Provide sample dialogue events and simulate dynamic scenarios via GameStateManager.
    /// - Refresh UI feedback via GameStateDisplay if available.
    /// 
    /// KEY COLLABORATORS:
    /// - EventDialogueManager: Handles rendering and processing of dialogue events.
    /// - GameStateManager: Supplies context and stores updated game state from outcomes.
    /// - GameStateDisplay: (Optional) Provides visual feedback of the current state for validation.
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Designed for editor testing and gameplay simulation, not included in final gameplay builds.
    /// - Contains hardcoded test data suitable for development validation.
    /// - Directly manipulates GameStateManager for testing conditional and outcome logic.
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Externalize sample event definitions to a ScriptableObject or JSON config for modularity.
    /// - Split button setup into a utility or base class for other testing scripts.
    /// - Consider runtime inspector integration to dynamically trigger and monitor test cases.
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Integrate more complex scenario templates (e.g., branching chains).
    /// - Enable in-editor simulation runner for multi-turn test scenarios.
    /// - Add support for live preview of outcomes and resulting game state changes.

    public class DialogueSystemTester : MonoBehaviour
    {
        [Header("Basic Dialogue Tests")]
        public Button testSimpleButton;
        public Button testMultiLineButton;
        public Button testChoicesButton;
        
        [Header("State-Based Dialogue Tests")]
        public Button testBasicEventButton;
        public Button testConditionalButton;
        public Button testWithOutcomesButton;
        
        [Header("Game State References")]
        public GameStateDisplay stateDisplay; // Optional reference to refresh display immediately
        
        // Sample dialogue events
        private List<SimpleDialogueEvent> sampleEvents = new List<SimpleDialogueEvent>();
        
        void Start()
        {
            // Set up basic test buttons
            if (testSimpleButton != null)
                testSimpleButton.onClick.AddListener(TestSimpleDialogue);
                
            if (testMultiLineButton != null)
                testMultiLineButton.onClick.AddListener(TestMultiLineDialogue);
                
            if (testChoicesButton != null)
                testChoicesButton.onClick.AddListener(TestChoicesDialogue);
                
            // Set up state-based test buttons
            if (testBasicEventButton != null)
                testBasicEventButton.onClick.AddListener(ShowBasicEvent);
                
            if (testConditionalButton != null)
                testConditionalButton.onClick.AddListener(ShowConditionalEvent);
                
            if (testWithOutcomesButton != null)
                testWithOutcomesButton.onClick.AddListener(ShowEventWithOutcomes);
                
            // Create sample events
            CreateSampleEvents();
        }
        
        // Basic dialogue tests
        void TestSimpleDialogue()
        {
            EventDialogueManager.ShowSimpleDialogue(
                "Economic Update", 
                "The economy is currently in the Expansion phase. Markets are optimistic."
            );
        }
        
        void TestMultiLineDialogue()
        {
            string[] lines = new string[]
            {
                "Greetings, Your Excellency. I am Lord Blackwood, your Economic Minister.",
                "Our nation's financial situation requires your immediate attention.",
                "The steel shortage is impacting our industrial capacity, and the merchants are demanding action.",
                "What would you like me to focus on in the next quarter?"
            };
            
            EventDialogueManager.ShowMultiLineDialogue("Cabinet Meeting", lines);
        }
        
        void TestChoicesDialogue()
        {
            string message = "A delegation from the neighboring country of Westoria has arrived. They propose a new trade agreement that would increase our iron imports but require us to export more grain.";
            
            string[] choices = new string[]
            {
                "Accept the trade agreement",
                "Decline politely and maintain current trade levels",
                "Counter-offer with a modified proposal",
                "Delay the decision and gather more information"
            };
            
            EventDialogueManager.ShowChoiceDialogue("Diplomatic Envoy", message, choices);
        }
        
        // State-based dialogue tests
        void CreateSampleEvents()
        {
            // Basic event
            var basicEvent = new SimpleDialogueEvent
            {
                id = "test_basic",
                title = "Economic Report",
                description = "Your economic advisor presents the latest report."
            };
            
            basicEvent.lines.Add(new DialogueLine
            {
                speakerName = "Economic Advisor",
                text = "Your Excellency, we are currently in the {EconomicPhase} phase of the economic cycle."
            });
            
            basicEvent.lines.Add(new DialogueLine
            {
                speakerName = "Economic Advisor",
                text = "We're now on turn {CurrentTurn}, and the economy is performing as expected for this phase."
            });
            
            basicEvent.choices.Add(new DialogueChoice
            {
                text = "Thank you for the report."
            });
            
            // Conditional event
            var conditionalEvent = new SimpleDialogueEvent
            {
                id = "test_conditional",
                title = "Resource Shortage",
                description = "An urgent matter requires your attention.",
                requiredResourceShortage = "Iron"
            };
            
            conditionalEvent.lines.Add(new DialogueLine
            {
                speakerName = "Resource Minister",
                text = "Your Excellency, we're facing a critical shortage of iron!"
            });
            
            conditionalEvent.choices.Add(new DialogueChoice
            {
                text = "Import more iron from neighboring nations",
                outcomes = new List<DialogueOutcome>
                {
                    new DialogueOutcome
                    {
                        type = DialogueOutcome.OutcomeType.AddResource,
                        targetId = "Iron",
                        value = 50,
                        description = "Purchase iron at market rates"
                    }
                }
            });
            
            conditionalEvent.choices.Add(new DialogueChoice
            {
                text = "Increase mining efforts in our territories",
                outcomes = new List<DialogueOutcome>
                {
                    new DialogueOutcome
                    {
                        type = DialogueOutcome.OutcomeType.AddResource,
                        targetId = "Iron",
                        value = 30,
                        description = "Boost domestic production"
                    },
                    new DialogueOutcome
                    {
                        type = DialogueOutcome.OutcomeType.ChangeSatisfaction,
                        targetId = "MiningRegion",
                        value = -0.1f,
                        description = "Workers are unhappy with increased quotas"
                    }
                }
            });
            
            // Event with impactful outcomes
            var outcomesEvent = new SimpleDialogueEvent
            {
                id = "test_outcomes",
                title = "Diplomatic Incident",
                description = "An incident with Westoria requires your attention."
            };
            
            outcomesEvent.lines.Add(new DialogueLine
            {
                speakerName = "Foreign Minister",
                text = "Your Excellency, Westoria has accused us of resource theft along the border."
            });
            
            outcomesEvent.lines.Add(new DialogueLine
            {
                speakerName = "Foreign Minister",
                text = "We should decide how to respond to maintain our diplomatic standing."
            });
            
            outcomesEvent.choices.Add(new DialogueChoice
            {
                text = "Deny the accusations and refuse to engage",
                outcomes = new List<DialogueOutcome>
                {
                    new DialogueOutcome
                    {
                        type = DialogueOutcome.OutcomeType.ChangeRelation,
                        targetId = "Westoria",
                        value = -20,
                        description = "Westoria is offended by our denial"
                    }
                }
            });
            
            outcomesEvent.choices.Add(new DialogueChoice
            {
                text = "Apologize and offer compensation",
                outcomes = new List<DialogueOutcome>
                {
                    new DialogueOutcome
                    {
                        type = DialogueOutcome.OutcomeType.ChangeRelation,
                        targetId = "Westoria",
                        value = 10,
                        description = "Westoria appreciates our honesty"
                    },
                    new DialogueOutcome
                    {
                        type = DialogueOutcome.OutcomeType.RemoveResource,
                        targetId = "Gold",
                        value = 100,
                        description = "Pay reparations"
                    }
                }
            });
            
            outcomesEvent.choices.Add(new DialogueChoice
            {
                text = "Launch a diplomatic investigation",
                hasCondition = true,
                requiredState = "EconomicPhase",
                requiredValue = 0, // This is a placeholder that would be "Expansion"
                outcomes = new List<DialogueOutcome>
                {
                    new DialogueOutcome
                    {
                        type = DialogueOutcome.OutcomeType.RecordDecision,
                        targetId = "StartedWestoriaInvestigation",
                        description = "Begin a formal investigation"
                    }
                }
            });
            
            // Add events to list
            sampleEvents.Add(basicEvent);
            sampleEvents.Add(conditionalEvent);
            sampleEvents.Add(outcomesEvent);
        }
        // In DialogueSystemTester.cs - Update ShowBasicEvent method

        void ShowBasicEvent()
        {
            // Make sure we have a GameStateManager to test with
            EnsureGameStateManager();
            
            // Set specific values for testing
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.Economy.CurrentEconomicCyclePhase = "Growth";
                // Add more test values here
                
                Debug.Log($"Set economic phase to 'Growth' and turn to {GameStateManager.Instance.GetCurrentTurn()}");
            }
            
            // Update the state display
            RefreshStateDisplayIfAvailable();
            
            // Show the event
            EventDialogueManager.ShowEvent(sampleEvents[0]);
        }
            
        void ShowConditionalEvent()
        {
            // Make sure we have a GameStateManager to test with
            EnsureGameStateManager();
            
            // Add Iron shortage to test condition
            if (GameStateManager.Instance != null)
            {
                if (!GameStateManager.Instance.Economy.ResourcesInShortage.Contains("Iron"))
                    GameStateManager.Instance.Economy.ResourcesInShortage.Add("Iron");
                    
                Debug.Log("Added Iron to resource shortages for testing");
            }
            
            // Update the state display
            RefreshStateDisplayIfAvailable();
            
            EventDialogueManager.ShowEvent(sampleEvents[1]);
        }
        
        void ShowEventWithOutcomes()
        {
            // Make sure we have a GameStateManager to test with
            EnsureGameStateManager();
            
            // Set economic phase to test conditional choice
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.Economy.CurrentEconomicCyclePhase = "Expansion";
                
                // Add Westoria relation if it doesn't exist
                if (!GameStateManager.Instance.Diplomacy.NationRelations.ContainsKey("Westoria"))
                    GameStateManager.Instance.Diplomacy.NationRelations["Westoria"] = 0;
                    
                // Add MiningRegion if it doesn't exist
                if (!GameStateManager.Instance.RegionStates.ContainsKey("MiningRegion"))
                    GameStateManager.Instance.RegionStates["MiningRegion"] = new RegionState
                    {
                        RegionName = "MiningRegion",
                        OwnerNation = "PlayerNation",
                        Satisfaction = 0.5f
                    };
                    
                Debug.Log("Set up GameState for testing outcomes (Expansion phase, added Westoria and MiningRegion)");
            }
            
            // Update the state display
            RefreshStateDisplayIfAvailable();
            
            EventDialogueManager.ShowEvent(sampleEvents[2]);
        }
        
        // Helper to ensure GameStateManager exists
        private void EnsureGameStateManager()
        {
            if (GameStateManager.Instance == null)
            {
                GameObject gameStateObj = new GameObject("GameStateManager");
                gameStateObj.AddComponent<GameStateManager>();
                Debug.Log("Created GameStateManager as it was missing");
                
                // Initialize the state with some values for testing
                GameStateManager.Instance.IncrementTurn(); // Set to turn 1
            }
        }
        
        // Helper to refresh state display
        private void RefreshStateDisplayIfAvailable()
        {
            if (stateDisplay != null)
            {
                stateDisplay.RefreshDisplay();
            }
        }
    }
}