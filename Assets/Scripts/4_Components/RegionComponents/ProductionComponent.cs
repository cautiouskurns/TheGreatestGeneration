using System.Collections.Generic;
using UnityEngine;

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
    
    // Execute a recipe, consuming inputs and creating outputs
    private void ProduceRecipe(ResourceProductionRecipe recipe)
    {
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
        // We need to determine which resource this recipe produces
        string outputResource = DetermineOutputResource(recipe);
        if (!string.IsNullOrEmpty(outputResource))
        {
            float outputAmount = recipe.outputAmount * productionEfficiency;
            resourceComponent.AddResource(outputResource, outputAmount);
            
            // Log the production for debugging
            Debug.Log($"Produced {outputAmount} of {outputResource} using recipe {recipe.recipeName}");
        }
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
}