using System.Collections.Generic;
using UnityEngine;

namespace V2.Systems.DialogueSystem
{
    [System.Serializable]
    public class EventChoice
    {
        public string text;
        public string response;
        public List<string> narrativeEffects = new List<string>(); // Text descriptions of effects (for display)
        public List<ParameterEffect> economicEffects = new List<ParameterEffect>(); // Actual economic effects
        public string nextEventId; // The ID of the next event in a chain, if any
        
        // For editor organization
        [HideInInspector] public bool showEffects = true;
        
        public EventChoice()
        {
            // Default constructor
        }
        
        // Clone constructor for editor operations
        public EventChoice(EventChoice source)
        {
            text = source.text;
            response = source.response;
            nextEventId = source.nextEventId;
            
            // Clone narrative effects
            narrativeEffects = new List<string>(source.narrativeEffects);
            
            // Clone economic effects
            economicEffects = new List<ParameterEffect>();
            foreach (var effect in source.economicEffects)
            {
                economicEffects.Add(new ParameterEffect
                {
                    target = effect.target,
                    effectType = effect.effectType,
                    value = effect.value,
                    description = effect.description
                });
            }
        }
    }
}