using UnityEngine;
using System.Collections.Generic;

public class MapView : MonoBehaviour
{
    public MapDataSO mapData;  // Reference to the ScriptableObject storing map info.
    public GameObject regionPrefab; // Prefab for visual representation of a region.
    
    private Dictionary<string, GameObject> regionObjects = new Dictionary<string, GameObject>();

    void Start()
    {
        GenerateMap();
    }

    private void GenerateMap()
    {
        foreach (NationDataSO nation in mapData.nations)
        {
            foreach (RegionDataSO region in nation.regions)
            {
                GameObject regionGO = Instantiate(regionPrefab, transform);
                regionGO.name = region.regionName;
                regionGO.GetComponent<SpriteRenderer>().color = nation.nationColor;
                
                // Store in dictionary for easy updates
                regionObjects[region.regionName] = regionGO;
            }
        }
    }

    public void UpdateRegionVisual(string regionName, Color newColor)
    {
        if (regionObjects.ContainsKey(regionName))
        {
            regionObjects[regionName].GetComponent<SpriteRenderer>().color = newColor;
        }
    }
}

