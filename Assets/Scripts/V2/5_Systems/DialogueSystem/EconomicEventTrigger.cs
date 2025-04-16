using UnityEngine;
using System.Collections.Generic;
using V2.Systems;
using V2.Entities;
using V2.Managers;

namespace V2.Systems.DialogueSystem
{
    /// <summary>
    /// Monitors economic parameters and triggers events when thresholds are reached
    /// </summary>
    public class EconomicEventTrigger : MonoBehaviour
    {
        // Reference to the economic system
        private EconomicSystem economicSystem;
        
        // List of parameter thresholds that can trigger events
        [System.Serializable]
        public class EventTriggerCondition
        {
            public enum ParameterType
            {
                Wealth,
                Production,
                LaborAvailable,
                InfrastructureLevel,
                PopulationSatisfaction,
                ProductivityFactor,
                CycleMultiplier
            }
            
            public enum ComparisonType
            {
                GreaterThan,
                LessThan,
                Equals
            }
            
            public ParameterType parameter;
            public ComparisonType comparison;
            public float thresholdValue;
            public string eventId;
            public bool hasTriggered = false; // Prevents repeated triggering
            
            public bool CheckCondition(EconomicSystem system, RegionEntity region)
            {
                if (hasTriggered || system == null || region == null)
                    return false;
                    
                float currentValue = GetParameterValue(parameter, system, region);
                
                bool conditionMet = false;
                switch (comparison)
                {
                    case ComparisonType.GreaterThan:
                        conditionMet = currentValue > thresholdValue;
                        break;
                    case ComparisonType.LessThan:
                        conditionMet = currentValue < thresholdValue;
                        break;
                    case ComparisonType.Equals:
                        conditionMet = Mathf.Approximately(currentValue, thresholdValue);
                        break;
                }
                
                return conditionMet;
            }
            
            private float GetParameterValue(ParameterType parameter, EconomicSystem system, RegionEntity region)
            {
                switch (parameter)
                {
                    case ParameterType.Wealth:
                        return region.Economy.Wealth;
                    case ParameterType.Production:
                        return region.Economy.Production;
                    case ParameterType.LaborAvailable:
                        return region.Population.LaborAvailable;
                    case ParameterType.InfrastructureLevel:
                        return region.Infrastructure.Level;
                    case ParameterType.PopulationSatisfaction:
                        return region.Population.Satisfaction;
                    case ParameterType.ProductivityFactor:
                        return system.productivityFactor;
                    case ParameterType.CycleMultiplier:
                        return system.cycleMultiplier;
                    default:
                        return 0f;
                }
            }
        }
        
        // List of event trigger conditions to check
        [SerializeField]
        public List<EventTriggerCondition> triggerConditions = new List<EventTriggerCondition>();
        
        // A list to store triggered event IDs that need to be displayed
        public List<string> triggeredEvents = new List<string>();
        
        // For testing in editor
        public bool debugMode = true;
        
        private void Awake()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            
            if (economicSystem == null)
            {
                Debug.LogWarning("EconomicEventTrigger: No EconomicSystem found in the scene.");
            }
            
            // Add some example trigger conditions for testing
            if (triggerConditions.Count == 0 && debugMode)
            {
                AddExampleTriggerConditions();
            }
        }
        
        private void AddExampleTriggerConditions()
        {
            // Example 1: Trigger "Economic Boom" event when wealth exceeds 500
            triggerConditions.Add(new EventTriggerCondition
            {
                parameter = EventTriggerCondition.ParameterType.Wealth,
                comparison = EventTriggerCondition.ComparisonType.GreaterThan,
                thresholdValue = 500f,
                eventId = "Economic Boom"
            });
            
            // Example 2: Trigger "Labor Shortage" event when labor falls below 50
            triggerConditions.Add(new EventTriggerCondition
            {
                parameter = EventTriggerCondition.ParameterType.LaborAvailable,
                comparison = EventTriggerCondition.ComparisonType.LessThan,
                thresholdValue = 50f,
                eventId = "Labor Shortage"
            });
            
            // Example 3: Trigger "Infrastructure Crisis" when infrastructure level is too low
            triggerConditions.Add(new EventTriggerCondition
            {
                parameter = EventTriggerCondition.ParameterType.InfrastructureLevel,
                comparison = EventTriggerCondition.ComparisonType.LessThan,
                thresholdValue = 2f,
                eventId = "Infrastructure Crisis"
            });
        }
        
        public void CheckTriggerConditions()
        {
            if (economicSystem == null || economicSystem.testRegion == null)
                return;
                
            RegionEntity region = economicSystem.testRegion;
            
            foreach (var condition in triggerConditions)
            {
                if (!condition.hasTriggered && condition.CheckCondition(economicSystem, region))
                {
                    // Event condition met, trigger the event
                    TriggerEvent(condition.eventId);
                    condition.hasTriggered = true;
                    
                    if (debugMode)
                    {
                        Debug.Log($"EconomicEventTrigger: Triggered event '{condition.eventId}' due to {condition.parameter} {condition.comparison} {condition.thresholdValue}");
                    }
                }
            }
        }
        
        private void TriggerEvent(string eventId)
        {
            // Add to the list of triggered events
            if (!triggeredEvents.Contains(eventId))
            {
                triggeredEvents.Add(eventId);
            }
            
            // In a full implementation, this would communicate with the event/dialogue system
            // For now, we'll just log the event
            Debug.Log($"EVENT TRIGGERED: {eventId}");
        }
        
        // Reset a specific trigger condition to allow it to fire again
        public void ResetTriggerCondition(string eventId)
        {
            foreach (var condition in triggerConditions)
            {
                if (condition.eventId == eventId)
                {
                    condition.hasTriggered = false;
                    break;
                }
            }
        }
        
        // Reset all trigger conditions
        public void ResetAllTriggerConditions()
        {
            foreach (var condition in triggerConditions)
            {
                condition.hasTriggered = false;
            }
            triggeredEvents.Clear();
        }
        
        // Used by EconomicSystem to check for events after each tick
        private void OnEnable()
        {
            if (economicSystem != null)
            {
                // Subscribe to the event that fires after an economic tick
                EventBus.Subscribe("EconomicTick", OnEconomicTick);
            }
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
        }
        
        private void OnEconomicTick(object data)
        {
            CheckTriggerConditions();
        }
    }
}