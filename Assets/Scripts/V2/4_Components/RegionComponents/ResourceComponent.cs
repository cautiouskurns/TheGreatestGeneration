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

        public void GenerateResources()
        {
            resources["Food"] += 10f;
            resources["Wood"] += 5f;
            
            Debug.Log($"Resources updated: {GetResourceOverview()}");
        }

        public Dictionary<string, float> GetAllResources()
        {
            return new Dictionary<string, float>(resources);
        }

        public float GetResource(string name)
        {
            return resources.ContainsKey(name) ? resources[name] : 0;
        }

        public bool UseResource(string name, float amount)
        {
            if (resources.ContainsKey(name) && resources[name] >= amount)
            {
                resources[name] -= amount;
                return true;
            }
            return false;
        }

        public string GetResourceOverview()
        {
            return string.Join(", ", resources.Select(kv => $"{kv.Key}: {kv.Value:F1}"));
        }
    }
}