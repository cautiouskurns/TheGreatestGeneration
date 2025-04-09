using UnityEngine;

namespace V1.Data
{

    /// CLASS PURPOSE:
    /// TurnManagerConfigSO defines configurable parameters for managing game turns,
    /// especially for auto-simulation behavior during gameplay or testing scenarios.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Store default values for auto-simulation toggle, turn interval timing, and turn limits
    /// - Provide a reusable configuration object to be referenced by TurnManager
    ///
    /// KEY COLLABORATORS:
    /// - TurnManager: Uses these parameters to control turn progression and auto-simulation
    /// - GameManager: May reference or override these values during dynamic gameplay states
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Exposes simple, designer-editable fields via Unity's inspector
    /// - Used as a ScriptableObject to allow for different presets per scenario
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Consider splitting time and simulation settings into separate ScriptableObjects if complexity grows
    /// - Add event hooks for runtime changes to enable dynamic configuration adjustments
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add fields for min/max thresholds or randomization options per turn
    /// - Enable context-sensitive turn pacing based on game phase or player actions

    /// <summary>
    /// Configuration for TurnManager
    /// </summary>
    [CreateAssetMenu(fileName = "TurnManagerConfig", menuName = "Game/Turn Manager Configuration")]
    public class TurnManagerConfigSO : ScriptableObject
    {
        [Header("Auto-Simulation Settings")]
        public bool defaultAutoSimulation = false;
        public float timeBetweenTurns = 3.0f;
        public int maxAutoTurns = 100;
    }
}
