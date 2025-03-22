using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the visual representation of the map and regions.
/// This class serves as the "View" in the MVC architecture.
/// </summary>
public class MapView : MonoBehaviour
{
    public MapDataSO mapData;
    public GameObject regionPrefab;
    public float regionSpacing = 2.0f;
    
    // Dictionary to store visual region GameObjects for easy access
    private Dictionary<string, GameObject> regionObjects = new Dictionary<string, GameObject>();
    
    private void OnEnable()
    {
        // Subscribe to events
        EventBus.Subscribe("RegionEntitiesReady", OnRegionsReady);
        EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Subscribe("RegionSelected", OnRegionSelected);
    }
    
    private void OnDisable()
    {
        // Unsubscribe from events
        EventBus.Unsubscribe("RegionEntitiesReady", OnRegionsReady);
        EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
    }
    
    private void OnRegionsReady(object regionsObj)
    {
        // Generate the visual map once regions are initialized
        Dictionary<string, RegionEntity> regions = regionsObj as Dictionary<string, RegionEntity>;
        GenerateMapVisuals(regions);
    }
    
    private void GenerateMapVisuals(Dictionary<string, RegionEntity> regions)
    {
        int x = 0, y = 0;
        
        foreach (NationDataSO nation in mapData.nations)
        {
            foreach (RegionDataSO regionData in nation.regions)
            {
                // Create visual representation
                GameObject regionGO = Instantiate(regionPrefab, new Vector3(x * regionSpacing, y * regionSpacing, 0), Quaternion.identity, transform);
                regionGO.name = regionData.regionName;
                regionGO.GetComponent<SpriteRenderer>().color = nation.nationColor;
                
                // Add region name as a tag for identification
                regionGO.tag = "Region";
                
                // Store for later reference
                regionObjects[regionData.regionName] = regionGO;
                
                // Adjust position for next region
                x++;
                if (x > 5)
                {
                    x = 0;
                    y++;
                }
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
            // Update visual based on region state
            Color updatedColor = CalculateRegionColor(region);
            regionObjects[region.regionName].GetComponent<SpriteRenderer>().color = updatedColor;
        }
    }
    
    private Color CalculateRegionColor(RegionEntity region)
    {
        // Example logic for color calculation based on region properties
        // This could be expanded to reflect dominant sector, prosperity, etc.
        float prosperity = Mathf.Clamp01(region.wealth / 1000f);
        return new Color(0.5f, 0.5f + prosperity * 0.5f, 0.5f);
    }
    
    public void HighlightSelectedRegion(string regionName)
    {
        // Reset all regions to their base appearance
        foreach (var kvp in regionObjects)
        {
            // Apply normal appearance
            UpdateRegionVisual(GetRegionEntityByName(kvp.Key));
        }
        
        // Highlight the selected region
        if (regionObjects.ContainsKey(regionName))
        {
            regionObjects[regionName].GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }
    
    // Helper method to get a RegionEntity by name (this would normally come from the controller)
    private RegionEntity GetRegionEntityByName(string regionName)
    {
        // This is a placeholder - in the full implementation the controller would manage this
        RegionEntity result = null;
        EventBus.Trigger("RequestRegionEntity", new RegionRequest(regionName, (entity) => result = entity));
        return result;
    }
    
    // Helper class for region requests
    public class RegionRequest
    {
        public string regionName;
        public System.Action<RegionEntity> callback;
        
        public RegionRequest(string name, System.Action<RegionEntity> callback)
        {
            this.regionName = name;
            this.callback = callback;
        }
    }
}
