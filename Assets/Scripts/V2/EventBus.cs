using System;
using System.Collections.Generic;
using UnityEngine;

namespace V2
{
    public static class EventBus
    {
        private static Dictionary<string, List<Action<object>>> eventHandlers = new Dictionary<string, List<Action<object>>>();
        
        public static void Subscribe(string eventName, Action<object> callback)
        {
            if (!eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName] = new List<Action<object>>();
            }
            
            eventHandlers[eventName].Add(callback);
        }
        
        public static void Unsubscribe(string eventName, Action<object> callback)
        {
            if (eventHandlers.ContainsKey(eventName))
            {
                eventHandlers[eventName].Remove(callback);
            }
        }
        
        public static void Trigger(string eventName, object data = null)
        {
            if (eventHandlers.ContainsKey(eventName))
            {
                // Create a copy to avoid issues if handlers subscribe/unsubscribe during iteration
                var handlers = new List<Action<object>>(eventHandlers[eventName]);
                
                foreach (var handler in handlers)
                {
                    try
                    {
                        handler(data);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error in event handler for {eventName}: {e}");
                    }
                }
            }
        }
    }
}