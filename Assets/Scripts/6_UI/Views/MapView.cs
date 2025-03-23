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
        if (mapData == null || mapData.nations == null)
        {
            Debug.LogError("MapData or nations array is null!");
            return;
        }

        // First, determine if position data is available in the RegionData
        bool usePositionData = false;
        
        // Check first region for position data (not at origin)
        foreach (var nation in mapData.nations)
        {
            if (nation.regions != null && nation.regions.Length > 0)
            {
                // If position is not (0,0), assume it's valid position data
                if (nation.regions[0].position != Vector2.zero)
                {
                    usePositionData = true;
                    break;
                }
            }
        }
        
        if (usePositionData)
        {
            // Calculate the map bounds to center it
            float minX = float.MaxValue, maxX = float.MinValue;
            float minY = float.MaxValue, maxY = float.MinValue;
            
            // Find the extent of the map
            foreach (var nation in mapData.nations)
            {
                foreach (var region in nation.regions)
                {
                    minX = Mathf.Min(minX, region.position.x);
                    maxX = Mathf.Max(maxX, region.position.x);
                    minY = Mathf.Min(minY, region.position.y);
                    maxY = Mathf.Max(maxY, region.position.y);
                }
            }
            
            // Calculate the center of the map in grid coordinates
            float centerX = (minX + maxX) / 2f;
            float centerY = (minY + maxY) / 2f;
            
            // Now create regions with offset to center
            foreach (var nation in mapData.nations)
            {
                foreach (var region in nation.regions)
                {
                    // Calculate position with offset to center the map
                    Vector3 position = new Vector3(
                        (region.position.x - centerX) * regionSpacing,
                        (region.position.y - centerY) * regionSpacing,
                        0
                    );
                    
                    CreateRegionGameObject(nation, region, position);
                }
            }
            
            Debug.Log($"Generated centered map with bounds: ({minX},{minY}) to ({maxX},{maxY})");
        }
        else
        {
            // Fall back to grid layout if position data is not available
            // Calculate grid dimensions
            int totalRegions = 0;
            foreach (var nation in mapData.nations)
            {
                totalRegions += nation.regions.Length;
            }
            
            int gridWidth = Mathf.CeilToInt(Mathf.Sqrt(totalRegions));
            int gridHeight = Mathf.CeilToInt((float)totalRegions / gridWidth);
            
            // Calculate center offset
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
                    
                    CreateRegionGameObject(nation, region, position);
                    
                    // Move to next grid position
                    x++;
                    if (x >= gridWidth)
                    {
                        x = 0;
                        y++;
                    }
                }
            }
            
            Debug.Log($"Generated grid map with {regionObjects.Count} regions in a {gridWidth}x{gridHeight} grid");
        }
    }

    // Helper method to create region game objects
    private void CreateRegionGameObject(MapDataSO.NationData nation, MapDataSO.RegionData region, Vector3 position)
    {
        GameObject regionGO = Instantiate(regionPrefab, position, Quaternion.identity, transform);
        regionGO.name = region.regionName;
        
        // Start with just the terrain color
        Color regionColor = Color.gray; // Default fallback color
        
        // Use the terrain type from region data if available
        if (showTerrainColors && !string.IsNullOrEmpty(region.terrainTypeName))
        {
            TerrainTypeDataSO terrain = null;
            
            // Try to find the terrain type by name
            if (terrainTypeDict.ContainsKey(region.terrainTypeName))
            {
                terrain = terrainTypeDict[region.terrainTypeName];
            }
            else if (availableTerrainTypes.Length > 0)
            {
                // Fallback to random if not found
                terrain = availableTerrainTypes[Random.Range(0, availableTerrainTypes.Length)];
                Debug.LogWarning($"Terrain type '{region.terrainTypeName}' not found for region {region.regionName}, using random terrain");
            }
            
            if (terrain != null)
            {
                regionTerrainMap[region.regionName] = terrain;
                regionColor = terrain.baseColor;
            }
        }
        else
        {
            // If not showing terrain colors, use nation color
            regionColor = nation.nationColor;
        }
        
        // Apply the color to main sprite
        SpriteRenderer sr = regionGO.GetComponent<SpriteRenderer>();
        sr.color = regionColor;
        
        // Create a border to show nation ownership
        AddNationBorder(regionGO, nation.nationColor);
        
        // Add a component to handle clicks
        RegionClickHandler clickHandler = regionGO.AddComponent<RegionClickHandler>();
        clickHandler.regionName = region.regionName;

        regionObjects[region.regionName] = regionGO;
    }

    // Method to add a colored border to represent nation ownership
    private void AddNationBorder(GameObject regionGO, Color nationColor)
    {
        // Create a child GameObject for the border
        GameObject borderObj = new GameObject("NationBorder");
        borderObj.transform.SetParent(regionGO.transform);
        borderObj.transform.localPosition = Vector3.zero;
        
        // Add LineRenderer component for the border
        LineRenderer border = borderObj.AddComponent<LineRenderer>();
        border.useWorldSpace = false;
        border.positionCount = 5; // Square (4 corners + connect back to start)
        border.startWidth = 0.1f;
        border.endWidth = 0.1f;
        border.loop = true;
        
        // Set material and color
        border.material = new Material(Shader.Find("Sprites/Default"));
        border.startColor = nationColor;
        border.endColor = nationColor;
        
        // Get the size from the parent's sprite
        SpriteRenderer parentSprite = regionGO.GetComponent<SpriteRenderer>();
        float width = parentSprite.bounds.size.x * 0.9f;
        float height = parentSprite.bounds.size.y * 0.9f;
        
        // Position the border slightly in front of the sprite
        float z = -0.1f;
        
        // Set the positions to form a rectangle
        border.SetPosition(0, new Vector3(-width/2, -height/2, z));
        border.SetPosition(1, new Vector3(-width/2, height/2, z));
        border.SetPosition(2, new Vector3(width/2, height/2, z));
        border.SetPosition(3, new Vector3(width/2, -height/2, z));
        border.SetPosition(4, new Vector3(-width/2, -height/2, z)); // Close the loop
    }

    public void HighlightRegion(string regionName)
    {
        if (regionObjects.ContainsKey(regionName))
        {
            GameObject regionObj = regionObjects[regionName];
            LineRenderer border = regionObj.GetComponentInChildren<LineRenderer>();
            
            if (border != null)
            {
                // Save original color and width for later restoration
                if (!regionObj.TryGetComponent<HighlightData>(out var highlightData))
                {
                    highlightData = regionObj.AddComponent<HighlightData>();
                    highlightData.originalColor = border.startColor;
                    highlightData.originalWidth = border.startWidth;
                }
                
                // Apply highlight
                border.startColor = Color.yellow;
                border.endColor = Color.yellow;
                border.startWidth = 0.05f; // Thicker highlight
                border.endWidth = 0.05f;
            }
        }
    }

    public void ResetHighlight(string regionName, Color originalColor)
    {
        if (regionObjects.ContainsKey(regionName))
        {
            GameObject regionObj = regionObjects[regionName];
            LineRenderer border = regionObj.GetComponentInChildren<LineRenderer>();
            HighlightData highlightData = regionObj.GetComponent<HighlightData>();
            
            if (border != null && highlightData != null)
            {
                // Restore original border appearance
                border.startColor = highlightData.originalColor;
                border.endColor = highlightData.originalColor;
                border.startWidth = highlightData.originalWidth;
                border.endWidth = highlightData.originalWidth;
            }
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
        
        // Start with terrain color if available
        Color mainColor = region.regionColor;
        if (showTerrainColors && regionTerrainMap.ContainsKey(region.regionName))
        {
            TerrainTypeDataSO terrain = regionTerrainMap[region.regionName];
            mainColor = terrain.baseColor;
        }
        
        // Apply the terrain color to the main sprite
        sr.color = mainColor;
        
        // Update the border with the nation color
        LineRenderer border = regionGO.GetComponentInChildren<LineRenderer>();
        if (border != null)
        {
            border.startColor = region.regionColor;
            border.endColor = region.regionColor;
            
            // Also update the stored original color in case this is highlighted
            HighlightData highlightData = regionGO.GetComponent<HighlightData>();
            if (highlightData != null)
            {
                highlightData.originalColor = region.regionColor;
            }
        }
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

    // Helper class to store original border appearance
    private class HighlightData : MonoBehaviour
    {
        public Color originalColor;
        public float originalWidth;
    }
}