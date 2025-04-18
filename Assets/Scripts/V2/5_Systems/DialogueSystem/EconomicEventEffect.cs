using UnityEngine;
using System.Collections.Generic;
using V2.Systems;
using V2.Entities;
using V2.Components;
using System.Linq;

namespace V2.Systems.DialogueSystem
{
    /// <summary>
    /// Applies effects from event choices to the economic system
    /// </summary>
    public class EconomicEventEffect : MonoBehaviour
    {
        // Reference to the economic system
        private EconomicSystem economicSystem;
        
        // Dictionary to store follow-up events after choices
        private Dictionary<string, Dictionary<int, string>> nextEventChain = new Dictionary<string, Dictionary<int, string>>();
        
        // Reference to trigger component
        private EconomicEventTrigger eventTrigger;
        
        // Flag to enable fallback event generation
        [SerializeField]
        private bool generateFallbackEvents = true;
        
        // List of event titles we can use for fallback generation
        private List<string> availableEventTitles = new List<string>();
        
        // Defines a change to apply to an economic parameter
        [System.Serializable]
        public class ParameterEffect
        {
            public enum EffectTarget
            {
                Wealth,
                Production,
                LaborAvailable,
                InfrastructureLevel,
                PopulationSatisfaction,
                ProductivityFactor,
                LaborElasticity,
                CapitalElasticity,
                CycleMultiplier,
                WealthGrowthRate,
                PriceVolatility,
                DecayRate,
                MaintenanceCostMultiplier,
                LaborConsumptionRate
            }
            
            public enum EffectType
            {
                Add,           // Add a fixed value
                Multiply,      // Multiply by a factor
                Set            // Set to a specific value
            }
            
            public EffectTarget target;
            public EffectType effectType;
            public float value;
            public string description;
            
            // Apply the effect to the economic system
            public void Apply(EconomicSystem system, RegionEntity region)
            {
                if (system == null || region == null)
                    return;
                
                // Get the current value
                float currentValue = GetCurrentValue(target, system, region);
                float newValue = currentValue;
                
                // Apply the modification
                switch (effectType)
                {
                    case EffectType.Add:
                        newValue = currentValue + value;
                        break;
                    case EffectType.Multiply:
                        newValue = currentValue * value;
                        break;
                    case EffectType.Set:
                        newValue = value;
                        break;
                }
                
                // Apply the new value
                SetParameterValue(target, newValue, system, region);
                
                Debug.Log($"Applied economic effect: {target} {effectType} {value} => {newValue}");
            }
            
            // Get the current value of the target parameter
            private float GetCurrentValue(EffectTarget target, EconomicSystem system, RegionEntity region)
            {
                switch (target)
                {
                    // Region entity parameters
                    case EffectTarget.Wealth:
                        return region.Economy.Wealth;
                    case EffectTarget.Production:
                        return region.Economy.Production;
                    case EffectTarget.LaborAvailable:
                        return region.Population.LaborAvailable;
                    case EffectTarget.InfrastructureLevel:
                        return region.Infrastructure.Level;
                    case EffectTarget.PopulationSatisfaction:
                        return region.Population.Satisfaction;
                    
                    // Economic system parameters
                    case EffectTarget.ProductivityFactor:
                        return system.productivityFactor;
                    case EffectTarget.LaborElasticity:
                        return system.laborElasticity;
                    case EffectTarget.CapitalElasticity:
                        return system.capitalElasticity;
                    case EffectTarget.CycleMultiplier:
                        return system.cycleMultiplier;
                    case EffectTarget.WealthGrowthRate:
                        return system.wealthGrowthRate;
                    case EffectTarget.PriceVolatility:
                        return system.priceVolatility;
                    case EffectTarget.DecayRate:
                        return system.decayRate;
                    case EffectTarget.MaintenanceCostMultiplier:
                        return system.maintenanceCostMultiplier;
                    case EffectTarget.LaborConsumptionRate:
                        return system.laborConsumptionRate;
                    default:
                        return 0f;
                }
            }
            
