using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using V1.Managers;
using V1.Utils;
using V1.Data;

namespace V1.Core
{

    /// <summary>
    /// CLASS PURPOSE:
    /// TurnManager is responsible for orchestrating the lifecycle of player and AI turns,
    /// managing the simulation flow, triggering turn-related events, and interfacing with the UI
    /// and GameStateManager to maintain a coherent turn-based experience.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Control turn transitions between player and AI
    /// - Manage auto-simulation and runtime toggling
    /// - Fire global events for TurnStarted, TurnEnded, TurnProcessed, etc.
    /// - Synchronize turn progression with GameStateManager
    /// - Handle related UI updates (toggle + status display)
    ///
    /// KEY COLLABORATORS:
    /// - GameStateManager: Tracks and synchronizes turn state globally
    /// - AutoSimulationManager: Encapsulates logic for running turns in simulation mode
    /// - EventBus: Handles all event broadcasting during turn transitions
    /// - UI Elements (Toggle, TextMeshPro): Visual state representation
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Class uses clear region-based segmentation
    /// - Composes with AutoSimulationManager for simulation control (good separation)
    /// - Mixes business logic with UI concerns (SRP violation potential)
    /// - Lacks abstraction or interfaces around UI and event names (tight coupling)
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Extract UI update logic (Toggle, StatusText) into TurnUIManager
    /// - Replace raw string event keys with enums or static constants for safety
    /// - Consider extracting rule logic into a TurnRuleEngine for testing/configurability
    /// - Enable DI or testing stubs for turn transitions and AI integration
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Could be linked with event-driven narrative system (e.g., on TurnEnd)
    /// - Potential logging hooks for telemetry or player feedback
    /// - Support for modded/custom turn configurations
    /// </summary>
    /// 

    public class TurnManager : MonoBehaviour
    {
        #region Configuration
        [Header("Configuration")]
        // Configuration asset controlling turn behavior
        [SerializeField] private TurnManagerConfigSO turnConfig;
        
        [Header("UI References")]
        // Displays the current status of the simulation
        [SerializeField] private TextMeshProUGUI simulationStatusText;
        // Toggle for enabling/disabling auto-simulation
        [SerializeField] private Toggle autoSimulationToggle;
        #endregion

        #region State Management
        // Manages the current state of the turn
        private TurnState turnState;
        // Handles automated turn progression
        private AutoSimulationManager autoSimManager;
        #endregion

        #region Unity Lifecycle Methods
        // Initializes internal components
        private void Awake()
        {
            InitializeComponents();
        }

        // Sets up UI interaction post-initialization
        private void Start()
        {
            ConfigureAutoSimulationToggle();
            UpdateSimulationStatus();
        }

        // Manages event subscription
        private void OnEnable()
        {
            EventBus.Subscribe("AITurnsCompleted", OnAITurnsCompleted);
        }

        // Manages event unsubscription
        private void OnDisable()
        {
            EventBus.Unsubscribe("AITurnsCompleted", OnAITurnsCompleted);
            
            // Stop auto-simulation
            autoSimManager.StopAutoSimulation();
        }

        // Handles debug/testing shortcuts (spacebar)
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ToggleAutoSimulation();
            }
        }
        #endregion

        #region Initialization
        // Ensures all core components are created
        private void InitializeComponents()
        {
            if (turnConfig == null)
            {
                turnConfig = ScriptableObject.CreateInstance<TurnManagerConfigSO>();
                Debug.LogWarning("Created default TurnManagerConfig");
            }

            turnState = new TurnState
            {
                DefaultAutoSimulation = turnConfig.defaultAutoSimulation
            };

            autoSimManager = new AutoSimulationManager(this, turnConfig, turnState);
        }

        // Binds the toggle UI to internal logic
        private void ConfigureAutoSimulationToggle()
        {
            if (autoSimulationToggle != null)
            {
                autoSimulationToggle.isOn = turnState.IsAutoSimulationEnabled;
                autoSimulationToggle.onValueChanged.AddListener(OnAutoSimulationToggleChanged);
            }
        }
        #endregion

        #region Turn Management
        // Ends the player's turn and triggers relevant events
        public void EndTurn()
        {
            if (turnState.IsPlayerTurn)
            {
                ProcessTurnEnd();
                EnsureGameStateManager();
                EventBus.Trigger("TurnEnded");
                EventBus.Trigger("PlayerTurnEnded");
            }
        }

        // Advances the turn state and triggers pre-turn logic
        private void ProcessTurnEnd()
        {
            Debug.Log($"Ending Player Turn. Current Turn: {turnState.CurrentTurn}");
            EventBus.Trigger("TurnStarted");
            turnState.AdvanceTurn();
        }

        // Returns control to the player after AI turn completion
        private void OnAITurnsCompleted(object _)
        {
            Debug.Log("AI Turns Completed. Returning to Player's turn.");
            EventBus.Trigger("TurnProcessed");
            turnState.SetPlayerTurn(true);
        }
        #endregion

        #region Auto-Simulation Methods
        // Toggles auto-simulation state
        public void ToggleAutoSimulation()
        {
            turnState.ToggleAutoSimulation();
            UpdateUIForAutoSimulation();
            if (turnState.IsAutoSimulationEnabled)
            {
                autoSimManager.StartAutoSimulation();
            }
            else
            {
                autoSimManager.StopAutoSimulation();
            }
        }

        // Syncs toggle state with turn state
        private void OnAutoSimulationToggleChanged(bool isOn)
        {
            turnState.SetAutoSimulation(isOn);
            UpdateUIForAutoSimulation();
            if (isOn)
            {
                autoSimManager.StartAutoSimulation();
            }
            else
            {
                autoSimManager.StopAutoSimulation();
            }
        }
        #endregion

        #region UI Management
        // Updates UI elements for auto-simulation
        private void UpdateUIForAutoSimulation()
        {
            if (autoSimulationToggle != null)
            {
                autoSimulationToggle.isOn = turnState.IsAutoSimulationEnabled;
            }
            UpdateSimulationStatus();
        }

        // Updates the simulation status text
        private void UpdateSimulationStatus()
        {
            if (simulationStatusText != null)
            {
                if (turnState.IsAutoSimulationEnabled)
                {
                    simulationStatusText.text = $"Auto-Simulation: ON (Turn {turnState.CurrentAutoTurn}/{turnConfig.maxAutoTurns})";
                    simulationStatusText.color = Color.green;
                }
                else
                {
                    simulationStatusText.text = "Auto-Simulation: OFF";
                    simulationStatusText.color = Color.white;
                }
            }
        }
        #endregion

        #region Utility Methods
        // Guarantees the GameStateManager is active and synced
        private void EnsureGameStateManager()
        {
            if (GameStateManager.Instance == null)
            {
                GameObject gameStateObj = new GameObject("GameStateManager");
                gameStateObj.AddComponent<GameStateManager>();
                Debug.Log("Created GameStateManager");
            }
            
            GameStateManager.Instance.IncrementTurn();
            GameStateManager.Instance.SyncWithGameSystems();
        }
        #endregion
    }
}