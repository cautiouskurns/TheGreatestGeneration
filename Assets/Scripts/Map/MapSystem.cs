using UnityEngine;

public class MapSystem : MonoBehaviour
{
    public MapView mapView;

    private void OnEnable()
    {
        EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
    }

    private void OnRegionUpdated(object regionObj)
    {
        RegionEntity region = (RegionEntity)regionObj;

        Color newColor = region.wealth > 1000 ? Color.green : Color.red;
        mapView.UpdateRegionVisual(region.regionName, newColor);
    }
}
