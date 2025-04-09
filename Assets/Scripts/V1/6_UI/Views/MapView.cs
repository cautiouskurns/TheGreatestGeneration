using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using V1.Data;
using V1.Managers;
using V1.Entities;
using V1.Core;
using V1.Utils;

namespace V1.UI
{  
    // MapView.cs - Updated version
    /// CLASS PURPOSE:
    /// MapView is responsible for visualizing the map layout in the game, creating and
    /// managing region GameObjects, handling UI overlays, and reacting to gameplay events
    /// such as selection and economic changes.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Generate and layout region tiles based on map data (positioned or grid)
    /// - Apply terrain and nation colors, and update visuals based on simulation state
    /// - Handle region highlighting and visual indicators (e.g., pulses, floating text)
    /// - Switch overlay modes (e.g., wealth, population, production) and update visuals
    /// - Respond to region update and selection events from the EventBus
    ///
    /// KEY COLLABORATORS:
    /// - MapDataSO: Provides static configuration of nations and their regions
    /// - RegionEntity: Supplies runtime data for visual updates
    /// - GameManager: Provides access to the map model and region data
    /// - EventBus: Subscribes to "RegionUpdated" and "RegionSelected" events
    /// - TerrainTypeDataSO: Defines visuals and characteristics of terrain types
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses a combination of dictionary lookups and partial class structure
    /// - Supports both position-based and grid-based layout generation
    /// - Modular update methods improve separation of visual concerns
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Separate rendering logic into its own class or service for better testability
    /// - Cache GameManager and MapModel references to reduce repeated lookups
    /// - Convert region prefab access into dependency injection or pooling
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add support for animated overlays or seasonal terrain effects
    /// - Include tooltip, click-and-drag selection, or zoom-based LOD
    /// - Enable runtime editing of region data or overlays via developer tools
    public partial class MapView : MonoBehaviour
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
    //            Debug.LogError("MapData is not assigned to MapView!");
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

        private void OnRegionSelected(object regionObj)
        {
            RegionEntity region = regionObj as RegionEntity;
            if (region != null)
            {
                // Reset all selections
                foreach (var entry in regionObjects)
                {
                    string regionName = entry.Key;
                    GameObject regionParent = entry.Value;
                    
                    // Skip the newly selected region
                    if (regionName == region.regionName)
                        continue;
                        
                    RegionEntity regionEntity = FindFirstObjectByType<GameManager>().GetRegion(regionName);
                    if (regionEntity != null)
                    {
                        // Restore original border color
                        Transform borderTransform = regionParent.transform.Find("Border");
                        if (borderTransform != null)
                        {
                            SpriteRenderer borderRenderer = borderTransform.GetComponent<SpriteRenderer>();
                            borderRenderer.color = regionEntity.regionColor;
                            borderTransform.localScale = new Vector3(1.1f, 1.1f, 1); // Normal border size
                        }
                    }
                }

                // Highlight the selected region
                if (regionObjects.ContainsKey(region.regionName))
                {
                    GameObject regionParent = regionObjects[region.regionName];
                    Transform borderTransform = regionParent.transform.Find("Border");
                    
                    if (borderTransform != null)
                    {
                        SpriteRenderer borderRenderer = borderTransform.GetComponent<SpriteRenderer>();
                        
                        // Store original color if needed for later
                        if (!regionParent.TryGetComponent<HighlightData>(out var highlightData))
                        {
                            highlightData = regionParent.AddComponent<HighlightData>();
                            highlightData.originalBorderColor = borderRenderer.color;
                        }
                        
                        // Change border to yellow and make it slightly larger
                        borderRenderer.color = Color.yellow;
                        borderTransform.localScale = new Vector3(1.15f, 1.15f, 1);
                    }
                }
            }
        }

        // Helper class to store original border color and scale
        private class HighlightData : MonoBehaviour
        {
            public Color originalBorderColor;
            public Vector3 originalBorderScale;
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
                
    //            Debug.Log($"Generated centered map with bounds: ({minX},{minY}) to ({maxX},{maxY})");
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
            // Create parent GameObject to hold both border and terrain sprites
            GameObject regionParent = new GameObject(region.regionName);
            regionParent.transform.position = position;
            regionParent.transform.SetParent(transform);
            
            // 1. Create border sprite (slightly larger, will be behind terrain)
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(regionParent.transform);
            borderObj.transform.localPosition = new Vector3(0, 0, 0.1f);
            
            SpriteRenderer borderRenderer = borderObj.AddComponent<SpriteRenderer>();
            borderRenderer.sprite = regionPrefab.GetComponent<SpriteRenderer>().sprite; // Use same sprite as region
            borderRenderer.color = nation.nationColor; // Nation color for border
            borderRenderer.sortingOrder = 0; // Behind terrain
            borderObj.transform.localScale = new Vector3(1.2f, 1.2f, 1); // 20% larger to create border effect
            
