using UnityEngine;

/// CLASS PURPOSE:
/// TurnState manages the current turn progression and auto-simulation settings for the game.
/// It provides a simple interface for tracking the active turn, switching between player and AI turns,
/// and enabling or disabling automated turn simulation.
///
/// CORE RESPONSIBILITIES:
/// - Track current turn number and auto-simulation turn count
/// - Manage turn ownership (player or AI)
/// - Enable and toggle auto-simulation mode
///
/// KEY COLLABORATORS:
/// - TurnManager or GameController: Calls TurnState methods to progress turns
/// - UI Systems: Display current turn and simulation status
/// - Simulation Systems: Use auto-turn tracking to manage pacing or batch turns
///
/// CURRENT ARCHITECTURE NOTES:
/// - Auto-simulation is a global toggle, not scoped to nation or context
/// - All fields are accessed via public methods for encapsulation
/// - Uses simple boolean toggles and counters (no enums or timers yet)
///
/// REFACTORING SUGGESTIONS:
/// - Consider separating player vs. AI turn state into an enum for clarity
/// - Allow optional per-nation turn state if multiplayer or AI nations expand
/// - Track turn duration or timestamps for analytics
///
/// EXTENSION OPPORTUNITIES:
/// - Add turn-phase breakdown (e.g., Planning, Execution)
/// - Support simultaneous turns or advanced simulation modes
/// - Integrate with historical turn log for game summaries
/// 

namespace V1.Data
{

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
}