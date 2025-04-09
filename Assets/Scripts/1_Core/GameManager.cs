using UnityEngine;
using System.Collections.Generic;

/// CLASS PURPOSE:
/// GameManager serves as the central orchestration layer responsible for initializing and connecting
/// the game's core systems and models at runtime, particularly during the early Unity lifecycle stages.
///
/// CORE RESPONSIBILITIES:
/// - Load configuration data and resources (SO-based) for use during runtime
/// - Initialize map, terrain, region, and nation structures
/// - Set up core models: MapModel, NationModel, and TradeSystem
/// - Configure and instantiate supporting systems (economic systems, resource logic)
/// - Subscribe to core game events (e.g., TurnEnded) and dispatch processing logic
///
/// KEY COLLABORATORS:
/// - GameConfigurationSO, NationTemplate, TerrainTypeDataSO: ScriptableObjects for static game data
/// - MapModel, NationModel: Store game state for regions and nations
/// - TradeSystem: Manages inter-region resource movement
/// - EventBus: Facilitates loose coupling between components through event broadcasting
/// - EnhancedEconomicSystem: Optional runtime economic logic system
///
/// CURRENT ARCHITECTURE NOTES:
/// - Modular and well-isolated initialization logic through factories and helper classes
/// - Region-based segmentation aids readability and maintenance
/// - Hard references to serialized fields reduce flexibility (e.g., no DI or dynamic reloading)
/// - Logic for game lifecycle and system startup still embedded directly in MonoBehaviour
///
/// REFACTORING SUGGESTIONS:
/// - Introduce a GameLifecycleController to encapsulate start/stop/reset logic
/// - Abstract initialization flows to allow for testability or custom modes (e.g., sandbox/test runs)
/// - Replace direct component discovery (FindFirstObjectByType) with dependency injection
/// - Consider breaking apart responsibilities across separate system setup classes
///
/// EXTENSION OPPORTUNITIES:
/// - Centralized logging or analytics system to track state over time
/// - Hook into mod systems for extensibility of map, nations, or economic rules
/// - Scenario loader to dynamically apply different starting configurations
///

public class GameManager : MonoBehaviour
{
    #region Configuration References
    [Header("Game Configuration")]
    [SerializeField] private GameConfigurationSO gameConfiguration;
    
    [Header("Resource Definitions")]
    [SerializeField] private ResourceDataSO[] availableResources;
    
    [Header("Nation Templates")]
    [SerializeField] private NationTemplate[] nationTemplates;
    #endregion

    #region System References
    [Header("View References")]
    public MapView mapView;
    public RegionInfoUI regionInfoUI;
    #endregion

    #region Game Systems
    private MapModel mapModel;
    private NationModel nationModel;
    private TradeSystem tradeSystem;
    #endregion

    #region Initialization Components
    private MapGenerationFactory mapGenerationFactory;
    private GameSystemInitializer systemInitializer;
    private ResourceInitializer resourceInitializer;
    #endregion

    private EnhancedEconomicSystem enhancedEconomicSystem;

    private void Awake()
    {
        // Initialize factories and initializers
        InitializeComponents();

        // Generate map data
        MapDataSO mapData = GenerateMapData();

        // Create terrain type dictionary
        Dictionary<string, TerrainTypeDataSO> terrainTypes = CreateTerrainDictionary();

        // Initialize core game systems
        InitializeCoreGameSystems(mapData, terrainTypes);

        // Assign map data to view
        mapView.mapData = mapData;
    }

    private void Start()
    {
        // Initialize resources after other systems are set up
        InitializeResources();
        
        // Apply global settings from configuration
        ApplyGlobalSettings();

        enhancedEconomicSystem = FindFirstObjectByType<EnhancedEconomicSystem>();
        if (enhancedEconomicSystem == null)
        {
            GameObject economicSystemObj = new GameObject("EnhancedEconomicSystem");
            enhancedEconomicSystem = economicSystemObj.AddComponent<EnhancedEconomicSystem>();
            
            // Configure with model
            //enhancedEconomicSystem.economicModel = /* reference to your EnhancedEconomicModelSO */;
        }
    }

    #region Initialization Methods
    /// <summary>
    /// Initialize helper components
    /// </summary>
    private void InitializeComponents()
    {
        mapGenerationFactory = new MapGenerationFactory();
        systemInitializer = new GameSystemInitializer();
        resourceInitializer = new ResourceInitializer();
    }

