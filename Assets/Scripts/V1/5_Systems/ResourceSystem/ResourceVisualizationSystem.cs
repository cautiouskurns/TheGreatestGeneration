using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using V1.Entities;
using V1.Managers;

namespace V1.Systems
{   
    /// CLASS PURPOSE:
    /// ScreenSpaceResourceVisualization manages the visual display of resource quantities
    /// over map regions using screen space UI elements. It dynamically updates icons
    /// and quantities based on simulation data.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Subscribe to region update events and display top resources per region
    /// - Create and position icons around region centers with quantity indicators
    /// - Handle user toggling of resource overlay visibility
    ///
    /// KEY COLLABORATORS:
    /// - RegionEntity: Supplies resource data for visualization
    /// - EventBus: Sends "RegionUpdated" and "RegionEntitiesReady" events to trigger updates
    /// - ResourceComponent: Source of resource quantities per region
    /// - Unity Canvas & UI: Used to instantiate and manage icon and text display
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses both prefab-based and dynamically constructed icons
    /// - Organizes all UI under a centralized parent in the screen space canvas
    /// - Positions icons in a circular pattern around region center
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Encapsulate visualizer creation logic into a builder or factory class
    /// - Cache region GameObjects or screen positions to reduce repetitive lookups
    /// - Consider using a pool for icon GameObjects to reduce GC and instantiation cost
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add filtering or customization options for visible resources
    /// - Integrate hover tooltips, animations, or interactivity
    /// - Allow scaling icons by relative quantity or importance
    /// <summary>
    /// Manages the visualization of resources on the map using Screen Space Canvas
    /// </summary>
    public class ScreenSpaceResourceVisualization : MonoBehaviour
    {
        [Header("Visualization Settings")]
        public bool showResourceIcons = true;
        [Range(0.1f, 2f)]
        public float iconSize = 0.3f;
        [Range(10f, 100f)]
        public float iconSpacing = 25f;
        public int maxIconsPerRegion = 3; // Show only top N resources by quantity
        public bool showResourceQuantities = true;
        
        [Header("Resource Icon References")]
        public Sprite defaultResourceIcon;
        public List<ResourceIconMapping> resourceIcons = new List<ResourceIconMapping>();
        
        [Header("UI References")]
        public Canvas mainCanvas; // Reference to your screen space overlay canvas
        public GameObject resourceToggleButton;
        public GameObject iconPrefab; // Prefab for resource icons

        [Header("Icon Size Settings")]
        [Range(10f, 50f)]
        public float baseIconSize = 25f; // Base icon size in pixels
        [Range(8f, 20f)]
        public float quantityFontSize = 10f; // Text size for quantities
        
        // Dictionary to map resource names to their icons
        private Dictionary<string, Sprite> resourceIconMap = new Dictionary<string, Sprite>();
        
        // Dictionary to track resource visualizers by region
        private Dictionary<string, List<GameObject>> regionResourceVisualizers = 
            new Dictionary<string, List<GameObject>>();

        // Main parent object for all resource icons
        private GameObject resourceIconsParent;
        
        private void Awake()
        {
            // Initialize the resource icon mapping
            foreach (var mapping in resourceIcons)
            {
                if (mapping.resourceIcon != null)
                {
                    resourceIconMap[mapping.resourceName] = mapping.resourceIcon;
                }
            }

            // Find main canvas if not set
            if (mainCanvas == null)
            {
                mainCanvas = FindFirstObjectByType<Canvas>();
                if (mainCanvas == null)
                {
                    Debug.LogError("No Canvas found in scene! Please assign a Canvas in the inspector.");
                    enabled = false;
                    return;
                }
            }

            // Create parent object for all resource icons
            resourceIconsParent = new GameObject("ResourceIcons");
            resourceIconsParent.transform.SetParent(mainCanvas.transform, false);
            RectTransform parentRect = resourceIconsParent.AddComponent<RectTransform>();
            parentRect.anchorMin = Vector2.zero;
            parentRect.anchorMax = Vector2.one;
            parentRect.offsetMin = Vector2.zero;
            parentRect.offsetMax = Vector2.zero;
        }
        
        private void OnEnable()
        {
            EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Subscribe("RegionEntitiesReady", OnRegionsReady);
        }
        
        private void OnDisable()
        {
            EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
            EventBus.Unsubscribe("RegionEntitiesReady", OnRegionsReady);
        }
        
        private void Start()
        {
            // Add toggle button functionality if assigned
            if (resourceToggleButton != null)
            {
                Button toggleButton = resourceToggleButton.GetComponent<Button>();
                if (toggleButton != null)
                {
                    toggleButton.onClick.AddListener(ToggleResourceIcons);
                }
            }
        }
        
