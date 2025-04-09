/// CLASS PURPOSE:
/// EventBus provides a centralized messaging system for loosely coupling different parts
/// of the application. It allows components to subscribe to and trigger named events
/// without direct references between sender and receiver.
///
/// CORE RESPONSIBILITIES:
/// - Maintain a dictionary of events keyed by string names
/// - Allow listeners to subscribe and unsubscribe from named events
/// - Trigger events with optional payloads, invoking all subscribed listeners
///
/// KEY COLLABORATORS:
/// - Any MonoBehaviour or system script: Can publish or subscribe to events
/// - Debugging and UI: May interact indirectly through event-based updates
///
/// CURRENT ARCHITECTURE NOTES:
/// - Uses static methods and dictionary to globally manage event flow
/// - Defensive programming handles null listener lists and safe invocation
///
/// REFACTORING SUGGESTIONS:
/// - Replace string keys with a strongly typed enum or event ID struct for safety
/// - Add support for multiple listener signatures using generics or overloads
///
/// EXTENSION OPPORTUNITIES:
/// - Support priority-based event ordering
/// - Add scoped or local event channels for sub-system isolation
/// - Provide performance metrics or logging for event traffic analysis

using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();

    public static void Subscribe(string eventName, Action<object> listener)
    {
        // Debug.Log($"EventBus: Subscribing to '{eventName}' event with listener {listener.Method.DeclaringType}.{listener.Method.Name}");
        
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = null;
        }
        
        eventDictionary[eventName] += listener;
    }

    public static void Unsubscribe(string eventName, Action<object> listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
            
            // Clean up if there are no more listeners
            if (eventDictionary[eventName] == null)
            {
                eventDictionary.Remove(eventName);
            }
        }
    }

    public static void Trigger(string eventName, object eventData = null)
    {
        // Debug.Log($"EventBus: Triggering '{eventName}' event");
        
        if (eventDictionary.ContainsKey(eventName) && eventDictionary[eventName] != null)
        {
            // Create a safe copy of the delegate to avoid problems if a listener unsubscribes during event processing
            var invocationList = eventDictionary[eventName].GetInvocationList();
            
            // Debug.Log($"EventBus: Found {invocationList.Length} listeners for '{eventName}'");
            
            foreach (var callback in invocationList)
            {
                try
                {
                    // Cast and invoke each delegate individually for safer error handling
                    ((Action<object>)callback).Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error in event listener for {eventName}: {e.Message}\n{e.StackTrace}");
                    // Continue processing other listeners despite the error
                }
            }
        }
        else
        {
            // Debug.Log($"EventBus: No listeners for '{eventName}'");
        }
    }
}
