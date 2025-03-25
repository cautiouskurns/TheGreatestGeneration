// GameManager.cs - Updated version with NationModel integration
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
    private NationModel nationModel;

    [Header("Nation Templates")]
    public NationTemplate[] nationTemplates;

    [Header("Resources")]
    public ResourceDataSO[] availableResources;

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

        // Initialize MapModel
        mapModel = new MapModel(mapData, terrainTypesDict);
        
        // Initialize NationModel with the same map data
        nationModel = new NationModel(mapData);
        
        // Register all regions with the NationModel
        RegisterRegionsWithNations();
        
        Debug.Log("GameManager initialized with " + 
                  (useProceduralMap ? "procedurally generated" : "predefined") + 
                  " map and nation model");
    }

    private void Start()
    {
        // Initialize resources after everything else is set up
        InitializeResources();
    }

    // Register existing regions with nations
    private void RegisterRegionsWithNations()
    {
        foreach (var regionEntry in mapModel.GetAllRegions())
        {
            RegionEntity region = regionEntry.Value;
            nationModel.RegisterRegion(region);
        }
        
        Debug.Log("Registered all regions with their nations");
    }

    private void InitializeResources()
    {
        if (availableResources == null || availableResources.Length == 0)
        {
            Debug.LogWarning("No resource definitions assigned to GameManager!");
            return;
        }
        
        Debug.Log($"Initializing {availableResources.Length} resource types for all regions");
        
        // Load resources into all regions
        foreach (var region in mapModel.GetAllRegions().Values)
        {
            if (region.resources != null)
            {
                region.resources.LoadResourceDefinitions(availableResources);
                region.productionComponent.ActivateRecipe("Basic Iron Smelting");

            }
        }
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
        // Process turn for all regions first
        mapModel.ProcessTurn();
        
        // Then update nation-level data
        nationModel.ProcessTurn();
        
        Debug.Log("Turn ended, region and nation processing complete");
    }
    
    public void SelectRegion(string regionName)
    {
        mapModel.SelectRegion(regionName);
    }
    
    public RegionEntity GetRegion(string regionName)
    {
        return mapModel.GetRegion(regionName);
    }

    public MapDataSO GetMapData()
    {
        return mapModel.GetMapData();
    }
    
    // Nation-related methods
    public NationEntity GetNation(string nationName)
    {
        return nationModel.GetNation(nationName);
    }

    public void SelectNation(string nationName)
    {
        nationModel.SelectNation(nationName);
    }

    public NationEntity GetSelectedNation()
    {
        return nationModel.GetSelectedNation();
    }

    public Dictionary<string, NationEntity> GetAllNations()
    {
        return nationModel.GetAllNations();
    }
    
    // For registering new regions if created during gameplay
    public void RegisterNewRegion(RegionEntity region)
    {
        // Add to map model if needed
        // mapModel.AddRegion(region); - Would need to implement this method
        
        // Register with nation model
        nationModel.RegisterRegion(region);
    }
}