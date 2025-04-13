using UnityEngine;
using V2.Entities;
using V2.Managers;
using V2.Components;
using System.Collections.Generic;

namespace V2.Systems
{
    public class EconomicSystem : MonoBehaviour
    {
        public static EconomicSystem Instance { get; private set; }
        
        public RegionEntity testRegion;
        private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();

        [Header("Production Settings")]
        public float productivityFactor = 1.0f;
        public float laborElasticity = 0.5f;
        public float capitalElasticity = 0.5f;

        [Header("Economic Cycle")]
        public float cycleMultiplier = 1.05f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.Subscribe("TurnEnded", OnTurnEnded);
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
        }

        private void OnTurnEnded(object _)
        {
            ProcessEconomicTick();
        }

        private void OnRegionUpdated(object data)
        {
            if (data is RegionEntity region)
            {
                RegisterRegion(region);
            }
        }

        [ContextMenu("Run Economic Tick")]
        public void ManualTick()
        {
            ProcessEconomicTick();
        }

        public void RegisterRegion(RegionEntity region)
        {
            if (!regions.ContainsKey(region.Name))
            {
                regions.Add(region.Name, region);
                Debug.Log($"Region registered: {region.Name}");
            }
        }

        public void ProcessEconomicTick()
        {
            if (testRegion == null)
            {
                Debug.LogWarning("TestRegion not assigned.");
                return;
            }

            // Process each economic aspect using the new component architecture
            ProcessSupplyAndDemand(testRegion);
            ProcessProduction(testRegion);
            ProcessInfrastructure(testRegion);
            ProcessPopulationConsumption(testRegion);
            ProcessEconomicCycle(testRegion);
            ProcessPriceVolatility();
        }

        private void ProcessSupplyAndDemand(RegionEntity region)
        {
            Debug.Log("Processing Supply and Demand...");
            float supply = region.Economy.Production * 0.8f;
            float demand = region.Population.LaborAvailable * 1.2f;
            float imbalance = demand - supply;
            Debug.Log($"[Supply/Demand] Supply: {supply}, Demand: {demand}, Imbalance: {imbalance}");
        }

        private void ProcessProduction(RegionEntity region)
        {
            Debug.Log("Processing Production...");
            float labor = region.Population.LaborAvailable;
            float capital = region.Infrastructure.Level;

            // Cobb-Douglas production
            float production = productivityFactor 
                * Mathf.Pow(labor, laborElasticity) 
                * Mathf.Pow(capital, capitalElasticity);

            // Update output in ProductionComponent
            int productionOutput = Mathf.RoundToInt(production);
            
            // Set output through method (simulating the component doing the calculation)
            // In the real implementation, this would come from the component itself
            SetProductionOutput(region.Production, productionOutput);
            
            Debug.Log($"[Production] Labor: {labor}, Capital: {capital}, Production: {productionOutput}");
        }
        
        // Helper method to set production output without breaking encapsulation
        private void SetProductionOutput(ProductionComponent productionComponent, int output)
        {
            // This would ideally be replaced by ProductionComponent's own calculation
            // For now, we're simulating it externally
            typeof(ProductionComponent).GetField("output", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(productionComponent, output);
        }

        private void ProcessInfrastructure(RegionEntity region)
        {
            Debug.Log("Processing Infrastructure...");
            float maintenanceCost = region.Infrastructure.GetMaintenanceCost();
            
            // Apply maintenance cost to economy
            region.Economy.Wealth -= Mathf.RoundToInt(maintenanceCost);

            Debug.Log($"[Infrastructure] Level: {region.Infrastructure.Level}, Maintenance Cost: {maintenanceCost}");
        }

        private void ProcessPopulationConsumption(RegionEntity region)
        {
            Debug.Log("Processing Population Consumption...");
            float consumption = region.Population.LaborAvailable * 1.5f;
            float unmetDemand = Mathf.Max(0, consumption - region.Economy.Production);
            float satisfaction = 1.0f;
            
            if (consumption > 0)
            {
                float unrestFactor = unmetDemand / consumption;
                satisfaction = Mathf.Clamp01(1.0f - unrestFactor);
            }

            // Update population satisfaction
            region.Population.UpdateSatisfaction(satisfaction);
            
            Debug.Log($"[Consumption] Total: {consumption}, Unmet: {unmetDemand}, Satisfaction: {satisfaction:F2}");
        }

        private void ProcessEconomicCycle(RegionEntity region)
        {
            Debug.Log("Processing Economic Cycle...");
            
            // Apply cycle effects to economy
            region.Economy.Production = Mathf.RoundToInt(region.Economy.Production * cycleMultiplier);
            region.Economy.Wealth = Mathf.RoundToInt(region.Economy.Wealth * cycleMultiplier);
            
            Debug.Log($"[Cycle] Production: {region.Economy.Production}, Wealth: {region.Economy.Wealth}");
        }

        private void ProcessPriceVolatility()
        {
            Debug.Log("Processing Price Volatility...");
            float volatility = Random.Range(-0.1f, 0.1f);
            float priceIndex = 100f * (1 + volatility);
            Debug.Log($"[Prices] Volatility: {volatility:F2}, Price Index: {priceIndex:F2}");
        }
    }
}