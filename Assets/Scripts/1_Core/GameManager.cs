// GameManager.cs - Updated to use gameConfiguration
using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region References
    public MapView mapView;
    public RegionInfoUI regionInfoUI;
    [SerializeField] private GameConfigurationSO gameConfiguration;
    #endregion

    #region Models
    private MapModel mapModel;
    private NationModel nationModel;
    private TradeSystem tradeSystem;
    #endregion

    #region Content Data
    [Header("Nation Templates")]
    public NationTemplate[] nationTemplates;

    [Header("Resources")]
    public ResourceDataSO[] availableResources;
    #endregion

    #region Initialization
    private void Awake()
    {
        // Check if gameConfiguration is assigned
        if (gameConfiguration == null)
        {
            Debug.LogError("GameConfiguration is not assigned! Using default values.");
        }

        // Generate or use predefined map
        MapDataSO mapData;
        
        if (gameConfiguration.useProceduralMap)
        {
            // Create map generator
            MapGenerator generator;
            
            if (gameConfiguration.useSavedTerrainMap && gameConfiguration.savedTerrainMap != null)
            {
                // Use the saved terrain map parameters
                generator = new MapGenerator(
                    gameConfiguration.savedTerrainMap.width,
                    gameConfiguration.savedTerrainMap.height,
                    gameConfiguration.nationCount,
                    gameConfiguration.regionsPerNation,
                    gameConfiguration.savedTerrainMap.seed
                );
                generator.SetTerrainParameters(
                    gameConfiguration.savedTerrainMap.elevationScale,
                    gameConfiguration.savedTerrainMap.moistureScale
                );
            }
            else
            {
                // Generate with configured parameters
                Debug.Log("Generating map with configured terrain parameters");
                int randomSeed = gameConfiguration.useRandomSeed ? 
                    Random.Range(0, 100000) : gameConfiguration.mapSeed;
                    
                generator = new MapGenerator(
                    gameConfiguration.mapWidth, 
                    gameConfiguration.mapHeight, 
                    gameConfiguration.nationCount, 
                    gameConfiguration.regionsPerNation, 
                    randomSeed);
                    
                generator.SetTerrainParameters(
                    gameConfiguration.elevationScale, 
                    gameConfiguration.moistureScale);
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
            }

            // Generate the map
            mapData = generator.GenerateMap();
        }
        else
        {
            // Use predefined map
            mapData = gameConfiguration.predefinedMapData;
            
            if (mapData == null)
            {
                Debug.LogError("No predefined map data assigned! Falling back to procedural generation.");
                
                // Fall back to a basic procedural generation
                MapGenerator fallbackGenerator = new MapGenerator(
                    10, 10, 3, 5, Random.Range(0, 100000));
                    
                mapData = fallbackGenerator.GenerateMap();
            }
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

        // Find or create TradeSystem with configured settings
        tradeSystem = FindFirstObjectByType<TradeSystem>();
        if (tradeSystem == null)
        {
            GameObject tradeSystemObj = new GameObject("TradeSystem");
            tradeSystem = tradeSystemObj.AddComponent<TradeSystem>();
            
            // Apply trade system configuration
            if (gameConfiguration.tradeSystemConfig != null)
            {
                tradeSystem.tradeRadius = gameConfiguration.tradeSystemConfig.baseTradeRadius;
            }
        }
        
        // Set game speed from configuration
        Time.timeScale = gameConfiguration.gameSpeed;
    }

    private void Start()
    {
        // Initialize resources after everything else is set up
        InitializeResources();
        
        // Apply any global game settings from configuration
        ApplyGlobalSettings();
    }
    
    private void ApplyGlobalSettings()
    {
        // Apply global economic modifiers if configured
        if (gameConfiguration.economicModifiers != null)
        {
            foreach (var region in mapModel.GetAllRegions().Values)
            {
                // Apply global production modifier
                if (region.economy != null)
                {
                    region.economy.productionEfficiency *= gameConfiguration.economicModifiers.globalProductionModifier;
                }
            }
        }
        
    }

    private void RegisterRegionsWithNations()
    {
        foreach (var regionEntry in mapModel.GetAllRegions())
        {
            RegionEntity region = regionEntry.Value;
            nationModel.RegisterRegion(region);
        }
    }

    private void InitializeResources()
    {
        if (availableResources == null || availableResources.Length == 0)
        {
            Debug.LogWarning("No resource definitions assigned to GameManager!");
            return;
        }
        
        // Load resources into all regions
        foreach (var region in mapModel.GetAllRegions().Values)
        {
            if (region.resources != null)
            {
                region.resources.LoadResourceDefinitions(availableResources);
                region.productionComponent.ActivateRecipe("Basic Iron Smelting");
            }
        }

        // Manually create some deficits and surpluses for testing
        var regions = mapModel.GetAllRegions();
        if (regions.Count >= 2)
        {
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
    #endregion

    #region Event Handling
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
    #endregion

    #region Region Management
    public void SelectRegion(string regionName)
    {
        mapModel.SelectRegion(regionName);
    }
    
    public RegionEntity GetRegion(string regionName)
    {
        return mapModel.GetRegion(regionName);
    }

    public Dictionary<string, RegionEntity> GetAllRegions()
    {
        return mapModel.GetAllRegions();
    }
    
    // For registering new regions if created during gameplay
    public void RegisterNewRegion(RegionEntity region)
    {
        // Add to map model if needed
        // mapModel.AddRegion(region); - Would need to implement this method
        
        // Register with nation model
        nationModel.RegisterRegion(region);
    }
    
    public MapDataSO GetMapData()
    {
        return mapModel.GetMapData();
    }
    #endregion

    #region Nation Management
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
    #endregion
}