// MapView.cs - Updated version
using UnityEngine;
using System.Collections.Generic;

public class MapView : MonoBehaviour
{
    public MapDataSO mapData;
    public GameObject regionPrefab;
    public float regionSpacing = 2.0f;

    private Dictionary<string, GameObject> regionObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        if (mapData != null)
        {
            GenerateMap();
        }
        else
        {
            Debug.LogError("MapData is not assigned to MapView!");
        }
    }

    void GenerateMap()
    {
        // Calculate grid dimensions
        int totalRegions = 0;
        foreach (var nation in mapData.nations)
        {
            totalRegions += nation.regions.Length;
        }
        
        int gridWidth = Mathf.CeilToInt(Mathf.Sqrt(totalRegions));
        int gridHeight = Mathf.CeilToInt((float)totalRegions / gridWidth);
        
        // Calculate center offset (to place first tile at negative coordinates)
        float offsetX = -(gridWidth * regionSpacing) / 2f;
        float offsetY = -(gridHeight * regionSpacing) / 2f;
        
        int x = 0, y = 0;
        
        foreach (var nation in mapData.nations)
        {
            foreach (var region in nation.regions)
            {
                // Position with offset to center
                Vector3 position = new Vector3(
                    offsetX + (x * regionSpacing), 
                    offsetY + (y * regionSpacing), 
                    0
                );
                
                GameObject regionGO = Instantiate(regionPrefab, position, Quaternion.identity, transform);
                regionGO.name = region.regionName;
                regionGO.GetComponent<SpriteRenderer>().color = nation.nationColor;
                
                // Add a component to handle clicks instead of using tags
                RegionClickHandler clickHandler = regionGO.AddComponent<RegionClickHandler>();
                clickHandler.regionName = region.regionName;

                regionObjects[region.regionName] = regionGO;

                // Move to next grid position
                x++;
                if (x >= gridWidth)
                {
                    x = 0;
                    y++;
                }
            }
        }

        Debug.Log($"Generated map with {regionObjects.Count} regions in a {gridWidth}x{gridHeight} grid");
    }

    public void HighlightRegion(string regionName)
    {
        if (regionObjects.ContainsKey(regionName))
        {
            regionObjects[regionName].GetComponent<SpriteRenderer>().color = Color.yellow;
        }
    }

    public void ResetHighlight(string regionName, Color originalColor)
    {
        if (regionObjects.ContainsKey(regionName))
        {
            regionObjects[regionName].GetComponent<SpriteRenderer>().color = originalColor;
        }
    }
}