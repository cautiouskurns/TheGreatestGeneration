using UnityEngine;
using System.Collections.Generic; 
using System.Linq;
using System;

public class StoryEventSystem : MonoBehaviour
{
    public List<StoryEvent> allEvents = new List<StoryEvent>();
    
    private GameStateManager stateManager;
    private EventConditionEvaluator conditionEvaluator;
    
    private void Start()
    {
        stateManager = GameStateManager.Instance;
        conditionEvaluator = new EventConditionEvaluator(stateManager);
        
        // Subscribe to turn end events
        EventBus.Subscribe("TurnEnded", CheckForEvents);
    }
    
    private void CheckForEvents(object _)
    {
        // First update the game state
        stateManager.UpdateState();
        
        // Get all eligible events
        // List<StoryEvent> eligibleEvents = allEvents
        //     .Where(evt => conditionEvaluator.CheckEventEligibility(evt))
        //     .ToList();
            
        // if (eligibleEvents.Count > 0)
        // {
        //     // Select an event (could be random, weighted, or priority-based)
        //     StoryEvent selectedEvent = ChooseEvent(eligibleEvents);
            
        //     // Trigger the event
        //     TriggerEvent(selectedEvent);
        // }
    }
    
    private StoryEvent ChooseEvent(List<StoryEvent> eligibleEvents)
    {
        // Add logic to pick the most appropriate event
        // Could weight by priority, rarity, or relevance to current game state
        return eligibleEvents[0]; // Simple version just takes the first one
    }
    
    private void TriggerEvent(StoryEvent storyEvent)
    {
        // Record that this event occurred
        stateManager.History.LastEventOccurrence[storyEvent.Id] = DateTime.Now;
        
        // Increment count of this event type
        if (!stateManager.History.EventCounts.ContainsKey(storyEvent.Id))
            stateManager.History.EventCounts[storyEvent.Id] = 0;
        stateManager.History.EventCounts[storyEvent.Id]++;
        
        // Show event UI
        EventDialogueManager.ShowEventDialogue(storyEvent, stateManager);
    }
}