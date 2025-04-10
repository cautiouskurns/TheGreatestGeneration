using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace V2.Components
{
    /// <summary>
    /// CLASS PURPOSE:
    /// ResourceComponent manages raw resource accumulation for a RegionEntity, simulating
    /// basic economic inputs like Food or Wood.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Store and manage the quantities of various resource types
    /// - Generate new resources each turn using predefined rules
    /// - Provide external systems with safe read-only access to current resource states
    ///
    /// KEY COLLABORATORS:
    /// - RegionEntity: Owns and invokes this component per simulation turn
    /// - ProductionComponent: Reads resource values for production transformations
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Internally uses a dictionary of string keys for extensibility and quick lookups
    /// - Logs all updates to the Unity Console for early-stage debug visibility
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace string keys with a strongly typed enum for resource safety
    /// - Move generation logic to configurable data sources or strategy classes
    /// - Implement resource constraints or caps to simulate scarcity or storage
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add methods for consumption, transfer, or trading of resources
    /// - Add modifiers from traits, buildings, policies, or infrastructure
    /// - Visualize resource flow over time using telemetry or UI dashboards
    /// </summary>
    public class ResourceComponent
    {
        private Dictionary<string, float> resources = new()
        {
            { "Food", 0f },
            { "Wood", 0f }
        };

        /// <summary>
        /// Increments all resource values based on fixed per-turn generation rules.
        /// </summary>
        public void GenerateResources()
        {
            resources["Food"] += 10f;
            resources["Wood"] += 5f;
            Debug.Log($"Resources updated - Food: {resources["Food"]}, Wood: {resources["Wood"]}");
        }

        /// <summary>
        /// Returns a shallow copy of the current resource dictionary for safe external access.
        /// </summary>
        public Dictionary<string, float> GetAllResources()
        {
            return new Dictionary<string, float>(resources);
        }

        /// <summary>
        /// Returns a formatted string showing current resource amounts for UI or debugging.
        /// </summary>
        public string GetResourceOverview()
        {
            return string.Join(", ", resources.Select(kv => $"{kv.Key}: {kv.Value:F1}"));
        }
    }
}