    /// <summary>
    /// Generate map data based on configuration
    /// </summary>
    private MapDataSO GenerateMapData()
    {
        // Validate configuration
        if (gameConfiguration == null)
        {
            Debug.LogError("Game Configuration is missing!");
            return null;
        }

        // Generate map using configuration and terrain types
        return mapGenerationFactory.CreateMap(
            gameConfiguration, 
            mapView.availableTerrainTypes,
            nationTemplates
        );
    }

    /// <summary>
    /// Create a dictionary of terrain types for easy lookup
    /// </summary>
    private Dictionary<string, TerrainTypeDataSO> CreateTerrainDictionary()
    {
        return mapGenerationFactory.CreateTerrainDictionary(mapView.availableTerrainTypes);
    }

    /// <summary>
    /// Initialize core game systems and models
    /// </summary>
    private void InitializeCoreGameSystems(
        MapDataSO mapData, 
        Dictionary<string, TerrainTypeDataSO> terrainTypes)
    {
        // Validate inputs
        if (mapData == null)
        {
            Debug.LogError("Cannot initialize game systems: MapData is null");
            return;
        }

        // Initialize game systems
        var systemsConfig = systemInitializer.InitializeGameSystems(
            mapData, 
            terrainTypes, 
            nationTemplates  // Pass nationTemplates instead of gameConfiguration
        );

        // Store references to initialized systems
        if (systemsConfig != null)
        {
            mapModel = systemsConfig.MapModel;
            nationModel = systemsConfig.NationModel;
            tradeSystem = systemsConfig.TradeSystem;
        }
    }

    /// <summary>
    /// Initialize resources for all regions
    /// </summary>
    private void InitializeResources()
    {
        // Ensure map model exists before initializing resources
        if (mapModel == null)
        {
            Debug.LogError("Cannot initialize resources: MapModel is null");
            return;
        }

        // Initialize resources
        resourceInitializer.InitializeRegionResources(
            mapModel.GetAllRegions(), 
            availableResources
        );
    }
    
    /// <summary>
    /// Apply global settings from configuration
    /// </summary>
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
        
        // Set game speed from configuration
        Time.timeScale = gameConfiguration.gameSpeed;
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
        // Process turn for map model
        mapModel?.ProcessTurn();
        
        // Process turn for nation model
        nationModel?.ProcessTurn();
        
        Debug.Log("Turn ended, region and nation processing complete");
    }
    #endregion

    public MapModel GetMapModel()
    {
        return mapModel;
    }

    #region Public Access Methods
    /// <summary>
    /// Get a specific region by name
    /// </summary>
    public RegionEntity GetRegion(string regionName)
    {
        return mapModel?.GetRegion(regionName);
    }

    /// <summary>
    /// Get all regions in the game
    /// </summary>
    public Dictionary<string, RegionEntity> GetAllRegions()
    {
        return mapModel?.GetAllRegions();
    }

    /// <summary>
    /// Select a specific region
    /// </summary>
    public void SelectRegion(string regionName)
    {
        mapModel?.SelectRegion(regionName);
    }

    /// <summary>
    /// Get the current map data
    /// </summary>
    public MapDataSO GetMapData()
    {
        return mapModel?.GetMapData();
    }
    
    /// <summary>
    /// Get a nation by name
    /// </summary>
    public NationEntity GetNation(string nationName)
    {
        return nationModel?.GetNation(nationName);
    }

    /// <summary>
    /// Select a specific nation
    /// </summary>
    public void SelectNation(string nationName)
    {
        nationModel?.SelectNation(nationName);
    }

    /// <summary>
    /// Get the currently selected nation
    /// </summary>
    public NationEntity GetSelectedNation()
    {
        return nationModel?.GetSelectedNation();
    }

    /// <summary>
    /// Get all nations in the game
    /// </summary>
    public Dictionary<string, NationEntity> GetAllNations()
    {
        return nationModel?.GetAllNations();
    }
    
    /// <summary>
    /// Register a new region with the appropriate nation
    /// </summary>
    public void RegisterNewRegion(RegionEntity region)
    {
        // Register with nation model
        nationModel?.RegisterRegion(region);
    }

    public EnhancedEconomicSystem GetEnhancedEconomicSystem() {
    return enhancedEconomicSystem;
    }
    #endregion
}