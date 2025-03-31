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
