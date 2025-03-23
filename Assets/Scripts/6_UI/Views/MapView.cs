// MapView.cs - Updated version
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class MapView : MonoBehaviour
{
    [Header("Map Data")]
    public MapDataSO mapData;
    
    [Header("Prefabs")]
    public GameObject regionPrefab;
    
    [Header("Visualization Settings")]
    public float regionSpacing = 2.0f;
    public bool showTerrainColors = true;
    public float terrainColorBlend = 0.7f; // How much terrain color influences the final color
    
    [Header("Terrain Types")]
    public TerrainTypeDataSO[] availableTerrainTypes;

    private Dictionary<string, GameObject> regionObjects = new Dictionary<string, GameObject>();
    private Dictionary<string, TerrainTypeDataSO> terrainTypeDict = new Dictionary<string, TerrainTypeDataSO>();
    
    // Cache terrain maps for regions for more efficient lookups
    private Dictionary<string, TerrainTypeDataSO> regionTerrainMap = new Dictionary<string, TerrainTypeDataSO>();

    void Awake()
    {
        // Initialize terrain type dictionary
        if (availableTerrainTypes != null && availableTerrainTypes.Length > 0)
        {
            foreach (var terrain in availableTerrainTypes)
            {
                if (terrain != null)
                {
                    terrainTypeDict[terrain.terrainName] = terrain;
                }
            }
        }
    }

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

    private void OnEnable()
    {
        // Add event subscriptions
        EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Subscribe("RegionSelected", OnRegionSelected);
    }

    private void OnDisable()
    {
        // Add event unsubscriptions
        EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Unsubscribe("RegionSelected", OnRegionSelected);
    }

    // Add this handler for RegionSelected events
    private void OnRegionSelected(object regionObj)
    {
        RegionEntity region = regionObj as RegionEntity;
        if (region != null)
        {
            // Reset all selections
            foreach (var regionGO in regionObjects.Values)
            {
                string regionName = regionGO.name;
                RegionEntity regionEntity = FindFirstObjectByType<GameManager>().GetRegion(regionName);
                if (regionEntity != null && regionEntity != region)
                {
                    // Restore original color based on terrain and nation
                    UpdateRegionVisualColors(regionEntity);
                }
            }

            // Highlight selected region
            if (regionObjects.ContainsKey(region.regionName))
            {
                // Apply a highlight color or effect
                regionObjects[region.regionName].GetComponent<SpriteRenderer>().color = Color.yellow;
            }
        }
    }

    private void OnRegionUpdated(object regionObj)
    {
        RegionEntity region = regionObj as RegionEntity;
        if (region != null)
        {
            // Update visual
            UpdateRegionVisual(region);
            
            // Show economy changes if applicable
            ShowEconomyChanges(region);
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
                
                // Apply initial color based on nation
                Color regionColor = nation.nationColor;
                
                // Check if we have a terrain type assigned to this region
                // This would be part of your extended RegionData class in MapDataSO
                // For now, we'll just randomly assign a terrain type for visualization
                if (showTerrainColors && availableTerrainTypes.Length > 0)
                {
                    // Randomly assign a terrain type for now
                    TerrainTypeDataSO terrain = availableTerrainTypes[Random.Range(0, availableTerrainTypes.Length)];
                    regionTerrainMap[region.regionName] = terrain;
                    
                    // Blend nation color with terrain color
                    regionColor = Color.Lerp(nation.nationColor, terrain.baseColor, terrainColorBlend);
                }
                
                regionGO.GetComponent<SpriteRenderer>().color = regionColor;
                
                // Add a component to handle clicks
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

    public void UpdateRegionVisual(RegionEntity region)
    {
        if (regionObjects.ContainsKey(region.regionName))
        {
            // Update visuals based on region data
            UpdateRegionVisualColors(region);
        }
    }
    
    private void UpdateRegionVisualColors(RegionEntity region)
    {
        if (!regionObjects.ContainsKey(region.regionName))
            return;
            
        GameObject regionGO = regionObjects[region.regionName];
        SpriteRenderer sr = regionGO.GetComponent<SpriteRenderer>();
        
        // Start with nation color
        Color finalColor = region.regionColor;
        
        // Blend with terrain color if available
        if (showTerrainColors && regionTerrainMap.ContainsKey(region.regionName))
        {
            TerrainTypeDataSO terrain = regionTerrainMap[region.regionName];
            finalColor = Color.Lerp(region.regionColor, terrain.baseColor, terrainColorBlend);
        }
        
        // Apply the color
        sr.color = finalColor;
    }

    // Show economy changes with visual effects
    public void ShowEconomyChanges(RegionEntity region)
    {
        if (!region.hasChangedThisTurn) return;
        
        if (regionObjects.ContainsKey(region.regionName))
        {
            GameObject regionObj = regionObjects[region.regionName];
            
            // Create floating text to show changes
            GameObject changeText = new GameObject("ChangeText");
            changeText.transform.SetParent(regionObj.transform);
            changeText.transform.localPosition = Vector3.up * 0.5f;
            
            TextMeshPro tmp = changeText.AddComponent<TextMeshPro>();
            tmp.text = (region.wealthDelta >= 0 ? "+" : "") + region.wealthDelta + "";
            tmp.fontSize = 2;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = region.wealthDelta >= 0 ? Color.green : Color.red;
            
            // Animate text floating up and fading
            StartCoroutine(AnimateChangeText(changeText));
            
            // Also pulse the region color briefly
            StartCoroutine(PulseRegion(regionObj, region.wealthDelta >= 0));
            
            // Reset change flags
            region.ResetChangeFlags();
        }
    }

    private IEnumerator AnimateChangeText(GameObject textObj)
    {
        TextMeshPro tmp = textObj.GetComponent<TextMeshPro>();
        float duration = 1.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            textObj.transform.localPosition = Vector3.up * (0.5f + t * 0.5f);
            tmp.alpha = 1 - t;
            
            yield return null;
        }
        
        Destroy(textObj);
    }

    private IEnumerator PulseRegion(GameObject regionObj, bool positive)
    {
        SpriteRenderer sr = regionObj.GetComponent<SpriteRenderer>();
        Color originalColor = sr.color;
        Color pulseColor = positive ? Color.green : Color.red;
        
        float duration = 0.5f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float pulse = Mathf.Sin(t * Mathf.PI);
            
            sr.color = Color.Lerp(originalColor, pulseColor, pulse * 0.5f);
            
            yield return null;
        }
        
        sr.color = originalColor;
    }
    
    // Method to assign terrain to region for testing
    public void AssignTerrainToRegion(string regionName, TerrainTypeDataSO terrain)
    {
        if (terrain != null)
        {
            regionTerrainMap[regionName] = terrain;
            
            // Update visuals if region exists
            if (regionObjects.ContainsKey(regionName))
            {
                RegionEntity region = FindFirstObjectByType<GameManager>().GetRegion(regionName);
                if (region != null)
                {
                    UpdateRegionVisualColors(region);
                }
            }
        }
    }
}