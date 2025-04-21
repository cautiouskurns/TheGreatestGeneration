using UnityEngine;

namespace V2.Systems
{
    /// <summary>
    /// Helper component to create and register test regions for economic scenarios
    /// without requiring the full map visualization system.
    /// </summary>
    public class TestRegionCreator : MonoBehaviour
    {
        [Tooltip("Whether to create test regions on start")]
        public bool createTestRegionsOnStart = true;
        
        [Tooltip("Number of test regions to create")]
        public int numberOfRegions = 4;
        
        [Tooltip("Name prefix for test regions")]
        public string regionNamePrefix = "Test Region ";
        
        private EconomicSystem economicSystem;
        
        private void Awake()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
        }
        
        private void Start()
        {
            if (createTestRegionsOnStart && economicSystem != null)
            {
                // Only create test regions if no regions exist yet
                if (economicSystem.GetAllRegions().Count == 0 && economicSystem.testRegion == null)
                {
                    CreateTestRegions();
                }
            }
        }
        
        [ContextMenu("Create Test Regions")]
        public void CreateTestRegions()
        {
            if (economicSystem == null)
            {
                Debug.LogWarning("No EconomicSystem found. Cannot create test regions.");
                return;
            }
            
            for (int i = 0; i < numberOfRegions; i++)
            {
                string regionName = regionNamePrefix + (i + 1);
                
                // Create a new region with scenario-based values
                int initialWealth = Random.Range(100, 300);
                int initialProduction = Random.Range(50, 100);
                
                V2.Entities.RegionEntity region = new V2.Entities.RegionEntity(regionName, initialWealth, initialProduction);
                
                // Randomize region properties to simulate different region types
                region.Population.LaborAvailable = Random.Range(50, 200);
                region.Infrastructure.Level = Random.Range(1, 6);
                region.Population.UpdateSatisfaction(Random.Range(0.3f, 0.9f));
                
                // Add some resources
                // region.Resources.AddResource("Food", Random.Range(10, 50));
                // region.Resources.AddResource("Timber", Random.Range(5, 30));
                // region.Resources.AddResource("Ore", Random.Range(2, 20));
                
                // Register the region with the economic system
                economicSystem.RegisterRegion(region);
                
                Debug.Log($"Created test region: {regionName}");
            }
            
            // Set the first region as the test region if no test region is set
            if (economicSystem.testRegion == null && numberOfRegions > 0)
            {
                economicSystem.testRegion = economicSystem.GetRegion(regionNamePrefix + "1");
            }
        }
    }
}