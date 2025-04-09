using UnityEngine;
using System.Collections.Generic;
using V1.Entities;
using V1.Data;

namespace V1.Utils
{ 
    /// CLASS PURPOSE:
    /// ResourceInitializer is responsible for initializing the resource systems of all
    /// regions at game startup. It loads resource definitions, activates default production
    /// recipes, and optionally sets up artificial resource imbalances for testing.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Validate region and resource input data
    /// - Load resource definitions into each region's ResourceComponent
    /// - Activate initial production recipes to bootstrap economic simulation
    /// - Create initial resource surpluses/deficits to enable trade testing
    /// 
    /// KEY COLLABORATORS:
    /// - RegionEntity: Owns the resource and production components for each region
    /// - ResourceDataSO: Defines available resources and metadata
    /// - ResourceComponent: Initialized with definitions and assigned quantities
    /// - ProductionComponent: Used to activate sample recipes
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Imbalance creation is hardcoded and currently only affects the first two regions
    /// - Method structure is simple and designed for bootstrapping test environments
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Move test logic (e.g., ActivateDefaultRecipes, CreateImbalances) into a dev-only initializer
    /// - Parameterize resource names and amounts to support different test setups
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Add support for loading region-specific starting resources from data files
    /// - Integrate with tech trees or scenario scripts for dynamic initialization
    /// - Track which resources were added programmatically for validation or rollback
    /// 
    /// <summary>
    /// Manages the initialization and configuration of resources across regions
    /// </summary>
    public class ResourceInitializer
    {
        /// <summary>
        /// Initialize resources for all regions
        /// </summary>
        /// <param name="regions">Dictionary of regions to initialize</param>
        /// <param name="availableResources">Array of resource definitions</param>
        public void InitializeRegionResources(
            Dictionary<string, RegionEntity> regions, 
            ResourceDataSO[] availableResources)
        {
            // Validate inputs
            if (regions == null || regions.Count == 0)
            {
                Debug.LogWarning("No regions available for resource initialization");
                return;
            }

            if (availableResources == null || availableResources.Length == 0)
            {
                Debug.LogWarning("No resource definitions available");
                return;
            }

            // Initialize resources for each region
            foreach (var regionEntry in regions)
            {
                RegionEntity region = regionEntry.Value;
                
                if (region.resources != null)
                {
                    // Load resource definitions
                    region.resources.LoadResourceDefinitions(availableResources);
                    
                    // Activate some default recipes for testing
                    if (region.productionComponent != null)
                    {
                        ActivateDefaultRecipes(region);
                    }
                }
            }

            // Optionally create some initial resource imbalances for testing
            CreateInitialResourceImbalances(regions);
        }

        /// <summary>
        /// Activate some default production recipes
        /// </summary>
        private void ActivateDefaultRecipes(RegionEntity region)
        {
            // Example of activating a specific recipe
            region.productionComponent.ActivateRecipe("Basic Iron Smelting");
        }

        /// <summary>
        /// Create some initial resource imbalances to test trade mechanics
        /// </summary>
        private void CreateInitialResourceImbalances(Dictionary<string, RegionEntity> regions)
        {
            // Ensure we have at least two regions to create imbalance
            if (regions.Count < 2)
            {
                Debug.LogWarning("Not enough regions to create resource imbalances");
                return;
            }

            // Convert regions to an array for easier indexing
            var regionArray = new RegionEntity[regions.Count];
            regions.Values.CopyTo(regionArray, 0);
            
            // Give first region excess food
            if (regionArray[0].resources != null)
            {
                regionArray[0].resources.AddResource("Crops", 100);
            }
            
            // Give second region excess iron
            if (regionArray[1].resources != null)
            {
                regionArray[1].resources.AddResource("Iron Ore", 100);
            }
        }
    }
}