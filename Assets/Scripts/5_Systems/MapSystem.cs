// MapSystem.cs - Fixed
using UnityEngine;

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