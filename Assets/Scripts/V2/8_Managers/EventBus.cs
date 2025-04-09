using System;
using System.Collections.Generic;
using UnityEngine;

namespace V2.Managers
{
    public static class EventBus
    {
        private static Dictionary<string, Action<object>> eventDictionary = new();

        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (!eventDictionary.ContainsKey(eventName))
                eventDictionary[eventName] = null;

            eventDictionary[eventName] += listener;
        }

        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (eventDictionary.ContainsKey(eventName))
            {
                eventDictionary[eventName] -= listener;
                if (eventDictionary[eventName] == null)
                    eventDictionary.Remove(eventName);
            }
        }

        public static void Trigger(string eventName, object eventData = null)
        {
            if (eventDictionary.TryGetValue(eventName, out var callbacks) && callbacks != null)
            {
                foreach (var callback in callbacks.GetInvocationList())
                {
                    try { ((Action<object>)callback).Invoke(eventData); }
                    catch (Exception e) { Debug.LogError($"EventBus error in '{eventName}': {e.Message}\n{e.StackTrace}"); }
                }
            }
        }
    }
}