// GameManager.cs - Updated version
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public MapView mapView;
    public RegionInfoUI regionInfoUI;

    [Header("Map Generation")]
    public bool useProceduralMap = true;
    public MapDataSO predefinedMapData;
    
    [Header("Terrain Generation")]
    public bool useSavedTerrainMap = true;
    public TerrainMapDataSO savedTerrainMap;
    
    [Header("Procedural Map Settings")]
    public int mapWidth = 10;
    public int mapHeight = 10;
    public int nationCount = 3;
    public int regionsPerNation = 5;
    
    private MapModel mapModel;

    private void Awake()
    {
        // Generate or use predefined map
        MapDataSO mapData;
        
        if (useProceduralMap)
        {
            // Create map generator
            MapGenerator generator;
            
            if (useSavedTerrainMap && savedTerrainMap != null)
            {
                // Use the saved terrain map parameters
                Debug.Log($"Using saved terrain map (Seed: {savedTerrainMap.seed})");
                generator = new MapGenerator(
                    savedTerrainMap.width,
                    savedTerrainMap.height,
                    nationCount,
                    regionsPerNation,
                    savedTerrainMap.seed
                );
                generator.SetTerrainParameters(
                    savedTerrainMap.elevationScale,
                    savedTerrainMap.moistureScale
                );
            }
            else
            {
                // Generate with random parameters
                Debug.Log("Generating map with random terrain parameters");
                int randomSeed = Random.Range(0, 100000);
                generator = new MapGenerator(mapWidth, mapHeight, nationCount, regionsPerNation, randomSeed);
                generator.SetTerrainParameters(30f, 50f); // Default values
            }
            
            // Set terrain types
            if (mapView.availableTerrainTypes != null && mapView.availableTerrainTypes.Length > 0)
            {
                Dictionary<string, TerrainTypeDataSO> terrainTypes = new Dictionary<string, TerrainTypeDataSO>();
                foreach (var terrain in mapView.availableTerrainTypes)
                {
                    if (terrain != null)
                    {
                        terrainTypes[terrain.terrainName] = terrain;
                    }
                }
                
                if (terrainTypes.Count > 0)
                {
                    generator.SetTerrainTypes(terrainTypes);
                }
            }
            
            // Generate the map
            mapData = generator.GenerateMap();
        }
        else
        {
            // Use predefined map
            mapData = predefinedMapData;
        }
        
        // Assign generated map data to MapView
        mapView.mapData = mapData;
        
        // Initialize model with the map data
        mapModel = new MapModel(mapData);
        
        Debug.Log("GameManager initialized with " + 
                  (useProceduralMap ? "procedurally generated" : "predefined") + 
                  " map");
    }

    private void OnEnable()
    {
        EventBus.Subscribe("TurnEnded", OnTurnEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
    }

    private void OnTurnEnded(object _)
    {
        mapModel.ProcessTurn();
        Debug.Log("Turn ended, processing complete");
    }
    
    public void SelectRegion(string regionName)
    {
        mapModel.SelectRegion(regionName);
    }
    
    public RegionEntity GetRegion(string regionName)
    {
        return mapModel.GetRegion(regionName);
    }
}