using System.Collections.Generic;
using UnityEngine;
using V2.Entities;
using V2.Systems;

namespace V2.Managers
{
    public class RegionManager : MonoBehaviour
    {
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
            // Subscribe to events
            EventBus.Subscribe("EconomicTick", OnEconomicTick);
            EventBus.Subscribe("RegionSelected", OnRegionSelected);
            EventBus.Subscribe("RegionsCreated", OnRegionsCreated);
            
            // Give MapManager a chance to create regions first
            Invoke("InitializeRegionReferences", 0.2f);
        }
        
        private void OnDestroy()
        {
            EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
            EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
            EventBus.Unsubscribe("RegionsCreated", OnRegionsCreated);
        }
        
        private void OnRegionsCreated(object data)
        {
            // When MapManager signals that it's created regions, initialize our references immediately
            CancelInvoke("InitializeRegionReferences");
            InitializeRegionReferences();
        }
        
        private void OnEconomicTick(object data)
        {
            // The EconomicSystem now triggers the RegionUpdated events directly
            // No need to do anything here
        }
        
        // This method finds all RegionView components in the scene and initializes them
        private void InitializeRegionReferences()
        {
            // Find all RegionView components in the scene
            RegionView[] regions = FindObjectsByType<RegionView>(FindObjectsSortMode.None);
            
            // Populate our dictionary
            foreach (var region in regions)
            {
                if (!string.IsNullOrEmpty(region.RegionName))
                {
                    regionViews[region.RegionName] = region;
                }
            }
            
            Debug.Log($"RegionManager: Found {regionViews.Count} regions to manage");
            
            // If we have regions but no testRegion is set, use the first one
            if (regionViews.Count > 0 && economicSystem.testRegion == null)
            {
                economicSystem.testRegion = regionViews.Values.GetEnumerator().Current.RegionEntity;
                if (economicSystem.testRegion != null)
                {
                    Debug.Log($"Set {economicSystem.testRegion.Name} as initial test region");
                }
            }
            
            // Manually trigger an economic tick to update all values
            if (economicSystem != null)
            {
                economicSystem.ManualTick();
            }
        }
        
        private void OnRegionSelected(object data)
        {
            if (data is string regionName && regionViews.TryGetValue(regionName, out RegionView selectedView))
            {
                SelectRegion(regionName);
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
                if (selectedView.RegionEntity != null)
                {
                    economicSystem.testRegion = selectedView.RegionEntity;
                }
                
                // Trigger a region event if possible
                if (selectedView.RegionEntity != null)
                {
                    TriggerRegionEvent(selectedView.RegionEntity);
                }
                
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