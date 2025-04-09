using UnityEngine;
using System.Collections.Generic;
using V1.Entities;
using V1.Data;
using V1.UI;
using V1.Managers;

namespace V1.Systems
{
    /// CLASS PURPOSE:
    /// MapSystem acts as a controller that listens for map-related game events and
    /// forwards updates to the visual MapView layer. It synchronizes region updates
    /// and selections between gameplay systems and UI.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Subscribe to and handle region update and selection events
    /// - Pass relevant data to the MapView for visual updates and highlighting
    /// 
    /// KEY COLLABORATORS:
    /// - MapView: Responsible for rendering and updating the map's appearance
    /// - EventBus: Dispatches and receives events such as "RegionUpdated" and "RegionSelected"
    /// - RegionEntity: Supplies data for the region being updated or selected
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Handles both string and RegionEntity types in selection events
    /// - Visual update calls are commented out, suggesting ongoing development or refactor
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Centralize region visual logic into MapView and use commands instead of direct method calls
    /// - Validate region object type more robustly to avoid casting issues
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Support additional event types (e.g., hover, deselect, focus)
    /// - Integrate with camera controls or overlays for visual feedback
    /// - Allow dynamic visual settings (e.g., color schemes, filters) from MapSystem

    public class MapSystem : MonoBehaviour
    {
        public MapView mapView;

        private void OnEnable()
        {
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Subscribe("RegionSelected", OnRegionSelected);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
        }

        private void OnRegionUpdated(object regionObj)
        {
            RegionEntity region = (RegionEntity)regionObj;
            
            // Changed to match the simplified MapView implementation
            // which only takes the RegionEntity as a parameter
        // mapView.UpdateRegionVisual(region);
        }

        private void OnRegionSelected(object regionNameObj)
        {
            if (regionNameObj is string)
            {
                string regionName = (string)regionNameObj;
                Debug.Log($"Region selected in MapSystem: {regionName}");
                //mapView.HighlightSelectedRegion(regionName);
            }
            else if (regionNameObj is RegionEntity)
            {
                RegionEntity region = (RegionEntity)regionNameObj;
                Debug.Log($"Region selected in MapSystem: {region.regionName}");
            // mapView.HighlightSelectedRegion(region.regionName);
            }
        }
    }
}