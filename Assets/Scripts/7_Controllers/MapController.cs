// MapController.cs - Updated version
using UnityEngine;
using TMPro;

public class MapController : MonoBehaviour
{
    public MapView mapView;
    public TextMeshProUGUI infoText;

    private string selectedRegion = "";

    void OnEnable()
    {
        // Subscribe to region click events
        RegionClickHandler.OnRegionClicked += OnRegionClicked;
    }

    void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        RegionClickHandler.OnRegionClicked -= OnRegionClicked;
    }

    void OnRegionClicked(string regionName)
    {
        SelectRegion(regionName);
    }

    void SelectRegion(string regionName)
    {
        // Reset previous selection
        if (!string.IsNullOrEmpty(selectedRegion))
        {
            Color originalColor = GetRegionNationColor(selectedRegion);
            mapView.ResetHighlight(selectedRegion, originalColor);
        }

        // Set new selection
        selectedRegion = regionName;
        mapView.HighlightRegion(regionName);

        // Update info panel
        MapDataSO.RegionData regionData = GetRegionData(regionName);
        if (regionData != null)
        {
            infoText.text = $"Region: {regionName}\nWealth: {regionData.initialWealth}\nProduction: {regionData.initialProduction}";
        }
    }

    MapDataSO.RegionData GetRegionData(string regionName)
    {
        foreach (var nation in mapView.mapData.nations)
        {
            foreach (var region in nation.regions)
            {
                if (region.regionName == regionName)
                    return region;
            }
        }
        return null;
    }

    Color GetRegionNationColor(string regionName)
    {
        foreach (var nation in mapView.mapData.nations)
        {
            foreach (var region in nation.regions)
            {
                if (region.regionName == regionName)
                    return nation.nationColor;
            }
        }
        return Color.white;
    }
}