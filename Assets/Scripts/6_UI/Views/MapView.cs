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
        GenerateMap();
    }

    void GenerateMap()
    {
        int x = 0, y = 0;

        foreach (var nation in mapData.nations)
        {
            foreach (var region in nation.regions)
            {
                GameObject regionGO = Instantiate(regionPrefab, new Vector3(x * regionSpacing, y * regionSpacing, 0), Quaternion.identity, transform);
                regionGO.name = region.regionName;
                regionGO.GetComponent<SpriteRenderer>().color = nation.nationColor;
                
                // Add a component to handle clicks instead of using tags
                RegionClickHandler clickHandler = regionGO.AddComponent<RegionClickHandler>();
                clickHandler.regionName = region.regionName;

                regionObjects[region.regionName] = regionGO;

                // Simple grid layout
                x++;
                if (x > 5)
                {
                    x = 0;
                    y++;
                }
            }
        }

        Debug.Log($"Generated map with {regionObjects.Count} regions");
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