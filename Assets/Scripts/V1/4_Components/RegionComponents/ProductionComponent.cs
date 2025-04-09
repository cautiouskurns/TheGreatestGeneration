using System.Collections.Generic;
using UnityEngine;
using V1.Entities;
using V1.Data;

namespace V1.Components
{
    /// CLASS PURPOSE:
    /// ProductionComponent handles the transformation of input resources into outputs using
    /// configurable recipes. It simulates industrial behavior at the region level in the economic system.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Track active production recipes and manage their progress over turns
    /// - Evaluate input availability, dependencies, and infrastructure readiness
    /// - Apply production logic, efficiency, and quality modifiers to determine output
    ///
    /// KEY COLLABORATORS:
    /// - RegionEntity: Owns this component and provides regional context
    /// - ResourceComponent: Supplies and stores resources consumed or produced
    /// - ResourceDataSO & ResourceProductionRecipe: Define what and how resources are processed
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Multi-turn recipe progress is tracked using a dictionary keyed by recipe name
    /// - Efficiency and quality impact are calculated dynamically per recipe
    /// - Output resource is inferred from recipe metadata, which could be simplified
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Move recipe lookup logic into ResourceComponent or a dedicated RecipeManager
    /// - Cache parsed recipes to reduce repeated iteration over definitions
    /// - Refactor DetermineOutputResource to store explicit output references in recipes
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add recipe categories, seasonal modifiers, or region traits affecting efficiency
    /// - Implement UI to visualize active recipes, progress, and output projections
    /// - Integrate with tech tree or policy system to unlock new recipes over time
    /// <summary>
    /// Handles the production and transformation of resources according to recipes
    /// </summary>
    public class ProductionComponent
    {
        // Reference to the region and its resource component
        private RegionEntity ownerRegion;
        private ResourceComponent resourceComponent;
        
        // Active production recipes
        private List<string> activeRecipes = new List<string>();
        
        // Production capacity and efficiency
        private float productionEfficiency = 1.0f;
        
        // Dictionary to store recipe cooldowns (for recipes that take multiple turns)
        private Dictionary<string, float> recipeProgress = new Dictionary<string, float>();
        
        // Constructor
        public ProductionComponent(RegionEntity owner, ResourceComponent resources)
        {
            ownerRegion = owner;
            resourceComponent = resources;
        }
        
        // Process all production recipes during a turn
        public void ProcessProduction()
        {
            // Skip if no resource component
            if (resourceComponent == null) return;
            
            // Get available recipes from resource definitions
            Dictionary<string, ResourceDataSO> resourceDefs = resourceComponent.GetResourceDefinitions();
            
            if (resourceDefs == null || resourceDefs.Count == 0) return;
            
            // Process each active recipe
            foreach (string recipeName in activeRecipes)
            {
                // Find the recipe in resource definitions
                ResourceProductionRecipe recipe = FindRecipe(recipeName, resourceDefs);
                if (recipe == null) continue;
                
                // Check if we can produce the recipe
                if (CanProduceRecipe(recipe))
                {
                    // Update progress for multi-turn recipes
                    if (recipe.productionTime > 1)
                    {
                        // Initialize progress if needed
                        if (!recipeProgress.ContainsKey(recipeName))
                        {
                            recipeProgress[recipeName] = 0;
                        }
                        
                        // Add progress (with efficiency applied)
                        recipeProgress[recipeName] += productionEfficiency;
                        
                        // Check if recipe is complete
                        if (recipeProgress[recipeName] >= recipe.productionTime)
                        {
                            ProduceRecipe(recipe);
                            recipeProgress[recipeName] = 0; // Reset progress
                        }
                    }
                    else
                    {
                        // Single-turn recipe, produce immediately
                        ProduceRecipe(recipe);
                    }
                }
            }
        }
        
