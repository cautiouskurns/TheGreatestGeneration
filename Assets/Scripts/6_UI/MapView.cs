using UnityEngine;
using System.Collections.Generic;

public class MapView : MonoBehaviour
{
    public MapDataSO mapData;
    private EconomicSystem economicSystem;
    public GameObject regionPrefab;
    public float regionSpacing = 2.0f;

    private Dictionary<string, GameObject> regionObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, RegionEntity> regionEntities = new Dictionary<string, RegionEntity>();

    private void OnEnable()
    {
        EventBus.Subscribe("EconomicSystemReady", OnEconomicSystemReady);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("EconomicSystemReady", OnEconomicSystemReady);
    }

    private void OnEconomicSystemReady(object system)
    {
        economicSystem = system as EconomicSystem;
        Debug.Log("✅ EconomicSystem registered in MapView.");
        GenerateMap();
    }
    

    private void GenerateMap()
    {
            if (economicSystem == null)
        {
            Debug.LogError("❌ EconomicSystem is not initialized!");
            return;
        }

        int x = 0, y = 0;

        foreach (NationDataSO nation in mapData.nations)
        {
            foreach (RegionDataSO region in nation.regions)
            {
                // ✅ Create RegionEntity (Previously in MapManager)
                RegionEntity regionEntity = new RegionEntity(region.regionName, region.initialWealth, region.initialProduction);
                regionEntities[region.regionName] = regionEntity;
                
                // ✅ Register the region with EconomicSystem
                economicSystem.RegisterRegion(regionEntity);

                // ✅ Create Visual Representation
                GameObject regionGO = Instantiate(regionPrefab, new Vector3(x * regionSpacing, y * regionSpacing, 0), Quaternion.identity, transform);
                regionGO.name = region.regionName;
                regionGO.GetComponent<SpriteRenderer>().color = nation.nationColor;

                // ✅ Scale up if too small
                regionGO.transform.localScale = new Vector3(2f, 2f, 1f);

                // ✅ Store in dictionary for easy updates
                regionObjects[region.regionName] = regionGO;

                x++; 
                if (x > 5)
                {
                    x = 0;
                    y++;
                }
            }
        }
    }

    // ✅ Updates the color of a region dynamically
    public void UpdateRegionVisual(string regionName, Color newColor)
    {
        if (regionObjects.ContainsKey(regionName))
        {
            regionObjects[regionName].GetComponent<SpriteRenderer>().color = newColor;
        }
        else
        {
            Debug.LogWarning("Region not found: " + regionName);
        }
    }
}

