using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using V2.Entities;
using V2.Systems;

namespace V2.Systems.DialogueSystem
{
    [System.Serializable]
    public class DialogueEvent
    {
        public string id;
        public string title;
        public string description;
        public EventCategory category;
        public bool hasTriggered = false;
        
        // Event trigger conditions
        public List<EventCondition> conditions = new List<EventCondition>();
        
        // Choices available to player
        public List<EventChoice> choices = new List<EventChoice>();
        
        // For editor organization
        [HideInInspector] public bool showConditions = true;
        [HideInInspector] public bool showChoices = true;
        
        // Check if all conditions are met
        public bool CheckConditions(EconomicSystem system, RegionEntity region)
        {
            if (conditions.Count == 0)
                return false;
                
            return conditions.All(c => c.CheckCondition(system, region));
        }
        
        // Apply the effects of a choice
        public void ApplyChoice(int choiceIndex, EconomicSystem system, RegionEntity region)
        {
            if (choiceIndex < 0 || choiceIndex >= choices.Count)
                return;
                
            EventChoice choice = choices[choiceIndex];
            foreach (var effect in choice.economicEffects)
            {
                effect.Apply(system, region);
            }
            
            Debug.Log($"Applied choice {choiceIndex} for event '{title}'");
            
            // Recalculate production after applying effects
            RecalculateProduction(system, region);
        }
        
        // Helper method to recalculate production after effects are applied
        private void RecalculateProduction(EconomicSystem system, RegionEntity region)
        {
            float labor = region.Population.LaborAvailable;
            float capital = region.Infrastructure.Level;
            float productivityFactor = system.productivityFactor;
            float laborElasticity = system.laborElasticity;
            float capitalElasticity = system.capitalElasticity;
            
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
        
        // Get next event in the chain (if any)
        public string GetNextEventId(int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex >= choices.Count)
                return null;
                
            return choices[choiceIndex].nextEventId;
        }
        
        // Clone the event (for editor operations)
        public DialogueEvent Clone()
        {
            var clone = new DialogueEvent
            {
                id = id,
                title = title,
                description = description,
                category = category,
                hasTriggered = hasTriggered,
                showConditions = showConditions,
                showChoices = showChoices
            };
            
            // Clone conditions
            clone.conditions = new List<EventCondition>();
            foreach (var condition in conditions)
            {
                clone.conditions.Add(new EventCondition
                {
                    parameter = condition.parameter,
                    comparison = condition.comparison,
                    thresholdValue = condition.thresholdValue
                });
            }
            
            // Clone choices
            clone.choices = new List<EventChoice>();
            foreach (var choice in choices)
            {
                clone.choices.Add(new EventChoice(choice));
            }
            
            return clone;
        }
    }
}