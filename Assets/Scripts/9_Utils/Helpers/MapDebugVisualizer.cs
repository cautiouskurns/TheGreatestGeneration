using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Helper class for visualizing terrain and map generation for debugging purposes
/// </summary>
public class MapDebugVisualizer : MonoBehaviour
{
    [Header("Visualization Settings")]
    public bool showElevationMap = false;
    public bool showMoistureMap = false;
    public bool showTerrainMap = true;
    public bool showNationMap = true;
    
    [Header("Generation Parameters")]
    [Range(1, 100)]
    public float elevationScale = 30f;
    [Range(1, 100)]
    public float moistureScale = 50f;
    public int seed = 0;
    
    [Header("Map Dimensions")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    
    [Header("References")]
    public TerrainTypeDataSO[] terrainTypes;
    
    [Header("Debug Visuals")]
    public GameObject mapContainer;
    public GameObject debugTilePrefab;
    
    // Generated data
    private float[,] elevationMap;
    private float[,] moistureMap;
    private TerrainTypeDataSO[,] terrainMap;
    private int[,] nationMap;
    private GameObject[,] mapTiles;
    private Dictionary<string, TerrainTypeDataSO> terrainTypeDict;
    
    private void Start()
    {
        if (terrainTypes == null || terrainTypes.Length < 5)
        {
            Debug.LogError("Please assign all basic terrain types (Plains, Mountains, Forest, Desert, Water)");
            return;
        }
        
        // Initialize terrain dictionary
        InitializeTerrainDictionary();
        
        // Generate and visualize the map
        GenerateAndVisualize();
    }
    
    /// <summary>
    /// Create a dictionary of terrain types by name for easy lookup
    /// </summary>
    private void InitializeTerrainDictionary()
    {
        terrainTypeDict = new Dictionary<string, TerrainTypeDataSO>();
        foreach (var terrain in terrainTypes)
        {
            terrainTypeDict[terrain.terrainName] = terrain;
        }
    }
    
    /// <summary>
    /// Regenerate the map with current parameters
    /// </summary>
    public void GenerateAndVisualize()
    {
        // Clear previous map
        ClearMapVisualization();
        
        // Generate the maps
        elevationMap = NoiseGenerator.GenerateElevationMap(mapWidth, mapHeight, elevationScale, seed);
        moistureMap = NoiseGenerator.GenerateMoistureMap(mapWidth, mapHeight, moistureScale, seed);
        terrainMap = NoiseGenerator.GenerateTerrainMap(mapWidth, mapHeight, elevationScale, moistureScale, terrainTypeDict, seed);
        
        // Create map container if needed
        if (mapContainer == null)
        {
            mapContainer = new GameObject("Map Visualization");
            mapContainer.transform.position = Vector3.zero;
            mapContainer.transform.parent = transform;
        }
        
        // Create map visualization
        mapTiles = new GameObject[mapWidth, mapHeight];
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Instantiate a tile
                GameObject tile = Instantiate(debugTilePrefab, mapContainer.transform);
                tile.transform.position = new Vector3(x, y, 0);
                tile.name = $"Tile_{x}_{y}";
                
                // Get the sprite renderer
                SpriteRenderer sr = tile.GetComponent<SpriteRenderer>();
                
                // Determine what to display based on settings
                if (showElevationMap)
                {
                    float elevation = elevationMap[x, y];
                    sr.color = new Color(elevation, elevation, elevation);
                }
                else if (showMoistureMap)
                {
                    float moisture = moistureMap[x, y];
                    sr.color = new Color(0, 0, moisture);
                }
                else if (showTerrainMap && terrainMap[x, y] != null)
                {
                    sr.color = terrainMap[x, y].baseColor;
                }
                
                // Store the tile
                mapTiles[x, y] = tile;
            }
        }
    }
    
    /// <summary>
    /// Clear the current map visualization
    /// </summary>
    private void ClearMapVisualization()
    {
        if (mapContainer != null)
        {
            // Clear all children
            foreach (Transform child in mapContainer.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    /// <summary>
    /// Regenerate the map when settings change
    /// </summary>
    public void RegenerateMap()
    {
        seed = Random.Range(0, 100000);
        GenerateAndVisualize();
    }
    
    /// <summary>
    /// Save the current map as a TerrainMapDataSO asset
    /// </summary>
    public void SaveCurrentMapToAsset()
    {
        // Create a new ScriptableObject to store the map data
        TerrainMapDataSO mapAsset = ScriptableObject.CreateInstance<TerrainMapDataSO>();
        
        // Store terrain data
        mapAsset.width = mapWidth;
        mapAsset.height = mapHeight;
        mapAsset.seed = seed;
        mapAsset.elevationScale = elevationScale;
        mapAsset.moistureScale = moistureScale;
        
        // Save the terrain type references for each cell
        mapAsset.terrainData = new string[mapWidth, mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (terrainMap[x, y] != null)
                {
                    mapAsset.terrainData[x, y] = terrainMap[x, y].terrainName;
                }
                else
                {
                    mapAsset.terrainData[x, y] = "Plains"; // Default
                }
            }
        }
        
        // Save the asset
        #if UNITY_EDITOR
        // Create directory if it doesn't exist
        string directory = "Assets/ScriptableObjects/Maps";
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        string path = $"{directory}/TerrainMap_Seed{seed}.asset";
        UnityEditor.AssetDatabase.CreateAsset(mapAsset, path);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"Map saved to {path}");
        #endif
    }
    
    /// <summary>
    /// Draw UI controls in the inspector for easy testing
    /// </summary>
    private void OnGUI()
    {
        // Only in editor or debug builds
        if (Application.isEditor || Debug.isDebugBuild)
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 350)); // Make area larger for save button
            
            GUILayout.Label("Map Visualization Controls");
            
            bool oldShowElevation = showElevationMap;
            bool oldShowMoisture = showMoistureMap;
            bool oldShowTerrain = showTerrainMap;
            
            showElevationMap = GUILayout.Toggle(showElevationMap, "Show Elevation Map");
            showMoistureMap = GUILayout.Toggle(showMoistureMap, "Show Moisture Map");
            showTerrainMap = GUILayout.Toggle(showTerrainMap, "Show Terrain Map");
            
            // Auto-disable other options
            if (showElevationMap && !oldShowElevation)
            {
                showMoistureMap = false;
                showTerrainMap = false;
            }
            else if (showMoistureMap && !oldShowMoisture)
            {
                showElevationMap = false;
                showTerrainMap = false;
            }
            else if (showTerrainMap && !oldShowTerrain)
            {
                showElevationMap = false;
                showMoistureMap = false;
            }
            
            GUILayout.Space(10);
            
            // Sliders for parameters
            GUILayout.Label($"Elevation Scale: {elevationScale}");
            elevationScale = GUILayout.HorizontalSlider(elevationScale, 1, 100);
            
            GUILayout.Label($"Moisture Scale: {moistureScale}");
            moistureScale = GUILayout.HorizontalSlider(moistureScale, 1, 100);
            
            GUILayout.Space(10);
            
            // Regenerate button
            if (GUILayout.Button("Regenerate Map"))
            {
                RegenerateMap();
            }
            
            GUILayout.Space(20);
            
            // Add save button
            if (GUILayout.Button("Save Current Map as Asset"))
            {
                SaveCurrentMapToAsset();
            }
            
            GUILayout.EndArea();
            
            // Check if visualization type changed
            if (oldShowElevation != showElevationMap || 
                oldShowMoisture != showMoistureMap || 
                oldShowTerrain != showTerrainMap)
            {
                GenerateAndVisualize();
            }
        }
    }
}
