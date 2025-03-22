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

        // If wealth is high, color green. If low, color red.
        Color highlightColor = Color.yellow;
        mapView.UpdateRegionVisual(region.regionName, highlightColor);
    }

    private void OnRegionSelected(object regionNameObj)
    {
        string regionName = (string)regionNameObj;
        Debug.Log($"Region selected in MapSystem: {regionName}");
        
        // Change the color based on selection
        Color selectedColor = Color.cyan; // Or any highlight color
        mapView.UpdateRegionVisual(regionName, selectedColor);
    }
}
