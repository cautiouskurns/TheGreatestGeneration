using UnityEngine;
using System.Collections.Generic;
using V2.Systems;
using V2.Entities;
using V2.Managers;

namespace V2.Systems.DialogueSystem
{
    /// <summary>
    /// Handles triggering events based on conditions
    /// </summary>
    public class EconomicEventTrigger : MonoBehaviour
    {
        // List of events that have been triggered
        private HashSet<string> triggeredEvents = new HashSet<string>();
        
        // List of events that have been completed (player made a choice)
        private HashSet<string> completedEvents = new HashSet<string>();
        
        // Cache of current event ID being displayed
        private string currentEventId = string.Empty;
        
        // Reference to event effect component
        private EconomicEventEffect eventEffect;
        
        private void Awake()
        {
            eventEffect = FindFirstObjectByType<EconomicEventEffect>();
            
            if (eventEffect == null)
            {
                Debug.LogWarning("EconomicEventTrigger: No EconomicEventEffect found in the scene.");
            }
        }
        
        /// <summary>
        /// Check if an event has been triggered
        /// </summary>
        public bool IsEventTriggered(string eventId)
        {
            return triggeredEvents.Contains(eventId);
        }
        
        /// <summary>
        /// Check if an event has been completed
        /// </summary>
        public bool IsEventCompleted(string eventId)
        {
            return completedEvents.Contains(eventId);
        }
        
        /// <summary>
        /// Mark an event as triggered
        /// </summary>
        public void TriggerEvent(string eventId, bool isManualTrigger = false)
        {
            if (string.IsNullOrEmpty(eventId))
                return;
                
            // Check if we're already showing this event
            if (currentEventId == eventId && !isManualTrigger)
                return;
                
            // If it's a manual trigger (from the event chain system), force it even if already triggered
            if (isManualTrigger || !triggeredEvents.Contains(eventId))
            {
                // Update the current event ID
                currentEventId = eventId;
                
                // Add to triggered events
                triggeredEvents.Add(eventId);
                
                Debug.Log($"Event triggered: {eventId} (manual: {isManualTrigger})");
            }
        }
        
        /// <summary>
        /// Mark an event as completed
        /// </summary>
        public void CompleteEvent(string eventId)
        {
            if (string.IsNullOrEmpty(eventId))
                return;
                
            // Add to completed events
            if (!completedEvents.Contains(eventId))
            {
                completedEvents.Add(eventId);
                Debug.Log($"Event completed: {eventId}");
                
                // Clear current event if we just completed it
                if (currentEventId == eventId)
                {
                    currentEventId = string.Empty;
                }
            }
        }
        
        /// <summary>
        /// Reset the trigger system
        /// </summary>
        public void ResetTriggers()
        {
            triggeredEvents.Clear();
            completedEvents.Clear();
            currentEventId = string.Empty;
            Debug.Log("Event triggers reset");
        }
        
        /// <summary>
        /// Get the current event ID
        /// </summary>
        public string GetCurrentEventId()
        {
            return currentEventId;
        }
        
        /// <summary>
        /// Check for any ongoing events
        /// </summary>
        public bool HasOngoingEvent()
        {
            return !string.IsNullOrEmpty(currentEventId);
        }
    }
}