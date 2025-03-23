// MapView.cs - Updated version
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

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

    private void OnEnable()
    {
        // Add this event subscription
        EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
    }

    private void OnDisable()
    {
        // Add this event unsubscription
        EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
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

    public void UpdateRegionVisual(RegionEntity region)
    {
        if (regionObjects.ContainsKey(region.regionName))
        {
            // Update visuals based on region data if needed
        }
    }

    // In MapView.cs, add a method to show economy changes
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
}