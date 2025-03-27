using System;
using System.Collections.Generic;
using UnityEngine; // Add this for Debug.Log

public static class EventBus
{
    private static Dictionary<string, Action<object>> eventDictionary = new Dictionary<string, Action<object>>();

    public static void Subscribe(string eventName, Action<object> listener)
    {
//        Debug.Log($"EventBus: Subscribing to '{eventName}' event with listener {listener.Method.DeclaringType}.{listener.Method.Name}");
        
        if (!eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] = delegate { };
        }
        eventDictionary[eventName] += listener;
    }

    public static void Unsubscribe(string eventName, Action<object> listener)
    {
        if (eventDictionary.ContainsKey(eventName))
        {
            eventDictionary[eventName] -= listener;
        }
    }

    public static void Trigger(string eventName, object eventData = null)
    {
//        Debug.Log($"EventBus: Triggering '{eventName}' event");
        
        if (eventDictionary.ContainsKey(eventName))
        {
        //    Debug.Log($"EventBus: Found {eventDictionary[eventName].GetInvocationList().Length} listeners for '{eventName}'");
            eventDictionary[eventName].Invoke(eventData);
        }
        else
        {
//            Debug.Log($"EventBus: No listeners for '{eventName}'");
        }
    }
}

