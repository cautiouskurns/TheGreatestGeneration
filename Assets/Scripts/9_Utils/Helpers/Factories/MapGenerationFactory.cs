/// CLASS PURPOSE:
/// MapGenerationFactory is a utility class responsible for orchestrating map creation
/// using either procedural generation or predefined data based on game configuration.
///
/// CORE RESPONSIBILITIES:
/// - Determine whether to use procedural or predefined map data
/// - Instantiate and configure a MapGenerator with appropriate settings
/// - Generate map data using terrain and nation configuration
/// - Provide terrain type dictionary for use in generation systems
///
/// KEY COLLABORATORS:
/// - GameConfigurationSO: Supplies user-defined map parameters and flags
/// - TerrainTypeDataSO: Describes terrain attributes used in generation
/// - NationTemplate: Provides national characteristics for region assignment
/// - MapGenerator: Core class responsible for procedural map construction
///
/// CURRENT ARCHITECTURE NOTES:
/// - Fallback to procedural generation is automatic if predefined map is missing
/// - Encapsulates both generator creation and configuration in private helpers
///
/// REFACTORING SUGGESTIONS:
/// - Abstract generator configuration into a strategy or configuration profile
/// - Move terrain dictionary logic into a shared terrain utility class
///
/// EXTENSION OPPORTUNITIES:
/// - Add support for multiple generator types or custom terrain distributions
/// - Allow editor-based validation or visualization of config parameters
/// - Integrate biome rules or faction-aware generation modifiers
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Factory class responsible for creating and configuring map generators
/// </summary>
public class MapGenerationFactory
{
    /// <summary>
    /// Creates a map based on game configuration
    /// </summary>
    /// <param name="config">Game configuration settings</param>
    /// <param name="availableTerrainTypes">Available terrain types</param>
    /// <param name="nationTemplates">Nation templates for generation</param>
    /// <returns>Generated map data</returns>
    public MapDataSO CreateMap(
        GameConfigurationSO config,
        TerrainTypeDataSO[] availableTerrainTypes,
        NationTemplate[] nationTemplates)
    {
        // Check if we should use predefined map
        if (!config.useProceduralMap)
        {
            // Use predefined map if available
            if (config.predefinedMapData != null)
            {
                return config.predefinedMapData;
            }
            
            Debug.LogError("No predefined map data assigned! Falling back to procedural generation.");
        }
        
        // Create a map generator
        MapGenerator generator = CreateMapGenerator(config);
        
        // Configure the generator
        ConfigureGenerator(generator, config, availableTerrainTypes, nationTemplates);
        
        // Generate and return the map
        return generator.GenerateMap();
    }
    
    /// <summary>
    /// Creates an appropriate map generator based on configuration
    /// </summary>
    private MapGenerator CreateMapGenerator(GameConfigurationSO config)
    {
        // Determine generator parameters
        int width, height, seed;
        
        if (config.useSavedTerrainMap && config.savedTerrainMap != null)
        {
            width = config.savedTerrainMap.width;
            height = config.savedTerrainMap.height;
            seed = config.savedTerrainMap.seed;
        }
        else
        {
            width = config.mapWidth;
            height = config.mapHeight;
            seed = config.useRandomSeed ? Random.Range(0, 100000) : config.mapSeed;
        }
        
        // Create the generator
        return new MapGenerator(
            width, 
            height, 
            config.nationCount, 
            config.regionsPerNation, 
            seed);
    }
    
    /// <summary>
    /// Configures the generator with appropriate settings
    /// </summary>
    private void ConfigureGenerator(
        MapGenerator generator, 
        GameConfigurationSO config,
        TerrainTypeDataSO[] availableTerrainTypes,
        NationTemplate[] nationTemplates)
    {
        // Set terrain parameters
        if (config.useSavedTerrainMap && config.savedTerrainMap != null)
        {
            generator.SetTerrainParameters(
                config.savedTerrainMap.elevationScale,
                config.savedTerrainMap.moistureScale
            );
        }
        else
        {
            generator.SetTerrainParameters(
                config.elevationScale,
                config.moistureScale
            );
        }
        
        // Set terrain types
        Dictionary<string, TerrainTypeDataSO> terrainTypesDict = CreateTerrainDictionary(availableTerrainTypes);
        if (terrainTypesDict.Count > 0)
        {
            generator.SetTerrainTypes(terrainTypesDict);
        }
        
        // Set nation templates if available
        if (nationTemplates != null && nationTemplates.Length > 0)
        {
            generator.SetNationTemplates(nationTemplates);
        }
    }
    
    /// <summary>
    /// Creates a dictionary of terrain types for easy lookup
    /// </summary>
    public Dictionary<string, TerrainTypeDataSO> CreateTerrainDictionary(TerrainTypeDataSO[] terrainTypes)
    {
        Dictionary<string, TerrainTypeDataSO> terrainTypesDict = new Dictionary<string, TerrainTypeDataSO>();
        
        if (terrainTypes != null)
        {
            foreach (var terrain in terrainTypes)
            {
                if (terrain != null)
                {
                    terrainTypesDict[terrain.terrainName] = terrain;
                }
            }
        }
        
        return terrainTypesDict;
    }
}