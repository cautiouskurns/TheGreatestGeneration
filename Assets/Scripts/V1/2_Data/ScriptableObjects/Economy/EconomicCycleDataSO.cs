using UnityEngine;
using System.Collections.Generic;

namespace V1.Data
{
        
    /// CLASS PURPOSE:
    /// EconomicCycleDataSO defines the configuration for each phase of the economic cycle,
    /// including duration, color coding, descriptive text, and sector-specific modifiers.
    /// It allows designers to tweak how each economic phase affects production, consumption, and prices.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Store configuration data for Expansion, Peak, Contraction, and Recovery phases
    /// - Define sector-based modifiers for each phase (production, consumption, price)
    /// - Provide helper methods for accessing current phase data
    ///
    /// KEY COLLABORATORS:
    /// - EconomicCycleSystem: Uses this SO to determine effects of the current phase
    /// - Region or Nation systems: Apply modifiers from this data during economic calculations
    /// - UI Layer: Uses phase color and description for player feedback
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Sector modifiers are defined using nested serializable classes
    /// - Lookup by phase name relies on string matching; potential for mismatches
    /// - Color and description fields support rich UI integration
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace string-based phase lookup with enum-based access
    /// - Move modifier access into a dedicated helper/utility class if logic expands
    /// - Support dynamic or custom-defined cycle phases
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add support for event hooks or triggers per phase transition
    /// - Introduce phase volatility, randomness, or external influences
    /// - Include narrative tags or UI cues tied to phase transitions

    [CreateAssetMenu(fileName = "NewEconomicCycle", menuName = "Game/Economic Cycle")]
    public class EconomicCycleDataSO : ScriptableObject
    {
        [System.Serializable]
        public class SectorModifiers
        {
            public string sectorName;
            [Range(0.5f, 2.0f)] public float productionModifier = 1.0f;
            [Range(0.5f, 2.0f)] public float consumptionModifier = 1.0f;
            [Range(0.5f, 2.0f)] public float priceModifier = 1.0f;
        }
        
        [System.Serializable]
        public class CyclePhaseData
        {
            public string phaseName;
            public Color phaseColor;
            [Range(3, 10)] public int phaseDuration = 5;
            public List<SectorModifiers> sectorModifiers = new List<SectorModifiers>();
            [TextArea(2, 4)] public string phaseDescription;
        }
        
        public CyclePhaseData expansionPhase;
        public CyclePhaseData peakPhase;
        public CyclePhaseData contractionPhase;
        public CyclePhaseData recoveryPhase;
        
        // Helper method to get current phase modifiers
        public Dictionary<string, float> GetProductionModifiers(string phaseName)
        {
            CyclePhaseData phase = GetPhaseByName(phaseName);
            Dictionary<string, float> modifiers = new Dictionary<string, float>();
            
            if (phase != null)
            {
                foreach (var sectorMod in phase.sectorModifiers)
                {
                    modifiers[sectorMod.sectorName] = sectorMod.productionModifier;
                }
            }
            
            return modifiers;
        }
        
        private CyclePhaseData GetPhaseByName(string phaseName)
        {
            if (phaseName == expansionPhase.phaseName) return expansionPhase;
            if (phaseName == peakPhase.phaseName) return peakPhase;
            if (phaseName == contractionPhase.phaseName) return contractionPhase;
            if (phaseName == recoveryPhase.phaseName) return recoveryPhase;
            return null;
        }
    }
}