            // Set the new value for the parameter
            private void SetParameterValue(EffectTarget target, float newValue, EconomicSystem system, RegionEntity region)
            {
                switch (target)
                {
                    // Region entity parameters
                    case EffectTarget.Wealth:
                        region.Economy.Wealth = Mathf.RoundToInt(newValue);
                        break;
                    case EffectTarget.Production:
                        region.Economy.Production = Mathf.RoundToInt(newValue);
                        region.Production.SetOutput(Mathf.RoundToInt(newValue));
                        break;
                    case EffectTarget.LaborAvailable:
                        int laborDelta = Mathf.RoundToInt(newValue) - region.Population.LaborAvailable;
                        region.Population.UpdateLabor(laborDelta);
                        break;
                    case EffectTarget.InfrastructureLevel:
                        // For infrastructure level, we need to use specific methods or reflection
                        // This is simplified for the demo
                        int levels = Mathf.RoundToInt(newValue - region.Infrastructure.Level);
                        for (int i = 0; i < levels; i++)
                        {
                            region.Infrastructure.Upgrade();
                        }
                        break;
                    case EffectTarget.PopulationSatisfaction:
                        region.Population.UpdateSatisfaction(Mathf.Clamp01(newValue));
                        break;
                    
                    // Economic system parameters
                    case EffectTarget.ProductivityFactor:
                        system.productivityFactor = newValue;
                        break;
                    case EffectTarget.LaborElasticity:
                        system.laborElasticity = newValue;
                        break;
                    case EffectTarget.CapitalElasticity:
                        system.capitalElasticity = newValue;
                        break;
                    case EffectTarget.CycleMultiplier:
                        system.cycleMultiplier = newValue;
                        break;
                    case EffectTarget.WealthGrowthRate:
                        system.wealthGrowthRate = newValue;
                        break;
                    case EffectTarget.PriceVolatility:
                        system.priceVolatility = newValue;
                        break;
                    case EffectTarget.DecayRate:
                        system.decayRate = newValue;
                        break;
                    case EffectTarget.MaintenanceCostMultiplier:
                        system.maintenanceCostMultiplier = newValue;
                        break;
                    case EffectTarget.LaborConsumptionRate:
                        system.laborConsumptionRate = newValue;
                        break;
                }
            }
        }
        
        private void Awake()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            eventTrigger = FindFirstObjectByType<EconomicEventTrigger>();
            
            if (economicSystem == null)
            {
                Debug.LogWarning("EconomicEventEffect: No EconomicSystem found in the scene.");
            }
            
            if (eventTrigger == null)
            {
                Debug.LogWarning("EconomicEventEffect: No EconomicEventTrigger found in the scene.");
            }
            
            // Setup some example chain relationships
            SetupExampleEventChains();
            
            // Collect available event titles for fallback generation
            CollectAvailableEventTitles();
        }

        // Setup example event chains for testing
        private void SetupExampleEventChains()
        {
            // Example: From "Economic Reform Proposal", choice 0 leads to "National Arts Initiative"
            AddNextEvent("Economic Reform Proposal", 0, "National Arts Initiative");
            
            // Example: From "Resource Shortage", choice 0 leads to "Economic Reform Proposal"
            AddNextEvent("Resource Shortage", 0, "Economic Reform Proposal");
            
            // Example: From "Military Modernization", choice 0 leads to "Research Breakthrough"
            AddNextEvent("Military Modernization", 0, "Research Breakthrough");
            
            // Example: From "Diplomatic Incident", choice 1 leads to "Labor Shortage Crisis"
            AddNextEvent("Diplomatic Incident", 1, "Labor Shortage Crisis");
        }
        
        // Collect all available event titles to use for fallback event generation
        private void CollectAvailableEventTitles()
        {
            availableEventTitles.Clear();
            
            // Add some default event titles that we know exist in the sample events
            string[] defaultTitles = new string[]
            {
                "Economic Reform Proposal", 
                "National Arts Initiative",
                "Military Modernization", 
                "Resource Shortage",
                "Diplomatic Incident", 
                "Devastating Floods",
                "Research Breakthrough", 
                "Labor Shortage Crisis"
            };
            
            availableEventTitles.AddRange(defaultTitles);
            
            Debug.Log($"Collected {availableEventTitles.Count} events for fallback generation");
        }
        
        // Add a next event to chain to after a specific choice
        public void AddNextEvent(string currentEventId, int choiceIndex, string nextEventId)
        {
            if (!nextEventChain.ContainsKey(currentEventId))
            {
                nextEventChain[currentEventId] = new Dictionary<int, string>();
            }
            
            nextEventChain[currentEventId][choiceIndex] = nextEventId;
            Debug.Log($"Added next event: {currentEventId} choice {choiceIndex} → {nextEventId}");
        }
        
