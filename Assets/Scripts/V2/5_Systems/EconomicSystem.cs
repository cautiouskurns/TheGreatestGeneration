using UnityEngine;
using V2.Entities;
using V2.Managers;
using System.Collections.Generic;

namespace V2.Systems
{
    public class EconomicSystem : MonoBehaviour
    {
        public static EconomicSystem Instance { get; private set; }
        
        public RegionEntity testRegion;
        private Dictionary<string, RegionEntity> regions = new Dictionary<string, RegionEntity>();

        [Header("Simple Economic Settings")]
        public float productivityFactor = 1.0f;
        public float laborElasticity = 0.5f;
        public float capitalElasticity = 0.5f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // optional safety
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.Subscribe("TurnEnded", OnTurnEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
        }

        private void OnTurnEnded(object _)
        {
            ProcessEconomicTick();
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
            }
        }

        public void ProcessEconomicTick()
        {
            if (testRegion == null)
            {
                Debug.LogWarning("TestRegion not assigned.");
                return;
            }

            ProcessSupplyAndDemand();
            ProcessProduction();
            ProcessInfrastructure();
            ProcessPopulationConsumption();
            ProcessEconomicCycle();
            ProcessPriceVolatility();
        }

        private void ProcessSupplyAndDemand()
        {
            Debug.Log("Processing Supply and Demand...");
            float supply = testRegion.Production * 0.8f;
            float demand = testRegion.laborAvailable * 1.2f;
            float imbalance = demand - supply;
            Debug.Log($"[Supply/Demand] Supply: {supply}, Demand: {demand}, Imbalance: {imbalance}");
        }

        private void ProcessProduction()
        {
            Debug.Log("Processing Production...");
            float labor = testRegion.laborAvailable;
            float capital = testRegion.infrastructureLevel;

            // Simple Cobb-Douglas production
            float production = productivityFactor 
                * Mathf.Pow(labor, laborElasticity) 
                * Mathf.Pow(capital, capitalElasticity);

            testRegion.Production = Mathf.RoundToInt(production);
            testRegion.Wealth += testRegion.Production;

            Debug.Log($"[Tick] {testRegion.Name} | Labor: {labor}, Capital: {capital}, Production: {testRegion.Production}, Wealth: {testRegion.Wealth}");
        }

        private void ProcessInfrastructure()
        {
            Debug.Log("Processing Infrastructure...");
            float decayRate = 0.01f;
            float maintenanceCost = testRegion.infrastructureLevel * 0.5f;

            testRegion.infrastructureLevel -= (int)(testRegion.infrastructureLevel * decayRate);
            testRegion.Wealth -= Mathf.RoundToInt(maintenanceCost);

            Debug.Log($"[Infrastructure] Level: {testRegion.infrastructureLevel}, Maintenance Cost: {maintenanceCost}");
        }

        private void ProcessPopulationConsumption()
        {
            Debug.Log("Processing Population Consumption...");
            float consumption = testRegion.laborAvailable * 1.5f;
            float unmetDemand = Mathf.Max(0, consumption - testRegion.Production);
            float unrest = unmetDemand / consumption;

            testRegion.satisfaction = Mathf.Clamp01(testRegion.satisfaction - unrest);
            Debug.Log($"[Consumption] Total: {consumption}, Unmet: {unmetDemand}, Satisfaction: {testRegion.satisfaction}");
        }

        private void ProcessEconomicCycle()
        {
            Debug.Log("Processing Economic Cycle...");
            float cycleMultiplier = 1.05f;
            testRegion.Production = Mathf.RoundToInt(testRegion.Production * cycleMultiplier);
            testRegion.Wealth = Mathf.RoundToInt(testRegion.Wealth * cycleMultiplier);
            Debug.Log($"[Cycle] Production: {testRegion.Production}, Wealth: {testRegion.Wealth}");
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