using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace V2.Components
{
    public class ResourceComponent
    {
        private Dictionary<string, float> resources = new()
        {
            {"Food", 0f},
            {"Wood", 0f}
        };

        public void GenerateResources()
        {
            resources["Food"] += 10f;
            resources["Wood"] += 5f;
            Debug.Log($"Resources updated - Food: {resources["Food"]}, Wood: {resources["Wood"]}");
        }

        public Dictionary<string, float> GetAllResources()
        {
            return new Dictionary<string, float>(resources);
        }

        public string GetResourceOverview()
        {
            return string.Join(", ", resources.Select(kv => $"{kv.Key}: {kv.Value:F1}"));
        }
    }
}