        /// <summary>
        /// Toggle resource icons visibility
        /// </summary>
        public void ToggleResourceIcons()
        {
            showResourceIcons = !showResourceIcons;
            
            // Update all existing visualizers
            resourceIconsParent.SetActive(showResourceIcons);
        }
        
        /// <summary>
        /// Handle when all regions are ready
        /// </summary>
        private void OnRegionsReady(object regionsObj)
        {
            if (regionsObj is Dictionary<string, RegionEntity> regions)
            {
                // Create visualizers for all regions
                foreach (var region in regions.Values)
                {
                    UpdateResourceVisualizers(region);
                }
            }
        }
        
        /// <summary>
        /// Handle when a region is updated
        /// </summary>
        private void OnRegionUpdated(object regionObj)
        {
            if (regionObj is RegionEntity region)
            {
                UpdateResourceVisualizers(region);
            }
        }
        
        /// <summary>
        /// Update or create resource visualizers for a region
        /// </summary>
        private void UpdateResourceVisualizers(RegionEntity region)
        {
            // Skip if region has no resources component
            if (region.resources == null) return;
            
            // Find the region GameObject
            GameObject regionObj = GameObject.Find(region.regionName);
            if (regionObj == null) return;
            
            // Clear existing visualizers for this region
            ClearVisualizersForRegion(region.regionName);
            
            // Skip if visualization is disabled
            if (!showResourceIcons) return;
            
            // Get all resources for this region
            Dictionary<string, float> resources = region.resources.GetAllResources();
            
            // Sort resources by quantity (descending)
            List<KeyValuePair<string, float>> sortedResources = new List<KeyValuePair<string, float>>(resources);
            sortedResources.Sort((a, b) => b.Value.CompareTo(a.Value));
            
            // Limit to max icons per region
            int count = Mathf.Min(sortedResources.Count, maxIconsPerRegion);
            
            // Create resource container for this region if it doesn't exist
            GameObject regionContainer = new GameObject(region.regionName + "_Resources");
            regionContainer.transform.SetParent(resourceIconsParent.transform, false);
            
            // Create resource visualizers
            List<GameObject> visualizers = new List<GameObject>();
            
            for (int i = 0; i < count; i++)
            {
                string resourceName = sortedResources[i].Key;
                float amount = sortedResources[i].Value;
                
                // Skip resources with very small amounts
                if (amount < 1f) continue;
                
                // Create icon
                GameObject visualizer = CreateResourceVisualizer(resourceName, amount, i, count, regionContainer.transform, regionObj.transform.position);
                if (visualizer != null)
                {
                    visualizers.Add(visualizer);
                }
            }
            
            // Store visualizers for this region
            regionResourceVisualizers[region.regionName] = visualizers;
        }
        
        /// <summary>
        /// Create a resource icon for a specific resource
        /// </summary>
        private GameObject CreateResourceVisualizer(string resourceName, float amount, int index, int totalCount, Transform parent, Vector3 regionPosition)
        {
            GameObject visualizer;
            
            // Create the icon
            if (iconPrefab == null)
            {
                // Create a basic GameObject if no prefab is provided
                visualizer = new GameObject(resourceName);
                visualizer.transform.SetParent(parent, false);
                RectTransform rect = visualizer.AddComponent<RectTransform>();
                
                // Add UI Image component for the icon
                GameObject iconObj = new GameObject("Icon");
                iconObj.transform.SetParent(visualizer.transform, false);
                Image iconImage = iconObj.AddComponent<Image>();
                iconImage.sprite = GetResourceIcon(resourceName);
                iconImage.preserveAspect = true;
                
                // Set icon size
                RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                iconRect.sizeDelta = new Vector2(baseIconSize, baseIconSize);
                iconRect.anchoredPosition = Vector2.zero;
                
                // Add text for quantity if needed
                if (showResourceQuantities)
                {
                    GameObject textObj = new GameObject("QuantityText");
                    textObj.transform.SetParent(visualizer.transform, false);
                    
                    TextMeshProUGUI quantityText = textObj.AddComponent<TextMeshProUGUI>();
                    quantityText.fontSize = quantityFontSize;
                    quantityText.alignment = TextAlignmentOptions.Center;
                    quantityText.color = Color.white;
                    quantityText.outlineWidth = 0.2f;
                    quantityText.outlineColor = Color.black;
                    
                    RectTransform textRect = quantityText.GetComponent<RectTransform>();
                    textRect.sizeDelta = new Vector2(baseIconSize * 1.5f, baseIconSize * 0.5f);
                    textRect.anchoredPosition = new Vector2(0f, -baseIconSize * 0.6f);
                    
                    UpdateQuantityText(quantityText, amount);
                }
            }
            else
            {
                // Instantiate from prefab
                visualizer = Instantiate(iconPrefab, parent);
                visualizer.name = resourceName;
                
                // Find icon image and set sprite
                Image iconImage = visualizer.GetComponentInChildren<Image>();
                if (iconImage != null)
                {
                    iconImage.sprite = GetResourceIcon(resourceName);
                    iconImage.preserveAspect = true;
                }
                
                // Update quantity text if present
                if (showResourceQuantities)
                {
                    TextMeshProUGUI quantityText = visualizer.GetComponentInChildren<TextMeshProUGUI>();
                    if (quantityText != null)
                    {
                        quantityText.fontSize = quantityFontSize;
                        UpdateQuantityText(quantityText, amount);
                    }
                }
            }
            
            // Position icon based on region position and index
            RectTransform rectTransform = visualizer.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = visualizer.AddComponent<RectTransform>();
            }
            
