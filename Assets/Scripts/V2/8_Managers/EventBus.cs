using System;
using System.Collections.Generic;
using UnityEngine;

namespace V2.Managers
{
    /// <summary>
    /// CLASS PURPOSE:
    /// EventBus provides a centralized messaging system for loosely coupling different parts
    /// of the application. It allows components to subscribe to and trigger named events
    /// without requiring direct references between sender and receiver.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Maintain a dictionary of string-keyed events
    /// - Allow listeners to subscribe and unsubscribe to named events
    /// - Dispatch triggered events to all subscribed listeners with optional payloads
    ///
    /// KEY COLLABORATORS:
    /// - Any MonoBehaviour or system that needs to communicate with other parts of the game
    /// - GameManager, TurnManager, UI components, and simulation systems
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses static dictionary and delegates for global access and loose coupling
    /// - Defensive coding ensures listeners are safely invoked
    /// - Simplified, scalable foundation for event-driven design
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Consider using strongly typed event identifiers (e.g., enums or structs)
    /// - Extend support for multiple argument types or generic payloads
    /// - Separate game-wide vs. scoped/local events
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add event prioritization or execution order
    /// - Implement event logging or visualization tools
    /// - Support pauseable/resumable event queues for simulation stepping
    /// </summary>

    public static class EventBus
    {
        private static Dictionary<string, Action<object>> eventDictionary = new();
        private static bool detailedLogging = true; // Enable for more detailed error information

        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (listener == null)
            {
                Debug.LogWarning($"EventBus: Attempted to subscribe null listener to event '{eventName}'");
                return;
            }

            if (!eventDictionary.ContainsKey(eventName))
                eventDictionary[eventName] = null;

            eventDictionary[eventName] += listener;
        }

        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (listener == null || !eventDictionary.ContainsKey(eventName))
                return;

            eventDictionary[eventName] -= listener;
            if (eventDictionary[eventName] == null)
                eventDictionary.Remove(eventName);
        }

        public static void Trigger(string eventName, object eventData = null)
        {
            if (eventDictionary.TryGetValue(eventName, out var callbacks) && callbacks != null)
            {
                foreach (var callback in callbacks.GetInvocationList())
                {
                    if (callback == null) continue;

                    try 
                    { 
                        var action = (Action<object>)callback;
                        if (detailedLogging)
                        {
                            // Log which method is being called to help with debugging
                            string methodInfo = callback.Method.DeclaringType?.FullName + "." + callback.Method.Name;
                            Debug.Log($"EventBus: Triggering '{eventName}' on {methodInfo}");
                        }
                        
                        action.Invoke(eventData);
                    }
                    catch (NullReferenceException nre)
                    {
                        // Specific handling for null reference exceptions
                        string methodInfo = callback.Method.DeclaringType?.FullName + "." + callback.Method.Name;
                        string targetInfo = callback.Target?.GetType().FullName ?? "null";
                        
                        Debug.LogError($"EventBus: NullReferenceException in '{eventName}' callback: {methodInfo}\n" +
                                      $"Target object: {targetInfo}\n" +
                                      $"Error: {nre.Message}\n{nre.StackTrace}");
                    }
                    catch (Exception e)
                    {
                        string methodInfo = callback.Method.DeclaringType?.FullName + "." + callback.Method.Name;
                        Debug.LogError($"EventBus error in '{eventName}' callback: {methodInfo}\n" +
                                      $"Error: {e.Message}\n{e.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Enable or disable detailed event logging
        /// </summary>
        public static void SetDetailedLogging(bool enabled)
        {
            detailedLogging = enabled;
        }

        /// <summary>
        /// Clear all event subscriptions - useful for scene changes or testing
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            eventDictionary.Clear();
            Debug.Log("EventBus: All event subscriptions cleared");
        }
    }
}