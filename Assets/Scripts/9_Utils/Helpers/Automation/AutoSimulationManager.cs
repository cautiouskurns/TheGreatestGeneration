/// CLASS PURPOSE:
/// AutoSimulationManager controls the automatic progression of game turns,
/// handling logic for running a simulation loop without user input.
/// 
/// CORE RESPONSIBILITIES:
/// - Start and stop the auto-simulation process
/// - Manage coroutine-based loop for automated turn advancement
/// - Enforce simulation constraints based on configuration settings
/// 
/// KEY COLLABORATORS:
/// - TurnManager: Executes the actual turn transitions
/// - TurnManagerConfigSO: Supplies parameters like max auto turns and delay between turns
/// - TurnState: Tracks current auto-simulation state and player turn status
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - Coroutine loop uses WaitForSeconds for timing between automated turns
/// - Directly tied to MonoBehaviour through TurnManager for coroutine lifecycle
/// 
/// REFACTORING SUGGESTIONS:
/// - Decouple from MonoBehaviour by extracting coroutine logic into a scheduler or runner
/// - Add events for simulation start, tick, and end to enable external reactions
/// 
/// EXTENSION OPPORTUNITIES:
/// - Add UI indicators or progress feedback during auto-simulation
/// - Allow pausing or step-by-step execution of auto-simulation
/// - Integrate with save/load to support resume of long-running simulations
using UnityEngine;
using System.Collections;

/// <summary>
/// Manages the auto-simulation process
/// </summary>
public class AutoSimulationManager
{
    private readonly TurnManager turnManager;
    private readonly TurnManagerConfigSO config;
    private readonly TurnState turnState;
    private Coroutine autoSimulationCoroutine;

    public bool IsAutoSimulating { get; private set; }

    public AutoSimulationManager(TurnManager manager, TurnManagerConfigSO config, TurnState turnState)
    {
        this.turnManager = manager;
        this.config = config;
        this.turnState = turnState;
    }

    public void StartAutoSimulation()
    {
        if (!IsAutoSimulating)
        {
            IsAutoSimulating = true;
            turnState.ResetAutoTurn();
            autoSimulationCoroutine = turnManager.StartCoroutine(AutoSimulationRoutine());
            
            Debug.Log("Auto-simulation started");
        }
    }

    public void StopAutoSimulation()
    {
        if (IsAutoSimulating)
        {
            IsAutoSimulating = false;
            
            if (autoSimulationCoroutine != null)
            {
                turnManager.StopCoroutine(autoSimulationCoroutine);
                autoSimulationCoroutine = null;
            }
            
            Debug.Log("Auto-simulation stopped");
        }
    }

    private IEnumerator AutoSimulationRoutine()
    {
        while (IsAutoSimulating && turnState.CurrentAutoTurn < config.maxAutoTurns)
        {
            // Process turn if it's the player's turn (wait for AI to complete otherwise)
            if (turnState.IsPlayerTurn)
            {
                turnManager.EndTurn();
                turnState.IncrementAutoTurn(); // Replace direct increment with method call
            }
            
            // Wait between turns
            yield return new WaitForSeconds(config.timeBetweenTurns);
        }
        
        // If we've reached max turns, stop auto-simulation
        if (turnState.CurrentAutoTurn >= config.maxAutoTurns)
        {
            turnState.SetAutoSimulation(false);
            IsAutoSimulating = false;
            Debug.Log($"Auto-simulation completed after {config.maxAutoTurns} turns");
        }
    }
}