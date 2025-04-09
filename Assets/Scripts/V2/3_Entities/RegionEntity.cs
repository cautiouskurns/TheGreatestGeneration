using UnityEngine;
using V2.Components;
using V2.Managers;

namespace V2.Entities
{
    public class RegionEntity
    {
        public string Name { get; private set; }
        public int Wealth { get; private set; }
        public int Production { get; private set; }

        private ResourceComponent resources;
        private ProductionComponent productionComponent;

        public RegionEntity(string name, int initialWealth, int initialProduction)
        {
            Name = name;
            Wealth = initialWealth;
            Production = initialProduction;
            resources = new ResourceComponent();
            productionComponent = new ProductionComponent(resources);
        }

        public void ProcessTurn()
        {
            Debug.Log($"[Region: {Name}] Processing Turn...");
            resources.GenerateResources();
            productionComponent.ProcessProduction();
            Wealth += 5;
            Production += 10;
            Debug.Log($"[Region: {Name}] Wealth: {Wealth}, Production: {Production}");
            EventBus.Trigger("RegionUpdated", this);
        }

        public string GetSummary()
        {
            return $"[{Name}] Wealth: {Wealth}, Production: {Production}, Resources: {resources.GetResourceOverview()}";
        }
    }
}