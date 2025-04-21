using System.Collections.Generic;
using UnityEngine;
using V2.Entities;
using V2.Systems;

namespace V2.Managers
{
    public class RegionManager : MonoBehaviour
    {
        [SerializeField] private GameObject regionPrefab;
        [SerializeField] private Transform mapContainer;
        
        private Dictionary<string, RegionView> regionViews = new Dictionary<string, RegionView>();
        private EconomicSystem economicSystem;
        private V2.Systems.DialogueSystem.DialogueEventManager dialogueManager;
        
        private void Awake()
        {
            economicSystem = FindFirstObjectByType<EconomicSystem>();
            dialogueManager = FindFirstObjectByType<V2.Systems.DialogueSystem.DialogueEventManager>();
        }
        
        private void Start()
        {
            InitializeRegions();
        }
        
        private void InitializeRegions()
        {
            // Create basic regions with positions and colors
            CreateRegion("Western Province", new Vector2(-300, 0), Color.blue);
            CreateRegion("Eastern Province", new Vector2(300, 0), Color.red);
            CreateRegion("Northern Highlands", new Vector2(0, 300), Color.green);
            CreateRegion("Southern Coast", new Vector2(0, -300), Color.yellow);
            
            // Initialize economic entities for each region
            foreach (var region in regionViews.Values)
            {
                RegionEntity regionEntity;
                
                // Use test region for the first one if it already exists
                if (region == regionViews["Western Province"] && economicSystem.testRegion != null)
                {
                    regionEntity = economicSystem.testRegion;
                    regionEntity.Name = "Western Province";
                }
                else
                {
                    // Create a new region entity with random initial values
                    int initialWealth = Random.Range(100, 300);
                    int initialProduction = Random.Range(50, 100);
                    regionEntity = new RegionEntity(region.RegionName, initialWealth, initialProduction);
                    
                    // Set additional properties
                    regionEntity.Population.LaborAvailable = Random.Range(50, 150);
                    regionEntity.Infrastructure.Level = Random.Range(1, 5);
                    regionEntity.Population.UpdateSatisfaction(Random.Range(0.4f, 0.8f));
                }
                
                region.SetRegionEntity(regionEntity);
            }
        }
        
        private void CreateRegion(string name, Vector2 position, Color color)
        {
            GameObject regionObj = Instantiate(regionPrefab, mapContainer);
            regionObj.name = name;
            regionObj.transform.localPosition = position;
            
            RegionView regionView = regionObj.GetComponent<RegionView>();
            if (regionView != null)
            {
                regionView.Initialize(name, color);
                regionViews.Add(name, regionView);
            }
        }
        
        public void SelectRegion(string regionName)
        {
            foreach (var region in regionViews.Values)
            {
                region.SetSelected(region.RegionName == regionName);
            }
            
            // Find the selected region
            if (regionViews.TryGetValue(regionName, out RegionView selectedView))
            {
                // Update the economic system's test region
                economicSystem.testRegion = selectedView.RegionEntity;
                
                // Trigger a region event if possible
                TriggerRegionEvent(selectedView.RegionEntity);
                
                Debug.Log($"Selected region: {regionName}");
            }
        }
        
        private void TriggerRegionEvent(RegionEntity region)
        {
            if (dialogueManager != null)
            {
                // Try to find a region-specific event
                string eventId = $"{region.Name.Replace(" ", "_").ToLower()}_event";
                
                // If we don't have a region-specific event, use a generic one
                if (!HasEvent(eventId))
                {
                    if (region.Economy.Wealth < 150)
                        eventId = "resource_shortage";
                    else
                        eventId = "economic_reform_proposal";
                }
                
                dialogueManager.TriggerEvent(eventId);
            }
        }
        
        private bool HasEvent(string eventId)
        {
            // Simple check - this would need to be expanded in a real implementation
            return eventId == "resource_shortage" || eventId == "economic_reform_proposal";
        }
    }
}