        // Find a recipe by name in resource definitions
        private ResourceProductionRecipe FindRecipe(string recipeName, Dictionary<string, ResourceDataSO> resourceDefs)
        {
            foreach (var resourceData in resourceDefs.Values)
            {
                if (resourceData.productionRecipes != null)
                {
                    foreach (var recipe in resourceData.productionRecipes)
                    {
                        if (recipe.recipeName == recipeName)
                        {
                            return recipe;
                        }
                    }
                }
            }
            return null;
        }
        
        // Check if we have all the inputs for a recipe
        public bool CanProduceRecipe(ResourceProductionRecipe recipe)
        {
            // Check infrastructure requirements if applicable
            if (!string.IsNullOrEmpty(recipe.requiredInfrastructureType) && recipe.minimumInfrastructureLevel > 0)
            {
                // TODO: Check infrastructure component once implemented
                // For now, assume infrastructure is available
            }
            
            // Check if we have all required inputs
            if (recipe.inputs != null && recipe.inputs.Length > 0)
            {
                foreach (var input in recipe.inputs)
                {
                    if (input.resource == null) continue;
                    
                    float available = resourceComponent.GetResourceAmount(input.resource.resourceName);
                    if (available < input.amount)
                    {
                        return false; // Not enough of this input
                    }
                }
            }
            
            return true;
        }
        
        private void ProduceRecipe(ResourceProductionRecipe recipe)
        {
            // Store original efficiency
            float originalEfficiency = productionEfficiency;
            
            // Check dependencies first
            CheckResourceDependencies(recipe);
            
            // First, consume all inputs
            if (recipe.inputs != null && recipe.inputs.Length > 0)
            {
                foreach (var input in recipe.inputs)
                {
                    if (input.resource == null) continue;
                    
                    if (input.consumed)
                    {
                        resourceComponent.RemoveResource(input.resource.resourceName, input.amount);
                    }
                }
            }
            
            // Then produce the output
            string outputResource = DetermineOutputResource(recipe);
            if (!string.IsNullOrEmpty(outputResource))
            {
                // Apply efficiency multiplier
                float efficiencyFactor = productionEfficiency * recipe.efficiencyMultiplier;
                float outputAmount = recipe.outputAmount * efficiencyFactor;
                
                // Apply quality impact if enabled
                if (recipe.qualityAffectsOutput)
                {
                    // Calculate quality factor based on input resources and dependencies
                    float qualityFactor = CalculateQualityFactor(recipe);
                    outputAmount *= qualityFactor;
                }
                
                resourceComponent.AddResource(outputResource, outputAmount);
                
                // Log the production with efficiency
                Debug.Log($"Produced {outputAmount:F1} of {outputResource} " +
                        $"(efficiency: {efficiencyFactor:P0}) using recipe {recipe.recipeName}");
            }
            
            // Restore original efficiency for other recipes
            productionEfficiency = originalEfficiency;
        }

        // Determine which resource this recipe produces
        private string DetermineOutputResource(ResourceProductionRecipe recipe)
        {
            // In the current design, we need to infer this from the resource definition
            // A better design might include the output resource directly in the recipe
            
            Dictionary<string, ResourceDataSO> resourceDefs = resourceComponent.GetResourceDefinitions();
            
            foreach (var entry in resourceDefs)
            {
                string resourceName = entry.Key;
                ResourceDataSO resourceData = entry.Value;
                
                if (resourceData.productionRecipes != null)
                {
                    foreach (var r in resourceData.productionRecipes)
                    {
                        if (r.recipeName == recipe.recipeName)
                        {
                            return resourceName; // This resource contains the recipe
                        }
                    }
                }
            }
            
            return string.Empty;
        }
        
