using UnityEngine;

/// <summary>
/// Manages the state of turns and auto-simulation
/// </summary>
public class TurnState
{
    public bool IsPlayerTurn { get; private set; } = true;
    public bool IsAutoSimulationEnabled { get; private set; }
    public int CurrentTurn { get; private set; } = 0;
    public int CurrentAutoTurn { get; private set; } = 0;
    public bool DefaultAutoSimulation { get; set; }

    public void AdvanceTurn()
    {
        CurrentTurn++;
        IsPlayerTurn = !IsPlayerTurn;
    }

    public void SetPlayerTurn(bool isPlayerTurn)
    {
        IsPlayerTurn = isPlayerTurn;
    }

    public void ToggleAutoSimulation()
    {
        IsAutoSimulationEnabled = !IsAutoSimulationEnabled;
    }

    public void SetAutoSimulation(bool isEnabled)
    {
        IsAutoSimulationEnabled = isEnabled;
    }

    public void ResetAutoTurn() => CurrentAutoTurn = 0;

    public void IncrementAutoTurn()
    {
        CurrentAutoTurn++;
    }
}