            // 2. Create terrain sprite
            GameObject terrainObj = new GameObject("Terrain");
            terrainObj.transform.SetParent(regionParent.transform);
            terrainObj.transform.localPosition = Vector3.zero;
            
            SpriteRenderer terrainRenderer = terrainObj.AddComponent<SpriteRenderer>();
            terrainRenderer.sprite = regionPrefab.GetComponent<SpriteRenderer>().sprite;
            terrainRenderer.sortingOrder = 1; // In front of border
            
            // Set terrain color
            Color terrainColor = Color.gray; // Default fallback
            
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
                    terrainColor = terrain.baseColor;
                }
            }
            else
            {
                // If not showing terrain colors, use a lighter version of nation color
                terrainColor = Color.Lerp(nation.nationColor, Color.white, 0.3f);
            }
            
            terrainRenderer.color = terrainColor;
            
            // Add BoxCollider2D to the parent for click detection
            BoxCollider2D collider = regionParent.AddComponent<BoxCollider2D>();
            float spriteSize = 1f; // Adjust based on your sprite size
            collider.size = new Vector2(spriteSize, spriteSize);
            
            // Add click handler to parent
            RegionClickHandler clickHandler = regionParent.AddComponent<RegionClickHandler>();
            clickHandler.regionName = region.regionName;
            
            // Store reference to parent object
            regionObjects[region.regionName] = regionParent;
        }

        // Updated highlight method
        public void HighlightRegion(string regionName)
        {
            if (regionObjects.ContainsKey(regionName))
            {
                GameObject regionParent = regionObjects[regionName];
                Transform borderTransform = regionParent.transform.Find("Border");
                
                if (borderTransform != null)
                {
                    GameObject borderObj = borderTransform.gameObject;
                    SpriteRenderer borderRenderer = borderObj.GetComponent<SpriteRenderer>();
                    
                    // Store original border color and scale for later restoration
                    if (!regionParent.TryGetComponent<HighlightData>(out var highlightData))
                    {
                        highlightData = regionParent.AddComponent<HighlightData>();
                        highlightData.originalBorderColor = borderRenderer.color;
                        highlightData.originalBorderScale = borderObj.transform.localScale;
                    }
                    
                    // Set highlight color and make border slightly larger
                    borderRenderer.color = Color.yellow;
                    borderObj.transform.localScale = new Vector3(1.3f, 1.3f, 1); // Larger for highlight
                }
            }
        }

        public void ResetHighlight(string regionName, Color originalColor)
        {
            if (regionObjects.ContainsKey(regionName))
            {
                GameObject regionParent = regionObjects[regionName];
                Transform borderTransform = regionParent.transform.Find("Border");
                
                if (borderTransform != null)
                {
                    SpriteRenderer borderRenderer = borderTransform.GetComponent<SpriteRenderer>();
                    borderRenderer.color = originalColor;
                    borderTransform.localScale = new Vector3(1.1f, 1.1f, 1); // Back to normal size
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
        
        // Updated visual colors method
        // private void UpdateRegionVisualColors(RegionEntity region)
        // {
        //     if (!regionObjects.ContainsKey(region.regionName))
        //         return;
                    
        //     GameObject regionParent = regionObjects[region.regionName];
            
        //     // Update terrain color
        //     Transform terrainTransform = regionParent.transform.Find("Terrain");
        //     Transform borderTransform = regionParent.transform.Find("Border");
            
        //     if (terrainTransform != null && borderTransform != null)
        //     {
        //         SpriteRenderer terrainRenderer = terrainTransform.GetComponent<SpriteRenderer>();
        //         SpriteRenderer borderRenderer = borderTransform.GetComponent<SpriteRenderer>();
                
        //         // Set border color to nation color
        //         borderRenderer.color = region.regionColor;
                
        //         // Set terrain color based on terrain type if available
        //         if (showTerrainColors && regionTerrainMap.ContainsKey(region.regionName))
        //         {
        //             TerrainTypeDataSO terrain = regionTerrainMap[region.regionName];
        //             terrainRenderer.color = terrain.baseColor;
        //         }
        //         else
        //         {
        //             // If not showing terrain colors, use a lighter version of the nation color
        //             terrainRenderer.color = Color.Lerp(region.regionColor, Color.white, 0.3f);
        //         }
                
        //         // Update highlight data if it exists
        //         HighlightData highlightData = regionParent.GetComponent<HighlightData>();
        //         if (highlightData != null)
        //         {
        //             highlightData.originalBorderColor = region.regionColor;
        //         }
        //     }
        // }

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

        // Updated pulse region method for new structure
        private IEnumerator PulseRegion(GameObject regionObj, bool positive)
        {
            Transform terrainTransform = regionObj.transform.Find("Terrain");
            if (terrainTransform == null) yield break;
            
            SpriteRenderer sr = terrainTransform.GetComponent<SpriteRenderer>();
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


    public partial class MapView : MonoBehaviour
    {
        // Add this enum at the top of the file, with your other enums or at class level
        public enum MapOverlayMode
        {
            Default,      // Normal terrain view
            Wealth,       // Color based on region wealth
            Population,   // Color based on population
            Production    // Color based on production
        }

        // Add this field with your other header attributes
        [Header("Overlay Visualization")]
        public MapOverlayMode currentOverlayMode = MapOverlayMode.Default;
        
        private MapModel mapModel;

        // Add this method to your existing methods
        public void SetOverlayMode(MapOverlayMode newMode)
        {
            currentOverlayMode = newMode;
            UpdateAllRegionVisuals();
        }

        // Modify your existing UpdateRegionVisualColors method
        private void UpdateRegionVisualColors(RegionEntity region)
        {
            if (!regionObjects.ContainsKey(region.regionName))
                return;
                    
            GameObject regionParent = regionObjects[region.regionName];
            
            // Update terrain color
            Transform terrainTransform = regionParent.transform.Find("Terrain");
            Transform borderTransform = regionParent.transform.Find("Border");
            
            if (terrainTransform != null && borderTransform != null)
            {
                SpriteRenderer terrainRenderer = terrainTransform.GetComponent<SpriteRenderer>();
                SpriteRenderer borderRenderer = borderTransform.GetComponent<SpriteRenderer>();
                
                // Set border color to nation color
                borderRenderer.color = region.regionColor;
                
                // Determine terrain color based on current overlay mode
                Color terrainColor = GetTerrainColorForOverlayMode(region);
                
                terrainRenderer.color = terrainColor;
            }
        }

        // Add these helper methods
        private Color GetTerrainColorForOverlayMode(RegionEntity region)
        {
            switch (currentOverlayMode)
            {
                case MapOverlayMode.Default:
                    return GetDefaultTerrainColor(region);
                
                case MapOverlayMode.Wealth:
                    return GetWealthColor(region);
                
                case MapOverlayMode.Population:
                    return GetPopulationColor(region);
                
                case MapOverlayMode.Production:
                    return GetProductionColor(region);
                
                default:
                    return GetDefaultTerrainColor(region);
            }
        }

        private Color GetDefaultTerrainColor(RegionEntity region)
        {
            // If terrain type exists, use its base color
            if (region.terrainType != null)
            {
                return region.terrainType.baseColor;
            }
            
            // Fallback to a light version of nation color
            return Color.Lerp(region.regionColor, Color.white, 0.3f);
        }

        private Color GetWealthColor(RegionEntity region)
        {
            // Normalize wealth (assuming 0-1000 range)
            float normalizedWealth = Mathf.Clamp01(region.wealth / 1000f);
            
            // Interpolate between light yellow (low wealth) and dark yellow (high wealth)
            Color lowWealth = new Color(1f, 1f, 0.7f); // Light yellow
            Color highWealth = new Color(1f, 0.8f, 0f); // Dark yellow
            
            return Color.Lerp(lowWealth, highWealth, normalizedWealth);
        }

        private Color GetPopulationColor(RegionEntity region)
        {
            // Normalize population (assuming 0-500 range)
            float normalizedPopulation = Mathf.Clamp01(region.laborAvailable / 500f);
            
            // Interpolate between light green (low population) and dark green (high population)
            Color lowPopulation = new Color(0.8f, 1f, 0.8f); // Light green
            Color highPopulation = new Color(0f, 0.6f, 0f); // Dark green
            
            return Color.Lerp(lowPopulation, highPopulation, normalizedPopulation);
        }

        private Color GetProductionColor(RegionEntity region)
        {
            // Normalize production (assuming 0-100 range)
            float normalizedProduction = Mathf.Clamp01(region.production / 100f);
            
            // Interpolate between light blue (low production) and dark blue (high production)
            Color lowProduction = new Color(0.8f, 0.9f, 1f); // Light blue
            Color highProduction = new Color(0f, 0f, 0.8f); // Dark blue
            
            return Color.Lerp(lowProduction, highProduction, normalizedProduction);
        }

        // Method to update all region visuals (call this when changing overlay mode)
        private void UpdateAllRegionVisuals()
        {
            // Ensure mapModel is available before trying to use it
            if (mapModel == null)
            {
                mapModel = FindFirstObjectByType<GameManager>()?.GetMapModel();
            }

            if (mapModel != null)
            {
                foreach (var regionEntry in mapModel.GetAllRegions())
                {
                    UpdateRegionVisualColors(regionEntry.Value);
                }
            }
        }
    }
}