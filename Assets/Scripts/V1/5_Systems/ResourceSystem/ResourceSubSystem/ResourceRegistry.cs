using UnityEngine;
using System.Collections.Generic;
using V1.Data;

namespace V1.Systems
{    
/// CLASS PURPOSE:
    /// ResourceRegistry serves as a global singleton registry for all defined resources
    /// in the game. It provides efficient access to resource metadata such as name,
    /// category, and base attributes.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Store and manage all resource definitions loaded via ResourceDataSO
    /// - Provide fast lookup via dictionary keyed by resource name
    /// - Offer access to filtered or categorized resource sets
    /// 
    /// KEY COLLABORATORS:
    /// - ResourceComponent: Uses registry to retrieve metadata and pricing info
    /// - ResourceMarket: Looks up base resource definitions and categories
    /// - ProductionComponent / UI Systems: Access resource names, types, and icons via this registry
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Follows MonoBehaviour singleton pattern with DontDestroyOnLoad
    /// - Uses defensive null checks and logs during initialization
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Validate uniqueness of resource names during initialization
    /// - Consider adding reverse lookups or support for localization
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Add event hooks for dynamic resource registration/unregistration
    /// - Support multiple tiers, resource evolution, or tech-tree unlocks
    /// - Enable export/import of resource definitions for modding or debugging


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
}