// MapController.cs - Aligned with EventBus
using UnityEngine;
using TMPro;

public class MapController : MonoBehaviour
{
    public MapView mapView;
    public TextMeshProUGUI infoText;
    private GameManager gameManager;

    private string selectedRegion = "";

    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
        }
    }

    private void OnEnable()
    {
        // Listen for RegionClicked events from RegionClickHandler
        EventBus.Subscribe("RegionClicked", OnRegionClicked);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("RegionClicked", OnRegionClicked);
    }

    private void OnRegionClicked(object regionNameObj)
    {
        string regionName = (string)regionNameObj;
        Debug.Log($"MapController received RegionClicked for: {regionName}");
        
        // Pass to GameManager which handles the model update
        if (gameManager != null)
        {
            gameManager.SelectRegion(regionName);
        }
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