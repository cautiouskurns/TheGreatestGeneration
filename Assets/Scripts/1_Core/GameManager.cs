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

    [Header("Nation Templates")]
    public NationTemplate[] nationTemplates;

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
             // Set nation templates if available
            if (nationTemplates != null && nationTemplates.Length > 0)
            {
                generator.SetNationTemplates(nationTemplates);
                Debug.Log($"Using {nationTemplates.Length} nation templates for map generation");
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
        
        // Initialize model with the map data and terrain types
        Dictionary<string, TerrainTypeDataSO> terrainTypesDict = new Dictionary<string, TerrainTypeDataSO>();
        if (mapView.availableTerrainTypes != null)
        {
            foreach (var terrain in mapView.availableTerrainTypes)
            {
                if (terrain != null)
                {
                    terrainTypesDict[terrain.terrainName] = terrain;
                }
            }
        }

        mapModel = new MapModel(mapData, terrainTypesDict);
        
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