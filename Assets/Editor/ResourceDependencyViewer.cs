using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
public class ResourceDependencyViewer : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ResourceDataSO> allResources = new List<ResourceDataSO>();
    
    [MenuItem("Game Tools/Resource Dependency Viewer")]
    public static void ShowWindow()
    {
        GetWindow<ResourceDependencyViewer>("Resource Dependencies");
    }
    
    private void OnEnable()
    {
        LoadAllResources();
    }
    
    private void LoadAllResources()
    {
        allResources.Clear();
        string[] guids = AssetDatabase.FindAssets("t:ResourceDataSO");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ResourceDataSO resource = AssetDatabase.LoadAssetAtPath<ResourceDataSO>(path);
            if (resource != null)
            {
                allResources.Add(resource);
            }
        }
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Resource Dependency Viewer", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Refresh Resources"))
        {
            LoadAllResources();
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        foreach (ResourceDataSO resource in allResources)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            // Resource header
            EditorGUILayout.BeginHorizontal();
            
            if (resource.resourceIcon != null)
            {
                GUILayout.Label(new GUIContent(resource.resourceIcon.texture), GUILayout.Width(32), GUILayout.Height(32));
            }
            
            EditorGUILayout.LabelField($"{resource.resourceName} ({resource.category})", EditorStyles.boldLabel);
            
            EditorGUILayout.EndHorizontal();
            
            // Production recipes
            if (resource.productionRecipes != null && resource.productionRecipes.Length > 0)
            {
                EditorGUILayout.LabelField("Production Recipes:", EditorStyles.boldLabel);
                
                foreach (ResourceProductionRecipe recipe in resource.productionRecipes)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(recipe.recipeName) ? 
                        "Recipe" : recipe.recipeName, EditorStyles.boldLabel);
                    
                    if (recipe.inputs != null && recipe.inputs.Length > 0)
                    {
                        EditorGUILayout.LabelField("Inputs:");
                        
                        foreach (ResourceInput input in recipe.inputs)
                        {
                            if (input.resource != null)
                            {
                                EditorGUILayout.BeginHorizontal();
                                
                                string consumedText = input.consumed ? "" : " (not consumed)";
                                EditorGUILayout.LabelField($"• {input.resource.resourceName}: {input.amount}{consumedText}");
                                
                                EditorGUILayout.EndHorizontal();
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No inputs required (raw resource)");
                    }
                    
                    EditorGUILayout.LabelField($"Output: {recipe.outputAmount} units");
                    EditorGUILayout.LabelField($"Time: {recipe.productionTime} turns");
                    EditorGUILayout.LabelField($"Infrastructure: {recipe.requiredInfrastructureType} (Level {recipe.minimumInfrastructureLevel}+)");
                    
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No production recipes defined.");
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.Space(10);
        }
        
        EditorGUILayout.EndScrollView();
    }
}
#endif