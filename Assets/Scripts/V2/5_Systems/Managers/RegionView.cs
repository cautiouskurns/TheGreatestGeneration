using UnityEngine;
using TMPro;
using V2.Managers;
using V2.Entities;
using V2.Systems;

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
    
    // Add a cache for the economic system reference to avoid repeated lookups
    private EconomicSystem economicSystem;
    
    // Flag to prevent multiple updates in the same frame
    private bool updatingThisFrame = false;
    private float lastUpdateTime = 0f;
    private const float MIN_UPDATE_INTERVAL = 0.25f; // Minimum 1/4 second between updates
    
    private void Awake()
    {
        // Get economic system reference once
        economicSystem = FindFirstObjectByType<EconomicSystem>();
    }
    
    private void LateUpdate()
    {
        // Reset the update flag at the end of the frame
        updatingThisFrame = false;
    }
    
    public void Initialize(string name, Color color)
    {
        RegionName = name;
        nameText.text = name;
        mainRenderer.color = color;
        highlightRenderer.enabled = false;
        
        // Initialize status texts with zeros
        UpdateStatus(0, 0, 0);
        
        // Subscribe to events right away
        SubscribeToEvents();
        
        // Try to get the region entity from the economic system (if available)
        TryGetRegionEntityFromSystem();
    }
    
    // Overload for compatibility with MapManager implementation
    public void Initialize(string id, string name, Color color)
    {
        RegionName = id;
        nameText.text = name;
        mainRenderer.color = color;
        highlightRenderer.enabled = false;
        
        // Initialize status texts with zeros
        UpdateStatus(0, 0, 0);
        
        // Subscribe to events right away
        SubscribeToEvents();
        
        // Try to get the region entity from the economic system (if available)
        TryGetRegionEntityFromSystem();
    }
    
    private void TryGetRegionEntityFromSystem()
    {
        if (economicSystem != null && !string.IsNullOrEmpty(RegionName))
        {
            // Check if this region already exists in the economic system
            RegionEntity existingEntity = economicSystem.GetRegion(RegionName);
            if (existingEntity != null)
            {
                RegionEntity = existingEntity;
                SafeUpdateStatus(
                    existingEntity.Economy.Wealth,
                    existingEntity.Economy.Production,
                    existingEntity.Population.Satisfaction
                );
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to RegionUpdated events to keep our view updated
        EventBus.Subscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Subscribe("EconomicTick", OnEconomicTick);
    }
    
    private void OnEconomicTick(object data)
    {
        // When an economic tick happens, refresh our entity reference and update UI
        if (economicSystem != null && !string.IsNullOrEmpty(RegionName))
        {
            RegionEntity entity = economicSystem.GetRegion(RegionName);
            if (entity != null)
            {
                // Update our reference if necessary
                if (RegionEntity != entity)
                {
                    RegionEntity = entity;
                }
                
                // Schedule a UI update for the next frame to avoid conflicts
                Invoke("DelayedUpdateFromEntity", 0.1f);
            }
        }
    }
    
    private void DelayedUpdateFromEntity()
    {
        if (RegionEntity != null)
        {
            SafeUpdateStatus(
                RegionEntity.Economy.Wealth,
                RegionEntity.Economy.Production,
                RegionEntity.Population.Satisfaction
            );
        }
    }
    
    private void OnDestroy()
    {
        // Always unsubscribe when destroying
        EventBus.Unsubscribe("RegionUpdated", OnRegionUpdated);
        EventBus.Unsubscribe("EconomicTick", OnEconomicTick);
        CancelInvoke();
    }
    
    private void OnRegionUpdated(object data)
    {
        // Only update if this is our region entity or if our RegionName matches
        if (data is RegionEntity region && region.Name == RegionName)
        {
            // Always keep our reference up to date
            RegionEntity = region;
            
            // Use the safe update method to avoid spamming updates
            SafeUpdateStatus(
                region.Economy.Wealth,
                region.Economy.Production,
                region.Population.Satisfaction
            );
        }
    }
    
    // For RegionManager.cs
    public void SetSelected(bool selected)
    {
        highlightRenderer.enabled = selected;
        
        // If selected, notify the RegionManager - but don't trigger events in this frame if already updating
        if (selected && !updatingThisFrame)
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
        if (regionEntity == null) return;
        
        // Keep stable reference to prevent flickering
        RegionEntity = regionEntity;
        
        // Update the UI with the entity's data immediately
        SafeUpdateStatus(
            regionEntity.Economy.Wealth,
            regionEntity.Economy.Production,
            regionEntity.Population.Satisfaction
        );
    }
    
    // Safe update that prevents multiple updates in the same frame
    private void SafeUpdateStatus(int wealth, int production, float satisfaction)
    {
        // Check if we've updated recently to prevent spamming updates
        if (updatingThisFrame || Time.realtimeSinceStartup - lastUpdateTime < MIN_UPDATE_INTERVAL)
        {
            return;
        }
        
        // Set flag to prevent multiple updates in the same frame
        updatingThisFrame = true;
        lastUpdateTime = Time.realtimeSinceStartup;
        
        // Update UI
        UpdateStatus(wealth, production, satisfaction);
    }
    
    public void UpdateStatus(int wealth, int production, float satisfaction)
    {
        // Use string interpolation for better performance and readability
        if (wealthText != null)
            wealthText.text = $"W: {wealth}";
            
        if (productionText != null)
            productionText.text = $"P: {production}";
            
        if (satisfactionText != null)
            satisfactionText.text = $"S: {satisfaction:F1}";
        
        // Optional: Add color coding based on values
        if (wealthText != null)
            wealthText.color = wealth > 100 ? Color.green : Color.white;
            
        if (productionText != null)
            productionText.color = production > 50 ? Color.green : Color.white;
            
        if (satisfactionText != null)
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
    
    // Update UI when entity changes (e.g., after economic processing)
    public void UpdateFromEntity()
    {
        if (RegionEntity != null)
        {
            SafeUpdateStatus(
                RegionEntity.Economy.Wealth,
                RegionEntity.Economy.Production,
                RegionEntity.Population.Satisfaction
            );
        }
    }
}