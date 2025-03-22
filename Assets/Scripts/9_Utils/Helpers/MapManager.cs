// MapManager.cs - Updated version
using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class MapManager : MonoBehaviour
{
    public RegionMapDataSO mapData;
    public GameObject regionPrefab;
    public TextMeshProUGUI infoText;  // Make sure this is assigned in the Inspector

    private Dictionary<string, GameObject> regionObjects = new Dictionary<string, GameObject>();
    private string selectedRegionName = "";

    void Start()
    {
        // Check if text component is assigned
        if (infoText == null)
        {
            Debug.LogError("InfoText is not assigned to MapManager!");
        }
        else
        {
            // Set initial text to verify it's working
            infoText.text = "Click on a region";
        }
        
        GenerateMap();
    }

    void GenerateMap()
    {
        foreach (var region in mapData.regions)
        {
            GameObject regionObj = Instantiate(regionPrefab, region.position, Quaternion.identity, transform);
            regionObj.name = region.regionName;
            regionObj.GetComponent<SpriteRenderer>().color = region.regionColor;
            
            RegionBehavior regionBehavior = regionObj.AddComponent<RegionBehavior>();
            regionBehavior.regionName = region.regionName;
            regionBehavior.mapManager = this;
            
            regionObjects[region.regionName] = regionObj;
            Debug.Log($"Created region: {region.regionName} at position {region.position}");
        }
    }

    public void SelectRegion(string regionName)
    {
        Debug.Log($"Region selected: {regionName}");
        
        // Reset previous selection
        if (!string.IsNullOrEmpty(selectedRegionName) && regionObjects.ContainsKey(selectedRegionName))
        {
            RegionMapDataSO.RegionData region = FindRegionData(selectedRegionName);
            if (region != null)
            {
                regionObjects[selectedRegionName].GetComponent<SpriteRenderer>().color = region.regionColor;
            }
        }

        // Set new selection
        selectedRegionName = regionName;
        if (regionObjects.ContainsKey(regionName))
        {
            regionObjects[regionName].GetComponent<SpriteRenderer>().color = Color.yellow;
            
            // Update info text with more information to make it obvious
            if (infoText != null)
            {
                infoText.text = $"SELECTED REGION: {regionName}";
                Debug.Log($"Updated text to: {infoText.text}");
            }
            else
            {
                Debug.LogError("Cannot update infoText - it is null!");
            }
        }
    }

    private RegionMapDataSO.RegionData FindRegionData(string regionName)
    {
        foreach (var region in mapData.regions)
        {
            if (region.regionName == regionName)
                return region;
        }
        return null;
    }
}