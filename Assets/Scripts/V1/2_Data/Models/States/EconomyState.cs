using UnityEngine;
using System.Collections.Generic;

namespace V1.Data
{
    /// CLASS PURPOSE:
    /// EconomyState holds runtime data for the overall state of the economy,
    /// including the current cycle phase, how long it has lasted, and dynamic resource conditions.
    /// It is used by economic systems to inform decision-making, event triggers, and UI feedback.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Track the current economic cycle phase (e.g., Expansion, Recession)
    /// - Maintain turn count within the current phase
    /// - Identify which resources are currently in surplus or shortage
    ///
    /// KEY COLLABORATORS:
    /// - EconomicCycleSystem: Updates cycle phase and turn count
    /// - ResourceSystem or EventSystem: Reads shortage/surplus lists to trigger effects
    /// - UI Dashboard: Displays the current economic phase and imbalances
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Currently phase is a simple string; consider using an enum for safety
    /// - Resource lists are not yet scoped to region/nation, only global
    /// - No validation exists for conflicting or duplicate entries
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Convert phase tracking to enum (e.g., EconomicPhase enum)
    /// - Add utility methods for updating or querying shortages/surpluses
    /// - Expand scope to support per-nation economic state if needed
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Link resource trends to pricing or trade modifiers
    /// - Store historical economic performance for UI graphs or events
    /// - Add volatility tracking or predictive indicators for upcoming shifts

    public class EconomyState
    {
        // The current phase of the economic cycle
        public string CurrentEconomicCyclePhase { get; set; } = "Expansion";
        
        // How many turns we've been in the current phase
        public int TurnsInCurrentPhase { get; set; } = 0;
        
        // Track resources that are in shortage or surplus
        public List<string> ResourcesInShortage { get; set; } = new List<string>();
        public List<string> ResourcesInSurplus { get; set; } = new List<string>();
    }
}
