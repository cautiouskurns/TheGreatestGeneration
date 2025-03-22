using UnityEngine;
using System.Collections.Generic;

public class MapView : MonoBehaviour
{
    public MapDataSO mapData;
    public GameObject regionPrefab;
    public float regionSpacing = 2.0f;

    private Dictionary<string, GameObject> regionObjects = new Dictionary<string, GameObject>();

    private void OnEnable()
    {
        EventBus.Subscribe("RegionEntitiesReady", OnRegionsReady);
        EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Subscribe("RegionSelected", OnRegionSelected);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("RegionEntitiesReady", OnRegionsReady);
        EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
    }

    private void OnRegionsReady(object regionsObj)
    {
        Dictionary<string, RegionEntity> regions = regionsObj as Dictionary<string, RegionEntity>;
        GenerateMapVisuals(regions);
    }

    private void GenerateMapVisuals(Dictionary<string, RegionEntity> regions)
    {
        int x = 0, y = 0;

        foreach (var region in regions.Values)
        {
            GameObject regionGO = Instantiate(regionPrefab, new Vector3(x * regionSpacing, y * regionSpacing, 0), Quaternion.identity, transform);
            regionGO.name = region.regionName;
            regionGO.GetComponent<SpriteRenderer>().color = region.regionColor;
            regionGO.tag = "Region";

            regionObjects[region.regionName] = regionGO;

            x++;
            if (x > 5)
            {
                x = 0;
                y++;
            }
        }

        Debug.Log("Map visuals generated with " + regionObjects.Count + " regions");
    }

    private void OnRegionUpdated(object regionObj)
    {
        RegionEntity region = regionObj as RegionEntity;
        UpdateRegionVisual(region);
    }

    private void OnRegionSelected(object regionObj)
    {
        RegionEntity region = regionObj as RegionEntity;
        HighlightSelectedRegion(region.regionName);
    }

    public void UpdateRegionVisual(RegionEntity region)
    {
        if (regionObjects.ContainsKey(region.regionName))
        {
            // Update sprite to reflect region's wealth
            float prosperity = Mathf.Clamp01(region.wealth / 500f);
            Color baseColor = region.regionColor;
            Color brightColor = new Color(
                Mathf.Min(1f, baseColor.r + 0.2f), 
                Mathf.Min(1f, baseColor.g + 0.2f), 
                Mathf.Min(1f, baseColor.b + 0.2f)
            );
            regionObjects[region.regionName].GetComponent<SpriteRenderer>().color = 
                Color.Lerp(baseColor, brightColor, prosperity);
        }
    }

    public void HighlightSelectedRegion(string regionName)
    {
        // Reset all regions to their base appearance
        foreach (var kvp in regionObjects)
        {
            // Reset highlight
            string name = kvp.Key;
            RegionEntity region = GameObject.Find("GameManager").GetComponent<GameManager>().GetRegion(name);
            if (region != null)
            {
                UpdateRegionVisual(region);
            }
        }

        // Highlight the selected region
        if (regionObjects.ContainsKey(regionName))
        {
            regionObjects[regionName].GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }
}