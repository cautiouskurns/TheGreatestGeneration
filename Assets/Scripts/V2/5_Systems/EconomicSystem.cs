using UnityEngine;
using V2.Entities;
using V2.Managers;
using V2.Components;
using System.Collections.Generic;
using V2.Systems.Economic;

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
        
        [Header("Wealth Management")]
        public float wealthGrowthRate = 1.0f;
        
        [Header("Market Settings")]
        public float priceVolatility = 0.1f;
        
        [Header("Infrastructure Settings")]
        public float decayRate = 0.01f;
        public float maintenanceCostMultiplier = 0.5f;
        
        [Header("Population Settings")]
        public float laborConsumptionRate = 1.5f;
        
        // Economic subsystems
        private ProductionCalculator productionCalculator;
        private InfrastructureManager infrastructureManager;
        private ConsumptionManager consumptionManager;
        private MarketBalancer marketBalancer;
        private CycleManager cycleManager;
        private PriceVolatilityManager priceVolatilityManager;
        private WealthManager wealthManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Initialize subsystems
            InitializeSubsystems();
        }
        
        private void InitializeSubsystems()
        {
            productionCalculator = new ProductionCalculator(this);
            infrastructureManager = new InfrastructureManager(this);
            consumptionManager = new ConsumptionManager(this);
            marketBalancer = new MarketBalancer(this);
            cycleManager = new CycleManager(this);
            priceVolatilityManager = new PriceVolatilityManager(this);
            wealthManager = new WealthManager(this);
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

            // Calculate production using the Cobb-Douglas function before processing subsystems
            CalculateAndApplyProduction(testRegion);
            
            // Process each economic aspect using the specialized subsystems
            marketBalancer.Process(testRegion);
            productionCalculator.Process(testRegion);
            infrastructureManager.Process(testRegion);
            consumptionManager.Process(testRegion);
            wealthManager.Process(testRegion);
            cycleManager.Process(testRegion);
            priceVolatilityManager.Process(testRegion);
            
            // Notify listeners that an economic tick has occurred
            // This allows the event system to check for economic parameter thresholds
            EventBus.Trigger("EconomicTick", testRegion);
        }
        
        /// <summary>
        /// Calculates and applies production using the Cobb-Douglas function
        /// </summary>
        private void CalculateAndApplyProduction(RegionEntity region)
        {
            // Get inputs for Cobb-Douglas production function
            float labor = region.Population.LaborAvailable;
            float capital = region.Infrastructure.Level;
            
            // Guard against zero values
            labor = Mathf.Max(1f, labor);
            capital = Mathf.Max(1f, capital);
            
            // Calculate production using Cobb-Douglas: Y = A * L^α * K^β
            float production = productivityFactor * 
                Mathf.Pow(labor, laborElasticity) * 
                Mathf.Pow(capital, capitalElasticity);
                
            int productionOutput = Mathf.RoundToInt(production);
            
            // Update both Production and Economy components
            region.Production.SetOutput(productionOutput);
            region.Economy.Production = productionOutput;
            
            Debug.Log($"[Production] Applied Cobb-Douglas calculation: Labor={labor}, Capital={capital}, " +
                      $"Productivity={productivityFactor}, Output={productionOutput}");
        }
    }
}