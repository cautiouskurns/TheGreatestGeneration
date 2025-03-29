using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TurnManager : MonoBehaviour
{
    #region Inspector Fields
    [Header("Auto-Simulation Settings")]
    public bool enableAutoSimulation = false;
    public float timeBetweenTurns = 3.0f;
    public int maxAutoTurns = 100;
    public TextMeshProUGUI simulationStatusText;
    public Toggle autoSimulationToggle;
    #endregion

    #region Private Fields
    private bool isPlayerTurn = true;
    private bool isAutoSimulating = false;
    private int currentAutoTurn = 0;
    private Coroutine autoSimulationCoroutine;
    #endregion

    #region Unity Lifecycle Methods
    void Start()
    {
        // Start with player's turn
        isPlayerTurn = true;
        
        // Setup auto-simulation toggle if assigned
        if (autoSimulationToggle != null)
        {
            autoSimulationToggle.isOn = enableAutoSimulation;
            autoSimulationToggle.onValueChanged.AddListener(OnAutoSimulationToggleChanged);
        }
        
        // Initialize simulation status
        UpdateSimulationStatus();
    }
    
    void Update()
    {
        // Allow pressing spacebar to toggle auto-simulation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleAutoSimulation();
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe("AITurnsCompleted", OnAITurnsCompleted);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("AITurnsCompleted", OnAITurnsCompleted);
        
        // Make sure to stop auto-simulation when disabled
        StopAutoSimulation();
    }
    #endregion

    #region Turn Management
    public void EndTurn()
    {
        if (isPlayerTurn)
        {
            Debug.Log("Player Turn Ended. AI's turn now.");
            
            // Create GameStateManager if it doesn't exist
            if (GameStateManager.Instance == null)
            {
                GameObject gameStateObj = new GameObject("GameStateManager");
                gameStateObj.AddComponent<GameStateManager>();
                Debug.Log("Created GameStateManager as it was missing.");
            }
            
            // Now safely call the sync method
            GameStateManager.Instance.SyncWithGameSystems();
            
            EventBus.Trigger("TurnStarted");
            EventBus.Trigger("TurnEnded");
            EventBus.Trigger("PlayerTurnEnded");
            
            isPlayerTurn = false;
        }
    }
    
    private void ProcessTurnEnd()
    {
        Debug.Log("Player Turn Ended. AI's turn now.");
        EventBus.Trigger("TurnStarted"); // Optional: Add this if you want pre-turn processing
        EventBus.Trigger("TurnEnded");
        EventBus.Trigger("PlayerTurnEnded"); // New event for AI to listen to
        
        isPlayerTurn = false;
    }
    
    private void OnAITurnsCompleted(object _)
    {
        Debug.Log("AI Turns Completed. Player's turn now.");
        EventBus.Trigger("TurnProcessed"); // Update UI after all processing
        
        isPlayerTurn = true;
        
        // If auto-simulation is active, the coroutine will automatically
        // process the next turn after the delay
    }
    #endregion

    #region Auto-Simulation Management
    public void ToggleAutoSimulation()
    {
        enableAutoSimulation = !enableAutoSimulation;
        
        // Update UI toggle if it exists
        if (autoSimulationToggle != null)
        {
            autoSimulationToggle.isOn = enableAutoSimulation;
        }
        
        if (enableAutoSimulation)
        {
            StartAutoSimulation();
        }
        else
        {
            StopAutoSimulation();
        }
        
        UpdateSimulationStatus();
    }
    
    private void OnAutoSimulationToggleChanged(bool isOn)
    {
        enableAutoSimulation = isOn;
        
        if (enableAutoSimulation)
        {
            StartAutoSimulation();
        }
        else
        {
            StopAutoSimulation();
        }
        
        UpdateSimulationStatus();
    }
    
    private void StartAutoSimulation()
    {
        if (!isAutoSimulating)
        {
            isAutoSimulating = true;
            currentAutoTurn = 0;
            autoSimulationCoroutine = StartCoroutine(AutoSimulationRoutine());
            
            Debug.Log("Auto-simulation started");
        }
    }
    
    private void StopAutoSimulation()
    {
        if (isAutoSimulating)
        {
            isAutoSimulating = false;
            
            if (autoSimulationCoroutine != null)
            {
                StopCoroutine(autoSimulationCoroutine);
                autoSimulationCoroutine = null;
            }
            
            Debug.Log("Auto-simulation stopped");
        }
    }
    
    private IEnumerator AutoSimulationRoutine()
    {
        while (isAutoSimulating && currentAutoTurn < maxAutoTurns)
        {
            // Process turn if it's the player's turn (wait for AI to complete otherwise)
            if (isPlayerTurn)
            {
                ProcessTurnEnd();
                currentAutoTurn++;
                UpdateSimulationStatus();
            }
            
            // Wait between turns
            yield return new WaitForSeconds(timeBetweenTurns);
        }
        
        // If we've reached max turns, stop auto-simulation
        if (currentAutoTurn >= maxAutoTurns)
        {
            enableAutoSimulation = false;
            isAutoSimulating = false;
            Debug.Log($"Auto-simulation completed after {maxAutoTurns} turns");
            
            // Update UI toggle
            if (autoSimulationToggle != null)
            {
                autoSimulationToggle.isOn = false;
            }
            
            UpdateSimulationStatus();
        }
    }
    #endregion

    #region UI Management
    private void UpdateSimulationStatus()
    {
        if (simulationStatusText != null)
        {
            if (enableAutoSimulation)
            {
                simulationStatusText.text = $"Auto-Simulation: ON (Turn {currentAutoTurn}/{maxAutoTurns})";
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
}