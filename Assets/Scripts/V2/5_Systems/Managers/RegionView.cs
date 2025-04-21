using UnityEngine;
using TMPro;
using V2.Managers;
using V2.Entities;

public class RegionView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer mainRenderer;
    [SerializeField] private SpriteRenderer highlightRenderer;
    [SerializeField] private TextMeshPro nameText;
    
    [Header("Status Texts")]
    [SerializeField] private TextMeshPro wealthText;
    [SerializeField] private TextMeshPro productionText;
    [SerializeField] private TextMeshPro satisfactionText;
    
    // Region data
    public string RegionName { get; private set; }
    public RegionEntity RegionEntity { get; private set; }
    
    public void Initialize(string name, Color color)
    {
        RegionName = name;
        nameText.text = name;
        mainRenderer.color = color;
        highlightRenderer.enabled = false;
        
        // Initialize status texts with zeros
        UpdateStatus(0, 0, 0);
    }
    
    // Overload for compatibility with your implementation
    public void Initialize(string id, string name, Color color)
    {
        RegionName = id;
        nameText.text = name;
        mainRenderer.color = color;
        highlightRenderer.enabled = false;
        
        // Initialize status texts with zeros
        UpdateStatus(0, 0, 0);
    }
    
    // For RegionManager.cs
    public void SetSelected(bool selected)
    {
        highlightRenderer.enabled = selected;
        
        // If selected, notify the RegionManager
        if (selected)
        {
            EventBus.Trigger("RegionSelected", RegionName);
        }
    }
    
    // For MapManager.cs
    public void SetHighlighted(bool highlighted)
    {
        highlightRenderer.enabled = highlighted;
    }
    
    public void SetRegionEntity(RegionEntity regionEntity)
    {
        RegionEntity = regionEntity;
        
        // Update the UI with the entity's data
        if (regionEntity != null)
        {
            UpdateStatus(
                regionEntity.Economy.Wealth,
                regionEntity.Economy.Production,
                regionEntity.Population.Satisfaction
            );
        }
    }
    
    public void UpdateStatus(int wealth, int production, float satisfaction)
    {
        // Update text values with prefixes
        wealthText.text = $"💰{wealth}";
        productionText.text = $"⚙️{production}";
        satisfactionText.text = $"😊{satisfaction:F1}";
        
        // Optional: Add color coding based on values
        wealthText.color = wealth > 100 ? Color.green : Color.white;
        productionText.color = production > 50 ? Color.green : Color.white;
        satisfactionText.color = satisfaction > 0.7f ? Color.green : (satisfaction < 0.3f ? Color.red : Color.white);
    }
    
    private void OnMouseDown()
    {
        // Check which system to notify based on what's available
        RegionManager regionManager = FindFirstObjectByType<RegionManager>();
        if (regionManager != null)
        {
            regionManager.SelectRegion(RegionName);
        }
        else
        {
            // Default to the EventBus for MapManager
            EventBus.Trigger("RegionSelected", RegionName);
        }
    }
    
    // Optional: Update UI when entity changes (e.g., after economic processing)
    public void UpdateFromEntity()
    {
        if (RegionEntity != null)
        {
            UpdateStatus(
                RegionEntity.Economy.Wealth,
                RegionEntity.Economy.Production,
                RegionEntity.Population.Satisfaction
            );
        }
    }
}