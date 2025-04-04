using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central registry for all resource definitions in the game
/// </summary>
public class ResourceRegistry : MonoBehaviour
{
    // Singleton instance
    public static ResourceRegistry Instance { get; private set; }
    
    [Header("Resource Data")]
    public ResourceDataSO[] resourceDefinitions;
    
    // Dictionary for quick lookup
    private Dictionary<string, ResourceDataSO> resourceDict = new Dictionary<string, ResourceDataSO>();
    
    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize resource dictionary
        InitializeResourceDictionary();
    }
    
    private void InitializeResourceDictionary()
    {
        resourceDict.Clear();
        
        if (resourceDefinitions == null)
        {
            Debug.LogWarning("No resource definitions assigned to ResourceRegistry!");
            return;
        }
        
        foreach (var resource in resourceDefinitions)
        {
            if (resource != null)
            {
                resourceDict[resource.resourceName] = resource;
            }
        }
        
        Debug.Log($"ResourceRegistry initialized with {resourceDict.Count} resources");
    }
    
    /// <summary>
    /// Get a resource definition by name
    /// </summary>
    public ResourceDataSO GetResourceDefinition(string resourceName)
    {
        if (resourceDict.TryGetValue(resourceName, out ResourceDataSO resource))
        {
            return resource;
        }
        
        return null;
    }
    
    /// <summary>
    /// Get all resource definitions
    /// </summary>
    public Dictionary<string, ResourceDataSO> GetAllResourceDefinitions()
    {
        return new Dictionary<string, ResourceDataSO>(resourceDict);
    }
    
    /// <summary>
    /// Get resources by category
    /// </summary>
    public List<ResourceDataSO> GetResourcesByCategory(ResourceDataSO.ResourceCategory category)
    {
        List<ResourceDataSO> result = new List<ResourceDataSO>();
        
        foreach (var resource in resourceDict.Values)
        {
            if (resource.category == category)
            {
                result.Add(resource);
            }
        }
        
        return result;
    }
}