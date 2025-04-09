using UnityEngine;
using System.Collections.Generic;

namespace V1.Data
{

    /// CLASS PURPOSE:
    /// DialogueEventLibrarySO serves as a centralized repository for all simple dialogue events,
    /// categorized by type. It supports both sequential and random access to events for narrative
    /// generation, decision-making, and simulation triggers.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Store categorized collections of SimpleDialogueEvent objects
    /// - Provide runtime lookup by ID using a dictionary for quick access
    /// - Support filtered and random event retrieval by event category
    /// 
    /// KEY COLLABORATORS:
    /// - SimpleDialogueEvent: ScriptableObject representing a narrative or decision-making event
    /// - NarrativeSystem or EventManager: Consumes dialogue events based on context
    /// - GameStateManager: May trigger events based on economic or political conditions
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Runtime dictionary is built during initialization from allEvents list
    /// - Events are split into predefined categories (Economic, Resource, etc.)
    /// - Duplicate IDs are silently overwritten; consider warning or validation
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Replace hardcoded category lists with a dictionary for category-to-event mapping
    /// - Validate uniqueness of event IDs during initialization
    /// - Allow dynamic registration or loading of event packs
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Add support for event weighting or rarity in random selection
    /// - Include localization hooks for dialogue content
    /// - Support chained or conditional events for advanced narratives
    /// 
    [CreateAssetMenu(fileName = "DialogueEventLibrary", menuName = "Economic Cycles/Dialogue Event Library")]
    public class DialogueEventLibrarySO : ScriptableObject
    {
        [Header("Event Collections")]
        [SerializeField] private List<SimpleDialogueEvent> allEvents = new List<SimpleDialogueEvent>();
        
        [Header("Event Categories")]
        [SerializeField] private List<SimpleDialogueEvent> economicEvents = new List<SimpleDialogueEvent>();
        [SerializeField] private List<SimpleDialogueEvent> resourceEvents = new List<SimpleDialogueEvent>();
        [SerializeField] private List<SimpleDialogueEvent> diplomaticEvents = new List<SimpleDialogueEvent>();
        [SerializeField] private List<SimpleDialogueEvent> disasterEvents = new List<SimpleDialogueEvent>();
        
        // Dictionary for fast lookup by ID (populated at runtime)
        private Dictionary<string, SimpleDialogueEvent> eventDictionary = new Dictionary<string, SimpleDialogueEvent>();
        
        public void Initialize()
        {
            // Build lookup dictionary
            eventDictionary.Clear();
            foreach (var dialogueEvent in allEvents)
            {
                if (!string.IsNullOrEmpty(dialogueEvent.id))
                {
                    eventDictionary[dialogueEvent.id] = dialogueEvent;
                }
                else
                {
                    Debug.LogWarning($"Event with title '{dialogueEvent.title}' has no ID and won't be added to lookup dictionary");
                }
            }
            
            Debug.Log($"DialogueEventLibrary initialized with {eventDictionary.Count} events");
        }
        
        public SimpleDialogueEvent GetEventById(string eventId)
        {
            if (eventDictionary.TryGetValue(eventId, out SimpleDialogueEvent dialogueEvent))
            {
                return dialogueEvent;
            }
            
            Debug.LogWarning($"Event with ID '{eventId}' not found");
            return null;
        }
        
        public List<SimpleDialogueEvent> GetEventsByCategory(EventCategory category)
        {
            switch (category)
            {
                case EventCategory.Economic:
                    return economicEvents;
                case EventCategory.Resource:
                    return resourceEvents;
                case EventCategory.Diplomatic:
                    return diplomaticEvents;
                case EventCategory.Disaster:
                    return disasterEvents;
                default:
                    return allEvents;
            }
        }
        
        public SimpleDialogueEvent GetRandomEvent(EventCategory category)
        {
            var events = GetEventsByCategory(category);
            if (events == null || events.Count == 0)
                return null;
                
            return events[Random.Range(0, events.Count)];
        }
        
        public enum EventCategory
        {
            All,
            Economic,
            Resource,
            Diplomatic,
            Disaster
        }
    }
}