            // Set size if using prefab
            if (iconPrefab != null)
            {
                rectTransform.sizeDelta = new Vector2(baseIconSize, baseIconSize);
            }
            
            // Position in screen space
            Vector2 screenPos = Camera.main.WorldToScreenPoint(regionPosition);
            
            // Position in a circle around the screen position
            Vector2 finalPos = CalculateIconPosition(screenPos, index, totalCount);
            rectTransform.position = finalPos;
            
            return visualizer;
        }
        
        /// <summary>
        /// Calculate the screen position for a resource icon
        /// </summary>
        private Vector2 CalculateIconPosition(Vector2 centerPos, int index, int totalCount)
        {
            // Calculate position in a circular pattern around the region center
            float angleStep = 360f / totalCount;
            float angle = index * angleStep;
            float radius = iconSpacing * iconSize; // Distance from center
            
            float x = centerPos.x + Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
            float y = centerPos.y + Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
            
            return new Vector2(x, y);
        }
        
        /// <summary>
        /// Update the text for resource quantity with readable format
        /// </summary>
        private void UpdateQuantityText(TMP_Text textComponent, float amount)
        {
            // Format amount for readability
            string formattedAmount;
            if (amount >= 1000)
            {
                formattedAmount = (amount / 1000f).ToString("F1") + "k";
            }
            else if (amount >= 100)
            {
                formattedAmount = Mathf.RoundToInt(amount).ToString();
            }
            else
            {
                formattedAmount = amount.ToString("F1");
            }
            
            textComponent.text = formattedAmount;
        }
        
        /// <summary>
        /// Get the icon for a resource, with fallback to default
        /// </summary>
        private Sprite GetResourceIcon(string resourceName)
        {
            if (resourceIconMap.TryGetValue(resourceName, out Sprite icon))
            {
                return icon;
            }
            
            return defaultResourceIcon;
        }
        
        /// <summary>
        /// Clear all visualizers for a specific region
        /// </summary>
        private void ClearVisualizersForRegion(string regionName)
        {
            if (regionResourceVisualizers.TryGetValue(regionName, out List<GameObject> visualizers))
            {
                foreach (var visualizer in visualizers)
                {
                    if (visualizer != null)
                    {
                        Destroy(visualizer);
                    }
                }
                
                visualizers.Clear();
            }
            
            // Also clean up the container
            Transform regionContainer = resourceIconsParent.transform.Find(regionName + "_Resources");
            if (regionContainer != null)
            {
                Destroy(regionContainer.gameObject);
            }
        }
        
        /// <summary>
        /// Clear all visualizers for all regions
        /// </summary>
        public void ClearAllVisualizers()
        {
            foreach (var regionName in regionResourceVisualizers.Keys)
            {
                ClearVisualizersForRegion(regionName);
            }
            
            regionResourceVisualizers.Clear();
        }
        
        /// <summary>
        /// Update positions when the camera moves or zooms
        /// </summary>
        private void LateUpdate()
        {
            // Skip if not showing icons
            if (!showResourceIcons) return;
            
            // Update positions of all resource visualizers
            foreach (var regionEntry in regionResourceVisualizers)
            {
                string regionName = regionEntry.Key;
                GameObject regionObj = GameObject.Find(regionName);
                
                if (regionObj != null)
                {
                    List<GameObject> visualizers = regionEntry.Value;
                    Vector2 screenPos = Camera.main.WorldToScreenPoint(regionObj.transform.position);
                    
                    for (int i = 0; i < visualizers.Count; i++)
                    {
                        GameObject visualizer = visualizers[i];
                        if (visualizer != null)
                        {
                            // Update position
                            RectTransform rt = visualizer.GetComponent<RectTransform>();
                            if (rt != null)
                            {
                                Vector2 newPos = CalculateIconPosition(screenPos, i, visualizers.Count);
                                rt.position = newPos;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Maps resource names to their icon sprites
    /// </summary>
    [System.Serializable]
    public class ResourceIconMapping
    {
        public string resourceName;
        public Sprite resourceIcon;
    }
}