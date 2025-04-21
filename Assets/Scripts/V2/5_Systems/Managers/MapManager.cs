using System.Collections.Generic;
using UnityEngine;
using V2.Managers;
using V2.Entities;
using V2.Systems;

public class MapManager : MonoBehaviour
{
    [Header("Map Settings")]
    [SerializeField] private GameObject regionPrefab;
    [SerializeField] private Transform regionsContainer;
    [SerializeField] private int gridWidth = 3;
    [SerializeField] private int gridHeight = 3;
    [SerializeField] private float regionSpacing = 2.2f;
    
    // Tracking
    private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();
    private string selectedRegionId;
    private EconomicSystem economicSystem;
    
    private void Awake()
    {
        economicSystem = FindFirstObjectByType<EconomicSystem>();
    }
    
    private void Start()
    {
        CreateRegionGrid();
        
        // Notify the economic system that all regions have been created
        if (regionViews.Count > 0 && economicSystem != null)
        {
            Debug.Log($"MapManager created {regionViews.Count} regions");
            // Trigger an event that RegionManager can listen for
            EventBus.Trigger("RegionsCreated", regionViews.Count);
        }
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe("RegionSelected", OnRegionSelected);
        EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
        EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
    }
    
    private void CreateRegionGrid()
    {
        // Hexagonal grid parameters
        float hexWidth = 1.5f * regionSpacing;  // Width between adjacent hexes
        float hexHeight = 1.732f * regionSpacing;  // Height (sqrt(3)) * size
        
        for (int q = -gridWidth/2; q <= gridWidth/2; q++)
        {
            for (int r = -gridHeight/2; r <= gridHeight/2; r++)
            {
                // Skip hexes that would make the grid more rectangular
                if (Mathf.Abs(q + r) > gridWidth/2)
                    continue;
                    
                // Calculate hex position using axial coordinates
                float xPos = hexWidth * (q + r/2f);
                float yPos = hexHeight * (r * 0.75f);
                Vector3 position = new Vector3(xPos, yPos, 0);
                
                // Generate region ID and name
                string regionId = $"Region_{q}_{r}";
                string regionName = $"R{q},{r}";
                
                // Create color based on position (for visual distinction)
                Color regionColor = new Color(
                    0.4f + (q + gridWidth/2f)/gridWidth * 0.6f,
                    0.4f + (r + gridHeight/2f)/gridHeight * 0.6f,
                    0.5f
                );
                
                // Create region
                CreateRegion(regionId, regionName, position, regionColor);
                
                // Create the economic entity for this region right away
                if (economicSystem != null)
                {
                    CreateRegionEntity(regionId);
                }
            }
        }
    }
    
    private void CreateRegionEntity(string regionId)
    {
        // Only create if the region doesn't already have an entity
        RegionEntity existingEntity = economicSystem.GetRegion(regionId);
        if (existingEntity == null)
        {
            // Create a new region entity with random initial values
            int initialWealth = Random.Range(100, 300);
            int initialProduction = Random.Range(50, 100);
            RegionEntity regionEntity = new RegionEntity(regionId, initialWealth, initialProduction);
            
            // Set additional properties
            regionEntity.Population.LaborAvailable = Random.Range(50, 150);
            regionEntity.Infrastructure.Level = Random.Range(1, 5);
            regionEntity.Population.UpdateSatisfaction(Random.Range(0.4f, 0.8f));
            
            // Register with the economic system
            economicSystem.RegisterRegion(regionEntity);
            
            // Update the region view if available
            if (regionViews.TryGetValue(regionId, out RegionView regionView))
            {
                regionView.SetRegionEntity(regionEntity);
            }
        }
    }
    
    private void CreateRegion(string id, string name, Vector3 position, Color color)
    {
        // Instantiate the region prefab
        GameObject regionObj = Instantiate(regionPrefab, position, Quaternion.identity, regionsContainer);
        
        // Get the RegionView component
        RegionView regionView = regionObj.GetComponent<RegionView>();
        
        // Initialize with data
        regionView.Initialize(id, name, color);
        
        // Store reference
        regionViews[id] = regionView;
    }
    
    private void OnRegionSelected(object data)
    {
        if (data is string regionId)
        {
            // Deselect previous region
            if (!string.IsNullOrEmpty(selectedRegionId) && 
                regionViews.TryGetValue(selectedRegionId, out var previousRegion))
            {
                previousRegion.SetHighlighted(false);
            }
            
            // Select new region
            selectedRegionId = regionId;
            if (regionViews.TryGetValue(regionId, out var currentRegion))
            {
                currentRegion.SetHighlighted(true);
                
                // Make this the current test region in the economic system
                if (economicSystem != null && currentRegion.RegionEntity != null)
                {
                    economicSystem.testRegion = currentRegion.RegionEntity;
                    Debug.Log($"Set {regionId} as test region for economic system");
                }
                
                Debug.Log($"Selected region: {regionId}");
            }
        }
    }
    
    private void OnRegionUpdated(object data)
    {
        // Update visuals when region data changes
        if (data is RegionEntity region && 
            regionViews.TryGetValue(region.Name, out var regionView))
        {
            regionView.UpdateStatus(
                region.Economy.Wealth,
                region.Economy.Production,
                region.Population.Satisfaction
            );
        }
    }
}