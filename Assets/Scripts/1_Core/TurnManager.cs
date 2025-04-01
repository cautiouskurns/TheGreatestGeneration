using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

/// <summary>
/// Manages turn progression, including player and AI turns
/// Supports both manual and auto-simulation modes
/// </summary>
public class TurnManager : MonoBehaviour
{
    #region Configuration
    [Header("Configuration")]
    [SerializeField] private TurnManagerConfigSO turnConfig;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI simulationStatusText;
    [SerializeField] private Toggle autoSimulationToggle;
    #endregion

    #region State Management
    private TurnState turnState;
    private AutoSimulationManager autoSimManager;
    #endregion

    #region Unity Lifecycle Methods
    private void Awake()
    {
        // Initialize state and managers
        InitializeComponents();
    }

    private void Start()
    {
        // Setup UI interactions
        ConfigureAutoSimulationToggle();
        UpdateSimulationStatus();
    }

    private void OnEnable()
    {
        // Subscribe to game events
        EventBus.Subscribe("AITurnsCompleted", OnAITurnsCompleted);
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        EventBus.Unsubscribe("AITurnsCompleted", OnAITurnsCompleted);
        
        // Stop auto-simulation
        autoSimManager.StopAutoSimulation();
    }

    private void Update()
    {
        // Quick toggle for auto-simulation via spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleAutoSimulation();
        }
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        // Ensure configuration exists
        if (turnConfig == null)
        {
            turnConfig = ScriptableObject.CreateInstance<TurnManagerConfigSO>();
            Debug.LogWarning("Created default TurnManagerConfig");
        }

        // Initialize turn state
        turnState = new TurnState
        {
            DefaultAutoSimulation = turnConfig.defaultAutoSimulation
        };

        // Initialize auto-simulation manager
        autoSimManager = new AutoSimulationManager(this, turnConfig, turnState);
    }

    private void ConfigureAutoSimulationToggle()
    {
        if (autoSimulationToggle != null)
        {
            // Set initial state
            autoSimulationToggle.isOn = turnState.IsAutoSimulationEnabled;
            
            // Add listener for toggle changes
            autoSimulationToggle.onValueChanged.AddListener(OnAutoSimulationToggleChanged);
        }
    }
    #endregion

    #region Turn Management
    public void EndTurn()
    {
        // Only process if it's the player's turn
        if (turnState.IsPlayerTurn)
        {
            ProcessTurnEnd();
            
            // Ensure GameStateManager exists
            EnsureGameStateManager();
            
            // Trigger turn-related events
            EventBus.Trigger("TurnEnded");
            EventBus.Trigger("PlayerTurnEnded");
        }
    }

    private void ProcessTurnEnd()
    {
        // Log turn progression
        Debug.Log($"Ending Player Turn. Current Turn: {turnState.CurrentTurn}");
        
        // Trigger pre-turn and turn-end events
        EventBus.Trigger("TurnStarted");
        
        // Switch turn state
        turnState.AdvanceTurn();
    }

    private void OnAITurnsCompleted(object _)
    {
        // Log AI turn completion
        Debug.Log("AI Turns Completed. Returning to Player's turn.");
        
        // Trigger post-processing event
        EventBus.Trigger("TurnProcessed");
        
        // Switch back to player turn
        turnState.SetPlayerTurn(true);
    }
    #endregion

    #region Auto-Simulation Methods
    public void ToggleAutoSimulation()
    {
        // Toggle auto-simulation state
        turnState.ToggleAutoSimulation();
        
        // Update UI
        UpdateUIForAutoSimulation();
        
        // Start or stop simulation
        if (turnState.IsAutoSimulationEnabled)
        {
            autoSimManager.StartAutoSimulation();
        }
        else
        {
            autoSimManager.StopAutoSimulation();
        }
    }

    private void OnAutoSimulationToggleChanged(bool isOn)
    {
        // Sync toggle state with turn state
        turnState.SetAutoSimulation(isOn);
        UpdateUIForAutoSimulation();
        
        // Manage auto-simulation
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
    private void UpdateUIForAutoSimulation()
    {
        // Update toggle if exists
        if (autoSimulationToggle != null)
        {
            autoSimulationToggle.isOn = turnState.IsAutoSimulationEnabled;
        }
        
        // Update status text
        UpdateSimulationStatus();
    }

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
    private void EnsureGameStateManager()
    {
        if (GameStateManager.Instance == null)
        {
            GameObject gameStateObj = new GameObject("GameStateManager");
            gameStateObj.AddComponent<GameStateManager>();
            Debug.Log("Created GameStateManager");
        }
        
        // Increment turn and sync state
        GameStateManager.Instance.IncrementTurn();
        GameStateManager.Instance.SyncWithGameSystems();
    }
    #endregion
}