using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the procedural generation of game maps with terrain and nations
/// </summary>
public class MapGenerator
{
    // Map dimensions
    private int width;
    private int height;
    private int nationCount;
    private int regionsPerNation;
    
    // Generation parameters
    private float elevationScale = 30f;
    private float moistureScale = 50f;
    private int seed;
    
    // Generated data
    private TerrainTypeDataSO[,] terrainMap;
    private int[,] nationMap; // Stores nation ID for each cell
    private Color[] nationColors;
    private Dictionary<string, TerrainTypeDataSO> terrainTypes;
    private NationTemplate[] nationTemplates;

    /// <summary>
    /// Creates a new map generator with the specified parameters
    /// </summary>
    public MapGenerator(int width, int height, int nationCount, int regionsPerNation, int seed = 0)
    {
        this.width = width;
        this.height = height;
        this.nationCount = nationCount;
        this.regionsPerNation = regionsPerNation;
        this.seed = seed;
        
        // Generate distinct colors for nations
        GenerateNationColors();
        
        // Initialize terrain map
        terrainMap = new TerrainTypeDataSO[width, height];
        nationMap = new int[width, height];
    }

    /// <summary>
    /// Sets terrain types to use for generation
    /// </summary>
    /// <param name="terrainTypes">Dictionary of terrain type SOs</param>
    public void SetTerrainTypes(Dictionary<string, TerrainTypeDataSO> terrainTypes)
    {
        this.terrainTypes = terrainTypes;
    }

    /// <summary>
    /// Sets nation templates for more controlled generation
    /// </summary>
    public void SetNationTemplates(NationTemplate[] templates)
    {
        this.nationTemplates = templates;
        this.nationCount = templates.Length;
        
        // Regenerate colors based on templates
        nationColors = new Color[nationCount];
        for (int i = 0; i < nationCount; i++)
        {
            nationColors[i] = templates[i].color;
        }
    }

    /// <summary>
    /// Adjust generation parameters for terrain
    /// </summary>
    public void SetTerrainParameters(float elevationScale, float moistureScale)
    {
        this.elevationScale = elevationScale;
        this.moistureScale = moistureScale;
    }

    /// <summary>
    /// Generates nation colors with good visual distinction
    /// </summary>
    private void GenerateNationColors()
    {
        nationColors = new Color[nationCount];
        
        // Use the HSV color model to generate evenly distributed hues
        for (int i = 0; i < nationCount; i++)
        {
            // Distribute hue evenly around the color wheel (0 to 1)
            float hue = (float)i / nationCount;
            
            // Use a consistent saturation and value for all nations
            float saturation = 0.7f;  // Fairly saturated but not too extreme
            float value = 0.9f;       // Bright but not blindingly so
            
            // Convert HSV to RGB
            Color color = Color.HSVToRGB(hue, saturation, value);
            
            nationColors[i] = color;
        }
    }

