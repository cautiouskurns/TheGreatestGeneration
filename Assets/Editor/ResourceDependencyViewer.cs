using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using V1.Data;

#if UNITY_EDITOR
public class ResourceDependencyViewer : EditorWindow
{
    private Vector2 scrollPosition;
    private List<ResourceDataSO> allResources = new List<ResourceDataSO>();
    
    // Visual tree settings
    private bool showVisualTree = true;
    private float nodeWidth = 180f;
    private float nodeHeight = 50f;
    private float horizontalSpacing = 50f;
    private float verticalSpacing = 80f;
    private Vector2 graphScrollPosition;
    private Dictionary<ResourceDataSO, Rect> nodePositions = new Dictionary<ResourceDataSO, Rect>();
    private Dictionary<ResourceDataSO, bool> nodeExpanded = new Dictionary<ResourceDataSO, bool>();
    private ResourceDataSO focusedResource;
    private float zoomLevel = 1f;
    private Vector2 graphOffset = Vector2.zero;
    private bool isDragging = false;
    private Vector2 dragStartPosition;
    
    // Appearance settings
    private GUIStyle nodeStyle;
    private GUIStyle primaryNodeStyle;
    private GUIStyle secondaryNodeStyle;
    private GUIStyle tertiaryNodeStyle;
    private GUIStyle abstractNodeStyle;
    private GUIStyle selectedNodeStyle;
    
    [MenuItem("Game Tools/Resource Dependency Viewer")]
    public static void ShowWindow()
    {
        GetWindow<ResourceDependencyViewer>("Resource Dependencies");
    }
    
    private void OnEnable()
    {
        LoadAllResources();
        InitializeStyles();
    }
    
