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