    /// <summary>
    /// Generates the complete map with terrain and nations
    /// </summary>
    public MapDataSO GenerateMap()
    {
        // First, generate terrain if we have terrain types
        if (terrainTypes != null && terrainTypes.Count >= 5) // Make sure we have the basic types
        {
            GenerateTerrainMap();
        }
        
        // Then, generate nations and regions
        GenerateNations();
        
        // Create a new MapDataSO instance
        MapDataSO mapData = ScriptableObject.CreateInstance<MapDataSO>();
        
        // Setup nations
        mapData.nations = new MapDataSO.NationData[nationCount];
        
        // Create regions list for each nation
        List<MapDataSO.RegionData>[] nationRegions = new List<MapDataSO.RegionData>[nationCount];
        for (int i = 0; i < nationCount; i++)
        {
            nationRegions[i] = new List<MapDataSO.RegionData>();
        }
        
        // Assign regions to nations based on nationMap
        int regionCounter = 1;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int nationId = nationMap[x, y];
                if (nationId >= 0 && nationId < nationCount) // Valid nation ID
                {
                    TerrainTypeDataSO terrain = terrainMap[x, y];
                    
                    // Base wealth and production values
                    int baseWealth = Random.Range(50, 200);
                    int baseProduction = Random.Range(5, 20);
                    
                    // Apply template modifiers if available
                    if (nationTemplates != null && nationId < nationTemplates.Length)
                    {
                        NationTemplate template = nationTemplates[nationId];
                        baseWealth = Mathf.RoundToInt(baseWealth * template.wealthMultiplier);
                        baseProduction = Mathf.RoundToInt(baseProduction * template.productionMultiplier);
                    }
                    
                    // Create region data with position information
                    MapDataSO.RegionData regionData = new MapDataSO.RegionData
                    {
                        regionName = "Region " + regionCounter++,
                        initialWealth = baseWealth,
                        initialProduction = baseProduction,
                        position = new Vector2(x, y), // Add position to store the grid location
                        terrainTypeName = terrain != null ? terrain.terrainName : "Plains" // Save terrain type name
                    };
                    
                    // Add to nation's region list
                    nationRegions[nationId].Add(regionData);
                }
            }
        }
        
        // Assign regions to nations in the MapDataSO
        for (int i = 0; i < nationCount; i++)
        {
            string nationName = "Nation " + (i + 1);
            
            // Use template name if available
            if (nationTemplates != null && i < nationTemplates.Length)
            {
                nationName = nationTemplates[i].name;
            }
            
            mapData.nations[i] = new MapDataSO.NationData
            {
                nationName = nationName,
                nationColor = nationColors[i],
                regions = nationRegions[i].ToArray() // Use all regions, no limiting
            };
        }
        
        return mapData;
    }

    /// <summary>
    /// Generates the terrain map using noise
    /// </summary>
    private void GenerateTerrainMap()
    {
        // Generate terrain using noise
        terrainMap = NoiseGenerator.GenerateTerrainMap(
            width, height,
            elevationScale,
            moistureScale,
            terrainTypes,
            seed
        );
    }

    /// <summary>
    /// Generates nation territories using templates if available, otherwise cellular automata
    /// </summary>
    private void GenerateNations()
    {
        // Initialize nationMap
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                nationMap[x, y] = -1; // -1 means unclaimed
            }
        }
        
        // Place nation seeds
        List<Vector2Int> nationSeeds = new List<Vector2Int>();
        System.Random prng = new System.Random(seed);
        
        // If templates are available, use them
        if (nationTemplates != null && nationTemplates.Length > 0)
        {
            for (int i = 0; i < nationTemplates.Length; i++)
            {
                NationTemplate template = nationTemplates[i];
                
                // Convert normalized position to grid coordinates
                int seedX = Mathf.RoundToInt(template.centerPosition.x * (width - 1));
                int seedY = Mathf.RoundToInt(template.centerPosition.y * (height - 1));
                
                // Make sure the position is valid
                bool validPlacement = false;
                int attempts = 0;
                
                while (!validPlacement && attempts < 100)
                {
                    // Add some randomness
                    int x = seedX + Random.Range(-3, 4);
                    int y = seedY + Random.Range(-3, 4);
                    
                    x = Mathf.Clamp(x, 0, width - 1);
                    y = Mathf.Clamp(y, 0, height - 1);
                    
                    // Check terrain constraints
                    bool terrainValid = true;
                    if (terrainMap != null && terrainMap[x, y] != null)
                    {
                        if (template.isLandlocked && terrainMap[x, y].terrainName == "Water")
                        {
                            terrainValid = false;
                        }
                    }
                    
                    if (terrainValid)
                    {
                        nationMap[x, y] = i;
                        nationSeeds.Add(new Vector2Int(x, y));
                        validPlacement = true;
//                        Debug.Log($"Placed seed for nation {template.name} at ({x}, {y})");
                    }
                    else
                    {
                        attempts++;
                    }
                }
            }
            
            // Expand nations with template constraints
            ExpandNationsWithTemplates();
        }
        else
        {
            // Fall back to original random seed placement
            for (int i = 0; i < nationCount; i++)
            {
                // Keep trying until we find an unoccupied space
                bool validPlacement = false;
                int attempts = 0;
                
                while (!validPlacement && attempts < 100)
                {
                    int x = prng.Next(0, width);
                    int y = prng.Next(0, height);
                    
                    // Check if this is a valid location (not water)
                    if (terrainMap != null && terrainMap[x, y] != null && terrainMap[x, y].terrainName == "Water")
                    {
                        attempts++;
                        continue;
                    }
                    
                    // Check distance from other seeds
                    bool tooClose = false;
                    foreach (var existingSeed in nationSeeds)
                    {
                        if (Vector2Int.Distance(new Vector2Int(x, y), existingSeed) < Mathf.Min(width, height) / (nationCount * 0.75f))
                        {
                            tooClose = true;
                            break;
                        }
                    }
                    
                    if (!tooClose)
                    {
                        nationMap[x, y] = i;
                        nationSeeds.Add(new Vector2Int(x, y));
                        validPlacement = true;
                    }
                    else
                    {
                        attempts++;
                    }
                }
            }
            
            // Expand nations using original method
            ExpandNations();
        }
    }

    /// <summary>
    /// Expands nation territories from their seed points using original algorithm
    /// </summary>
    private void ExpandNations()
    {
        bool changes;
        int iterations = 0;
        int maxIterations = width * height; // Safety limit
        
        do
        {
            changes = false;
            
            // Skip every other cell in a checkerboard pattern each iteration
            bool evenIteration = iterations % 2 == 0;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Skip if this is occupied already
                    if (nationMap[x, y] != -1)
                        continue;
                        
                    // Skip checkerboard pattern
                    if ((x + y) % 2 == (evenIteration ? 1 : 0))
                        continue;
                    
                    // Check neighbors
                    List<int> neighborNations = new List<int>();
                    float[] nationInfluence = new float[nationCount];
                    
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue; // Skip self
                            
                            int nx = x + dx;
                            int ny = y + dy;
                            
                            // Check bounds
                            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                                continue;
                                
                            int neighborNation = nationMap[nx, ny];
                            if (neighborNation != -1)
                            {
                                // Add more influence for cardinal directions than diagonals
                                float influence = (dx == 0 || dy == 0) ? 1.5f : 1.0f;
                                
                                // Add terrain resistance
                                if (terrainMap != null && terrainMap[x, y] != null)
                                {
                                    // Make it harder to expand into mountains and water
                                    if (terrainMap[x, y].terrainName == "Mountains")
                                        influence *= 0.5f;
                                    else if (terrainMap[x, y].terrainName == "Water")
                                        influence *= 0.2f; // Very hard to expand into water
                                }
                                
                                nationInfluence[neighborNation] += influence;
                                
                                if (!neighborNations.Contains(neighborNation))
                                    neighborNations.Add(neighborNation);
                            }
                        }
                    }
                    
                    // If we have neighbors, pick the most influential one
                    if (neighborNations.Count > 0)
                    {
                        int strongestNation = 0;
                        float strongestInfluence = 0;
                        
                        for (int i = 0; i < nationCount; i++)
                        {
                            if (nationInfluence[i] > strongestInfluence)
                            {
                                strongestInfluence = nationInfluence[i];
                                strongestNation = i;
                            }
                        }
                        
                        nationMap[x, y] = strongestNation;
                        changes = true;
                    }
                }
            }
            
            iterations++;
        } while (changes && iterations < maxIterations);
    }

    /// <summary>
    /// Expands nations using template-specific constraints and preferences
    /// </summary>
    private void ExpandNationsWithTemplates()
    {
        bool changes;
        int iterations = 0;
        int maxIterations = width * height; // Safety limit
        
        do
        {
            changes = false;
            
            // Skip every other cell in a checkerboard pattern each iteration
            bool evenIteration = iterations % 2 == 0;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Skip if this is occupied already
                    if (nationMap[x, y] != -1)
                        continue;
                        
                    // Skip checkerboard pattern
                    if ((x + y) % 2 == (evenIteration ? 1 : 0))
                        continue;
                    
                    // Check neighbors
                    List<int> neighborNations = new List<int>();
                    float[] nationInfluence = new float[nationCount];
                    
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            if (dx == 0 && dy == 0) continue; // Skip self
                            
                            int nx = x + dx;
                            int ny = y + dy;
                            
                            // Check bounds
                            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                                continue;
                                
                            int neighborNation = nationMap[nx, ny];
                            if (neighborNation != -1)
                            {
                                // Base influence
                                float influence = (dx == 0 || dy == 0) ? 1.5f : 1.0f;
                                
                                // Apply template constraints if available
                                if (nationTemplates != null && neighborNation < nationTemplates.Length)
                                {
                                    NationTemplate template = nationTemplates[neighborNation];
                                    
                                    // Distance from nation center
                                    Vector2 center = new Vector2(
                                        template.centerPosition.x * width,
                                        template.centerPosition.y * height
                                    );
                                    
                                    float distFactor = Vector2.Distance(new Vector2(x, y), center) / 
                                                      (template.expansionRadius * Mathf.Max(width, height));
                                    
                                    // Reduce influence as we get farther from center
                                    influence *= Mathf.Lerp(1.0f, 0.2f, Mathf.Clamp01(distFactor));
                                    
                                    // Apply terrain preferences
                                    if (terrainMap != null && terrainMap[x, y] != null)
                                    {
                                        string terrainType = terrainMap[x, y].terrainName;
                                        
                                        if (template.isLandlocked && terrainType == "Water")
                                        {
                                            influence *= 0.1f; // Very hard to expand into water
                                        }
                                        
                                        if (template.isMountainous && terrainType == "Mountains")
                                        {
                                            influence *= 1.5f; // More likely to expand into mountains
                                        }
                                        
                                        // Apply terrain type preferences
                                        if (terrainType == "Forest")
                                        {
                                            influence *= 0.5f + template.forestPreference;
                                        }
                                        else if (terrainType == "Desert")
                                        {
                                            influence *= 0.5f + template.desertPreference;
                                        }
                                        else if (terrainType == "Plains")
                                        {
                                            influence *= 0.5f + template.plainsPreference;
                                        }
                                    }
                                }
                                else
                                {
                                    // Apply default terrain influences
                                    if (terrainMap != null && terrainMap[x, y] != null)
                                    {
                                        // Make it harder to expand into mountains and water
                                        if (terrainMap[x, y].terrainName == "Mountains")
                                            influence *= 0.5f;
                                        else if (terrainMap[x, y].terrainName == "Water")
                                            influence *= 0.2f;
                                    }
                                }
                                
                                nationInfluence[neighborNation] += influence;
                                
                                if (!neighborNations.Contains(neighborNation))
                                    neighborNations.Add(neighborNation);
                            }
                        }
                    }
                    
                    // If we have neighbors, pick the most influential one
                    if (neighborNations.Count > 0)
                    {
                        int strongestNation = 0;
                        float strongestInfluence = 0;
                        
                        for (int i = 0; i < nationCount; i++)
                        {
                            if (nationInfluence[i] > strongestInfluence)
                            {
                                strongestInfluence = nationInfluence[i];
                                strongestNation = i;
                            }
                        }
                        
                        nationMap[x, y] = strongestNation;
                        changes = true;
                    }
                }
            }
            
            iterations++;
        } while (changes && iterations < maxIterations);
    }
}
