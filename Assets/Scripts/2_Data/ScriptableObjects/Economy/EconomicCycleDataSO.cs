using UnityEngine;
using System.Collections.Generic;

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