    private void InitializeStyles()
    {
        nodeStyle = new GUIStyle(EditorStyles.helpBox)
        {
            padding = new RectOffset(10, 10, 8, 8),
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        
        primaryNodeStyle = new GUIStyle(nodeStyle);
        primaryNodeStyle.normal.background = MakeColorTexture(new Color(0.8f, 0.9f, 0.8f));
        
        secondaryNodeStyle = new GUIStyle(nodeStyle);
        secondaryNodeStyle.normal.background = MakeColorTexture(new Color(0.8f, 0.8f, 0.9f));
        
        tertiaryNodeStyle = new GUIStyle(nodeStyle);
        tertiaryNodeStyle.normal.background = MakeColorTexture(new Color(0.9f, 0.8f, 0.9f));
        
        abstractNodeStyle = new GUIStyle(nodeStyle);
        abstractNodeStyle.normal.background = MakeColorTexture(new Color(0.9f, 0.9f, 0.8f));
        
        selectedNodeStyle = new GUIStyle(nodeStyle);
        selectedNodeStyle.normal.background = MakeColorTexture(new Color(1f, 0.8f, 0.6f));
    }
    
    private Texture2D MakeColorTexture(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
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
                
                // Initialize expansion state
                if (!nodeExpanded.ContainsKey(resource))
                {
                    nodeExpanded[resource] = false;
                }
            }
        }
    }
    
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        
        EditorGUILayout.LabelField("Resource Dependency Viewer", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
        {
            LoadAllResources();
        }
        
        showVisualTree = GUILayout.Toggle(showVisualTree, "Graph View", EditorStyles.toolbarButton);
        
        if (showVisualTree)
        {
            // Zoom slider
            EditorGUILayout.LabelField("Zoom:", GUILayout.Width(40));
            zoomLevel = EditorGUILayout.Slider(zoomLevel, 0.5f, 2f, GUILayout.Width(100));
            
            // Resource selector
            EditorGUILayout.LabelField("Focus:", GUILayout.Width(40));
            
            int selectedIndex = focusedResource ? allResources.IndexOf(focusedResource) : -1;
            string[] options = new string[allResources.Count + 1];
            options[0] = "All Resources";
            for (int i = 0; i < allResources.Count; i++)
            {
                options[i + 1] = allResources[i].resourceName;
            }
            
            int newIndex = EditorGUILayout.Popup(selectedIndex + 1, options, GUILayout.Width(150)) - 1;
            if (newIndex != selectedIndex)
            {
                focusedResource = newIndex >= 0 ? allResources[newIndex] : null;
                RecalculateNodePositions();
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        if (showVisualTree)
        {
            DrawResourceGraph();
        }
        else
        {
            DrawListView();
        }
        
        EditorGUILayout.EndVertical();
        
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            GUI.FocusControl(null);
            Repaint();
        }
    }
    
    private void DrawResourceGraph()
    {
        Rect graphRect = new Rect(0, 30, position.width, position.height - 30);
        
        GUI.Box(graphRect, "");
        
        // Handle graph input events
        HandleGraphEvents(graphRect);
        
        // Begin scroll view
        graphScrollPosition = GUI.BeginScrollView(graphRect, graphScrollPosition, 
            new Rect(0, 0, position.width * 2, position.height * 2));
        
        // Calculate node positions if needed
        if (nodePositions.Count == 0)
        {
            RecalculateNodePositions();
        }
        
        // Draw connections between nodes first (so they're behind)
        DrawNodeConnections();
        
        // Then draw nodes
        DrawNodes();
        
        GUI.EndScrollView();
    }
    
    private void HandleGraphEvents(Rect graphRect)
    {
        Event e = Event.current;
        
        // Check if mouse is inside graph area
        if (!graphRect.Contains(e.mousePosition))
            return;
            
        // Handle mouse drag
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            isDragging = true;
            dragStartPosition = e.mousePosition;
            e.Use();
        }
        else if (e.type == EventType.MouseDrag && isDragging)
        {
            Vector2 delta = e.mousePosition - dragStartPosition;
            graphScrollPosition -= delta / zoomLevel;
            dragStartPosition = e.mousePosition;
            Repaint();
            e.Use();
        }
        else if (e.type == EventType.MouseUp && isDragging)
        {
            isDragging = false;
            e.Use();
        }
        
        // Handle mousewheel zoom
        if (e.type == EventType.ScrollWheel)
        {
            float oldZoom = zoomLevel;
            zoomLevel = Mathf.Clamp(zoomLevel - e.delta.y * 0.05f, 0.5f, 2f);
            
            // Adjust scroll position to zoom toward mouse position
            if (oldZoom != zoomLevel)
            {
                Vector2 mousePos = e.mousePosition - graphRect.position;
                Vector2 zoomCenter = graphScrollPosition + mousePos / oldZoom;
                graphScrollPosition = zoomCenter - mousePos / zoomLevel;
                
                Repaint();
                e.Use();
            }
        }
    }
    
    private void RecalculateNodePositions()
    {
        nodePositions.Clear();
        
        if (focusedResource != null)
        {
            // Focus on single resource and its dependencies
            CalculateNodePositionsForResource(focusedResource, new Vector2(position.width / 2, 50), new HashSet<ResourceDataSO>(), 0);
        }
        else
        {
            // Show all resources organized by category
            float y = 50;
            float x = 50;
            
            // Group resources by category
            var primaryResources = allResources.FindAll(r => r.category == ResourceDataSO.ResourceCategory.Primary);
            var secondaryResources = allResources.FindAll(r => r.category == ResourceDataSO.ResourceCategory.Secondary);
            var tertiaryResources = allResources.FindAll(r => r.category == ResourceDataSO.ResourceCategory.Tertiary);
            var abstractResources = allResources.FindAll(r => r.category == ResourceDataSO.ResourceCategory.Abstract);
            
            // Layout resources by category
            x = 50;
            y = 50;
            foreach (var resource in primaryResources)
            {
                nodePositions[resource] = new Rect(x, y, nodeWidth, nodeHeight);
                x += nodeWidth + horizontalSpacing;
                if (x > position.width - nodeWidth - 50)
                {
                    x = 50;
                    y += nodeHeight + verticalSpacing;
                }
            }
            
            y += nodeHeight + verticalSpacing * 2;
            x = 50;
            foreach (var resource in secondaryResources)
            {
                nodePositions[resource] = new Rect(x, y, nodeWidth, nodeHeight);
                x += nodeWidth + horizontalSpacing;
                if (x > position.width - nodeWidth - 50)
                {
                    x = 50;
                    y += nodeHeight + verticalSpacing;
                }
            }
            
            y += nodeHeight + verticalSpacing * 2;
            x = 50;
            foreach (var resource in tertiaryResources)
            {
                nodePositions[resource] = new Rect(x, y, nodeWidth, nodeHeight);
                x += nodeWidth + horizontalSpacing;
                if (x > position.width - nodeWidth - 50)
                {
                    x = 50;
                    y += nodeHeight + verticalSpacing;
                }
            }
            
            y += nodeHeight + verticalSpacing * 2;
            x = 50;
            foreach (var resource in abstractResources)
            {
                nodePositions[resource] = new Rect(x, y, nodeWidth, nodeHeight);
                x += nodeWidth + horizontalSpacing;
                if (x > position.width - nodeWidth - 50)
                {
                    x = 50;
                    y += nodeHeight + verticalSpacing;
                }
            }
        }
    }
    
    private void CalculateNodePositionsForResource(ResourceDataSO resource, Vector2 position, HashSet<ResourceDataSO> visited, int depth)
    {
        if (visited.Contains(resource))
            return;
            
        visited.Add(resource);
        
        // Position this node
        nodePositions[resource] = new Rect(position.x - nodeWidth / 2, position.y, nodeWidth, nodeHeight);
        
        // Find inputs (dependencies)
        List<ResourceDataSO> dependencies = new List<ResourceDataSO>();
        if (resource.productionRecipes != null)
        {
            foreach (var recipe in resource.productionRecipes)
            {
                if (recipe.inputs != null)
                {
                    foreach (var input in recipe.inputs)
                    {
                        if (input.resource != null && !dependencies.Contains(input.resource))
                        {
                            dependencies.Add(input.resource);
                        }
                    }
                }
            }
        }
        
        // Calculate positions for dependencies
        if (dependencies.Count > 0)
        {
            float totalWidth = (dependencies.Count - 1) * (nodeWidth + horizontalSpacing);
            float startX = position.x - totalWidth / 2;
            
            for (int i = 0; i < dependencies.Count; i++)
            {
                float x = startX + i * (nodeWidth + horizontalSpacing);
                Vector2 childPos = new Vector2(x, position.y + nodeHeight + verticalSpacing);
                
                CalculateNodePositionsForResource(dependencies[i], childPos, visited, depth + 1);
            }
        }
    }
    
    private void DrawNodes()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        
        // Apply zoom
        GUIUtility.ScaleAroundPivot(new Vector2(zoomLevel, zoomLevel), Vector2.zero);
        
        foreach (var resource in nodePositions.Keys)
        {
            Rect nodeRect = nodePositions[resource];
            
            // Skip if not in view
            if (!IsNodeVisible(nodeRect))
                continue;
            
            // Choose style based on category
            GUIStyle style = nodeStyle;
            switch (resource.category)
            {
                case ResourceDataSO.ResourceCategory.Primary:
                    style = primaryNodeStyle;
                    break;
                case ResourceDataSO.ResourceCategory.Secondary:
                    style = secondaryNodeStyle;
                    break;
                case ResourceDataSO.ResourceCategory.Tertiary:
                    style = tertiaryNodeStyle;
                    break;
                case ResourceDataSO.ResourceCategory.Abstract:
                    style = abstractNodeStyle;
                    break;
            }
            
            // Override style if this is the focused resource
            if (resource == focusedResource)
            {
                style = selectedNodeStyle;
            }
            
            // Draw node box
            GUI.Box(nodeRect, "", style);
            
            // Draw resource icon if available
            if (resource.resourceIcon != null)
            {
                Rect iconRect = new Rect(
                    nodeRect.x + 5, 
                    nodeRect.y + (nodeRect.height - 24) / 2, 
                    24, 24);
                GUI.DrawTexture(iconRect, resource.resourceIcon.texture);
                
                // Draw label next to icon
                Rect labelRect = new Rect(
                    iconRect.x + iconRect.width + 5,
                    nodeRect.y,
                    nodeRect.width - iconRect.width - 15,
                    nodeRect.height);
                GUI.Label(labelRect, resource.resourceName);
            }
            else
            {
                // Draw centered label if no icon
                GUI.Label(nodeRect, resource.resourceName);
            }
            
            // Handle click on node
            if (Event.current.type == EventType.MouseDown && 
                Event.current.button == 0 && 
                nodeRect.Contains(Event.current.mousePosition / zoomLevel))
            {
                focusedResource = resource;
                RecalculateNodePositions();
                Event.current.Use();
                Repaint();
            }
            
            // Handle double-click to inspect resource
            if (Event.current.type == EventType.MouseDown && 
                Event.current.button == 0 && 
                Event.current.clickCount == 2 &&
                nodeRect.Contains(Event.current.mousePosition / zoomLevel))
            {
                Selection.activeObject = resource;
                EditorGUIUtility.PingObject(resource);
                Event.current.Use();
            }
        }
        
        GUI.matrix = oldMatrix;
    }
    
    private bool IsNodeVisible(Rect nodeRect)
    {
        // Adjust for zoom and scroll
        Rect screenRect = new Rect(
            graphScrollPosition.x, 
            graphScrollPosition.y, 
            position.width / zoomLevel, 
            position.height / zoomLevel);
            
        return nodeRect.Overlaps(screenRect);
    }
    
    private void DrawNodeConnections()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        
        // Apply zoom
        GUIUtility.ScaleAroundPivot(new Vector2(zoomLevel, zoomLevel), Vector2.zero);
        
        // Draw connections for each resource with recipes
        foreach (var resource in allResources)
        {
            if (!nodePositions.ContainsKey(resource))
                continue;
                
            if (resource.productionRecipes == null || resource.productionRecipes.Length == 0)
                continue;
                
            Rect targetRect = nodePositions[resource];
            
            // Draw connection for each input in each recipe
            foreach (var recipe in resource.productionRecipes)
            {
                if (recipe.inputs == null)
                    continue;
                    
                foreach (var input in recipe.inputs)
                {
                    if (input.resource == null || !nodePositions.ContainsKey(input.resource))
                        continue;
                        
                    Rect sourceRect = nodePositions[input.resource];
                    
                    Vector3 start = new Vector3(sourceRect.x + sourceRect.width / 2, sourceRect.y + sourceRect.height / 2, 0);
                    Vector3 end = new Vector3(targetRect.x + targetRect.width / 2, targetRect.y + targetRect.height / 2, 0);
                    
                    // Calculate curve control points
                    Vector3 startTangent = start + Vector3.up * 50;
                    Vector3 endTangent = end + Vector3.down * 50;
                    
                    // Draw arrow
                    Handles.BeginGUI();
                    Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                    Handles.DrawBezier(start, end, startTangent, endTangent, Handles.color, null, 2f);
                    
                    // Draw arrow tip
                    Vector3 direction = (end - (end + Vector3.down * 20)).normalized;
                    Vector3 arrowTip = end - direction * 10;
                    Vector3 arrowSide1 = arrowTip + Quaternion.Euler(0, 0, 30) * direction * 6;
                    Vector3 arrowSide2 = arrowTip + Quaternion.Euler(0, 0, -30) * direction * 6;
                    Handles.DrawPolyLine(arrowSide1, arrowTip, arrowSide2);
                    
                    // Draw amount label
                    Vector3 labelPos = Vector3.Lerp(start, end, 0.5f);
                    Rect labelRect = new Rect(labelPos.x - 15, labelPos.y - 10, 30, 20);
                    GUI.Label(labelRect, input.amount.ToString(), EditorStyles.miniLabel);
                    
                    Handles.EndGUI();
                }
            }
        }
        
        GUI.matrix = oldMatrix;
    }
    
    private void DrawListView()
    {
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