        private float CalculateQualityFactor(ResourceProductionRecipe recipe)
        {
            // Base quality is 1.0
            float qualityFactor = 1.0f;
            
            // Adjust based on input amount surplus
            if (recipe.inputs != null && recipe.inputs.Length > 0)
            {
                float totalQualityContribution = 0f;
                
                foreach (var input in recipe.inputs)
                {
                    if (input.resource == null) continue;
                    
                    // Get available amount
                    float available = resourceComponent.GetResourceAmount(input.resource.resourceName);
                    
                    // If we have a surplus beyond what's required, it can improve quality
                    if (available > input.amount * 1.5f)
                    {
                        // Cap the bonus to prevent extreme values
                        float surplus = Mathf.Min(available / input.amount, 3.0f) - 1.0f;
                        totalQualityContribution += surplus * 0.1f; // 10% quality boost per doubling of input
                    }
                }
                
                // Apply quality contribution with the recipe's quality impact factor
                qualityFactor += totalQualityContribution * recipe.qualityImpact;
            }
            
            // Cap quality factor to reasonable range
            return Mathf.Clamp(qualityFactor, 0.5f, 1.5f);
        }

        // Activate a recipe for production
        public void ActivateRecipe(string recipeName)
        {
            if (!activeRecipes.Contains(recipeName))
            {
                activeRecipes.Add(recipeName);
            }
        }
        
        // Deactivate a recipe
        public void DeactivateRecipe(string recipeName)
        {
            activeRecipes.Remove(recipeName);
            
            // Clear any progress
            if (recipeProgress.ContainsKey(recipeName))
            {
                recipeProgress.Remove(recipeName);
            }
        }
        
        // Set production efficiency (modified by infrastructure, skills, etc.)
        public void SetProductionEfficiency(float efficiency)
        {
            productionEfficiency = Mathf.Max(0.1f, efficiency);
        }
        
        // Get a list of all active recipes
        public List<string> GetActiveRecipes()
        {
            return new List<string>(activeRecipes);
        }
        
        // Get production progress for a recipe
        public float GetRecipeProgress(string recipeName)
        {
            if (recipeProgress.ContainsKey(recipeName))
            {
                return recipeProgress[recipeName];
            }
            return 0;
        }
        
        // Get recipes that can be currently produced
        public List<ResourceProductionRecipe> GetAvailableRecipes()
        {
            List<ResourceProductionRecipe> available = new List<ResourceProductionRecipe>();
            Dictionary<string, ResourceDataSO> resourceDefs = resourceComponent.GetResourceDefinitions();
            
            if (resourceDefs == null) return available;
            
            foreach (var resourceData in resourceDefs.Values)
            {
                if (resourceData.productionRecipes != null)
                {
                    foreach (var recipe in resourceData.productionRecipes)
                    {
                        if (CanProduceRecipe(recipe))
                        {
                            available.Add(recipe);
                        }
                    }
                }
            }
            
            return available;
        }

        /// <summary>
        /// Check if we have all required dependencies for a recipe
        /// </summary>
        /// <param name="recipe">The recipe to check</param>
        /// <returns>True if all dependencies are available, otherwise false with reduced efficiency</returns>
        private bool CheckResourceDependencies(ResourceProductionRecipe recipe)
        {
            // Skip if no dependencies
            if (recipe.dependencies == null || recipe.dependencies.Length == 0)
                return true;
            
            // Track original efficiency to restore if needed
            float originalEfficiency = productionEfficiency;
            bool allDependenciesMet = true;
            
            // Check each dependency
            foreach (var dependency in recipe.dependencies)
            {
                // Get amount of dependency available
                float available = resourceComponent.GetResourceAmount(dependency.resourceName);
                
                // If we don't have enough of the dependency, reduce efficiency
                if (available < dependency.requiredAmount)
                {
                    allDependenciesMet = false;
                    
                    // Only reduce efficiency if this dependency affects it
                    if (dependency.affectsEfficiency)
                    {
                        // Calculate efficiency reduction
                        float ratio = available / dependency.requiredAmount;
                        
                        // Apply impact weight to determine how much this affects efficiency
                        float impact = Mathf.Pow(ratio, dependency.impactWeight);
                        
                        // Apply to production efficiency (proportional reduction)
                        productionEfficiency *= Mathf.Max(0.1f, impact);
                    }
                }
            }
            
            // Log if efficiency was reduced
            if (productionEfficiency < originalEfficiency)
            {
                Debug.Log($"Production efficiency reduced to {productionEfficiency:P0} due to missing dependencies");
            }
            
            return allDependenciesMet;
        }

    }
}