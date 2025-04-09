using UnityEngine;
using System.Linq;
using V2.Components;

namespace V2.Components
{
    public class ProductionComponent
    {
        private ResourceComponent resourceComponent;

        public ProductionComponent(ResourceComponent resourceComponent)
        {
            this.resourceComponent = resourceComponent;
        }

        public void ProcessProduction()
        {
            Debug.Log("ProductionComponent: processing production...");
        }

        public string GetResourceOverview()
        {
            var resources = resourceComponent.GetAllResources();
            return string.Join(", ", resources.Select(kv => $"{kv.Key}: {kv.Value:F1}"));
        }
    }
}