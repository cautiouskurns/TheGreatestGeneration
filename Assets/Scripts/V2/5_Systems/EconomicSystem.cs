using UnityEngine;
using V2.Entities;
using V2.Managers;
using V2.Components;
using System.Collections.Generic;
using System.Linq;
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
            if (region == null) return;
            
            if (!regions.ContainsKey(region.Name))
            {
                regions.Add(region.Name, region);
                Debug.Log($"Region registered: {region.Name}");
                
                // If testRegion is null, use this region as the test region
                if (testRegion == null)
                {
                    testRegion = region;
                    Debug.Log($"Assigned {region.Name} as test region");
                }
            }
        }

        public void ProcessEconomicTick()
        {
            // To avoid TransientArtifactProvider errors, we'll use a two-step process:
            // First process all regions, then trigger notifications
            
            // Step 1: Process all registered regions
            if (regions.Count > 0)
            {
                List<RegionEntity> processedRegions = new List<RegionEntity>();
                
                foreach (var regionEntry in regions)
                {
                    ProcessRegion(regionEntry.Value);
                    processedRegions.Add(regionEntry.Value);
                }
                
                // Step 2: Trigger notifications using coroutine to spread them over multiple frames
                StartCoroutine(TriggerRegionUpdates(processedRegions));
                return;
            }
            
            // Fall back to testRegion if we have no registered regions
            if (testRegion != null)
            {
                ProcessRegion(testRegion);
                
                // Trigger notification on the next frame
                StartCoroutine(TriggerSingleRegionUpdate(testRegion));
                return;
            }
            
            // If we get here, we have no regions at all
            Debug.LogWarning("No regions available for economic processing");
            
            // Create a default region as a fallback if nothing is available
            if (Application.isPlaying)
            {
                CreateDefaultRegion();
            }
        }

        // Coroutine to spread region updates over multiple frames to avoid artifact provider errors
        private System.Collections.IEnumerator TriggerRegionUpdates(List<RegionEntity> regionsToUpdate)
        {
            // Notify listeners that an economic tick has occurred first
            EventBus.Trigger("EconomicTick", regionsToUpdate.Count);
            
            // Wait one frame
            yield return null;
            
            // Update each region with a short delay between them
            foreach (var region in regionsToUpdate)
            {
                EventBus.Trigger("RegionUpdated", region);
                
                // Small yield to spread out the updates and prevent artifact errors
                yield return new WaitForSeconds(0.03f);
            }
        }

        // Coroutine for a single region update
        private System.Collections.IEnumerator TriggerSingleRegionUpdate(RegionEntity region)
        {
            // Notify listeners that an economic tick has occurred first
            EventBus.Trigger("EconomicTick", region);
            
            // Wait one frame
            yield return null;
            
            // Trigger the region update
            EventBus.Trigger("RegionUpdated", region);
            
            yield return null;
        }

        private void ProcessRegion(RegionEntity region)
        {
            if (region == null) return;
            
            // Calculate production using the Cobb-Douglas function before processing subsystems
            CalculateAndApplyProduction(region);
            
            // Process each economic aspect using the specialized subsystems
            marketBalancer.Process(region);
            productionCalculator.Process(region);
            infrastructureManager.Process(region);
            consumptionManager.Process(region);
            wealthManager.Process(region);
            cycleManager.Process(region);
            priceVolatilityManager.Process(region);
        }
        
        private void CreateDefaultRegion()
        {
            // Create a default region as a fallback
            RegionEntity defaultRegion = new RegionEntity("Default Region", 200, 50);
            defaultRegion.Population.LaborAvailable = 100;
            defaultRegion.Infrastructure.Level = 5;
            defaultRegion.Population.UpdateSatisfaction(0.5f);
            
            // Register the region
            RegisterRegion(defaultRegion);
            
            Debug.Log("Created default region as economic fallback");
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

        void Start()
        {
            // Check if we have any regions at startup
            if (testRegion == null && regions.Count == 0)
            {
                // Try to find a RegionManager and register its regions
                RegionManager regionManager = FindFirstObjectByType<RegionManager>();
                if (regionManager != null)
                {
                    Debug.Log("EconomicSystem: Found RegionManager, waiting for regions to be registered");
                }
                else
                {
                    // Create a default region if no RegionManager exists
                    CreateDefaultRegion();
                }
            }
        }

        public RegionEntity GetRegion(string regionName)
        {
            if (regions.TryGetValue(regionName, out RegionEntity region))
            {
                return region;
            }
            return null;
        }

        public List<RegionEntity> GetAllRegions()
        {
            return regions.Values.ToList();
        }
    }
}