        // Get the next event ID for a choice (if any)
        public string GetNextEvent(string currentEventId, int choiceIndex)
        {
            if (nextEventChain.ContainsKey(currentEventId) && 
                nextEventChain[currentEventId].ContainsKey(choiceIndex))
            {
                return nextEventChain[currentEventId][choiceIndex];
            }
            
            return null;
        }
        
        // Generate a fallback event if we don't have a pre-defined next event
        private string GenerateFallbackEvent(string currentEventId, int choiceIndex)
        {
            if (!generateFallbackEvents || availableEventTitles.Count == 0 || eventTrigger == null)
                return null;
                
            // Don't use the current event as a fallback
            List<string> possibleEvents = availableEventTitles
                .Where(title => title != currentEventId)
                .ToList();
                
            if (possibleEvents.Count == 0)
                return null;
                
            // Pick a random event from our available options
            string nextEventId = possibleEvents[Random.Range(0, possibleEvents.Count)];
            Debug.Log($"Generating fallback event: {currentEventId} choice {choiceIndex} → {nextEventId} (fallback)");
            
            // Store this in our event chain for future use
            AddNextEvent(currentEventId, choiceIndex, nextEventId);
            
            return nextEventId;
        }

        // Apply multiple effects at once (from a dialogue choice)
        public void ApplyEffects(string eventId, int choiceIndex, List<ParameterEffect> effects)
        {
            if (economicSystem == null || economicSystem.testRegion == null)
                return;
                
            RegionEntity region = economicSystem.testRegion;
            
            Debug.Log($"Applying effects for event '{eventId}', choice {choiceIndex}:");
            foreach (var effect in effects)
            {
                effect.Apply(economicSystem, region);
            }
            
            // After applying effects, recalculate production
            RecalculateProduction(region);
            
            // Handle event chaining and event progression
            ProgressToNextEvent(eventId, choiceIndex);
        }
        
        // New method to handle progression to next event
        public void ProgressToNextEvent(string eventId, int choiceIndex)
        {
            if (eventTrigger == null)
                return;
            
            // Check if there's a next event to trigger
            string nextEventId = GetNextEvent(eventId, choiceIndex);
            
            // If we don't have a pre-defined next event, generate a fallback
            if (string.IsNullOrEmpty(nextEventId) && generateFallbackEvents)
            {
                nextEventId = GenerateFallbackEvent(eventId, choiceIndex);
            }
            
            if (!string.IsNullOrEmpty(nextEventId))
            {
                Debug.Log($"Triggering next event in chain: {nextEventId}");
                eventTrigger.TriggerEvent(nextEventId, true);
            }
            else
            {
                Debug.Log($"No next event available after {eventId} choice {choiceIndex}");
            }
            
            // Mark the current event as completed
            eventTrigger.CompleteEvent(eventId);
        }
        
        // Helper method to recalculate production after effects are applied
        private void RecalculateProduction(RegionEntity region)
        {
            float labor = region.Population.LaborAvailable;
            float capital = region.Infrastructure.Level;
            float productivityFactor = economicSystem.productivityFactor;
            float laborElasticity = economicSystem.laborElasticity;
            float capitalElasticity = economicSystem.capitalElasticity;
            
            // Cobb-Douglas production function
            float calculatedProduction = productivityFactor * 
                Mathf.Pow(Mathf.Max(1f, labor), laborElasticity) * 
                Mathf.Pow(Mathf.Max(1f, capital), capitalElasticity);
                
            int productionOutput = Mathf.RoundToInt(calculatedProduction);
            
            // Update production values
            region.Production.SetOutput(productionOutput);
            region.Economy.Production = productionOutput;
            
            Debug.Log($"Recalculated production: {productionOutput}");
        }
        
        // Simple method to quickly create and apply a single effect
        public void ApplySingleEffect(ParameterEffect.EffectTarget target, ParameterEffect.EffectType effectType, float value)
        {
            ParameterEffect effect = new ParameterEffect
            {
                target = target,
                effectType = effectType,
                value = value,
                description = $"{target} {effectType} {value}"
            };
            
            List<ParameterEffect> effects = new List<ParameterEffect> { effect };
            ApplyEffects("Direct Effect", 0, effects);
        }
    }
}