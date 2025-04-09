using UnityEngine;
using System.Collections.Generic;
using V1.Data;

namespace V1.Utils
{ 
    /// CLASS PURPOSE:
    /// NationGenerator procedurally generates nations across a terrain map by placing seeds
    /// and expanding them into territories based on terrain type and proximity rules.
    /// It produces a MapDataSO object containing nation and region information used at runtime.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Initialize and validate terrain map data
    /// - Place initial nation seeds using distance and terrain constraints
    /// - Expand nation territories using influence rules and terrain resistance
    /// - Generate and return a MapDataSO with fully constructed nations and regions
    /// 
    /// KEY COLLABORATORS:
    /// - TerrainMapDataSO: Supplies terrain data and seed for procedural logic
    /// - MapDataSO: Output object representing the generated nations and their regions
    /// - UnityEngine.Debug: Logs progress and issues during generation
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Expansion uses a checkerboard iteration pattern for organic borders
    /// - Terrain resistance modifies influence strength per tile
    /// - Nation data includes name, color, and generated region list
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Move terrain resistance logic into a configurable data profile or ScriptableObject
    /// - Separate seed placement and expansion into strategy classes
    /// - Add more robust validation for terrain and nation parameters
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Support factions, capital placement rules, or weighted terrain preferences
    /// - Visualize generation process in editor or runtime
    /// - Enable saving and reloading of generated maps
    /// 
    public class NationGenerator
    {
        private TerrainMapDataSO terrainMap;
        private int nationCount;
        private int regionsPerNation;
        private int[,] nationMap;
        private Color[] nationColors;
        
        public NationGenerator(TerrainMapDataSO terrainMap, int nationCount, int regionsPerNation)
        {
            this.terrainMap = terrainMap;
            this.nationCount = nationCount;
            this.regionsPerNation = regionsPerNation;
            
            // Check if terrain data is valid
            if (terrainMap.terrainData == null)
            {
                Debug.LogError("TerrainMapDataSO.terrainData is null!");
                terrainMap.terrainData = new string[terrainMap.width, terrainMap.height];
                // Fill with default terrain
                for (int y = 0; y < terrainMap.height; y++)
                {
                    for (int x = 0; x < terrainMap.width; x++)
                    {
                        terrainMap.terrainData[x, y] = "Plains";
                    }
                }
            }
            
            this.nationMap = new int[terrainMap.width, terrainMap.height];
            
            // Initialize nation map to -1 (unclaimed)
            for (int y = 0; y < terrainMap.height; y++)
            {
                for (int x = 0; x < terrainMap.width; x++)
                {
                    nationMap[x, y] = -1;
                }
            }
            
            // Generate nation colors
            GenerateNationColors();
        }
        
        // Generate distinct colors for each nation
        private void GenerateNationColors() 
        {
            nationColors = new Color[nationCount];
            
            for (int i = 0; i < nationCount; i++)
            {
                float hue = (float)i / nationCount;
                float saturation = 0.7f;
                float value = 0.9f;
                nationColors[i] = Color.HSVToRGB(hue, saturation, value);
            }
        }
        
        public MapDataSO GenerateNations()
        {
            // Place nation seeds
            PlaceNationSeeds();
            
            // Expand nations from seeds
            ExpandNations();
            
            // Create MapDataSO from nation map
            return CreateMapData();
        }
        
        // Place initial nation seeds (capitals)
        // Place initial nation seeds (capitals)
        private void PlaceNationSeeds()
        {
            List<Vector2Int> nationSeeds = new List<Vector2Int>();
            System.Random prng = new System.Random(terrainMap.seed);
            
            for (int i = 0; i < nationCount; i++)
            {
                // Keep trying until we find an unoccupied space
                bool validPlacement = false;
                int attempts = 0;
                
                while (!validPlacement && attempts < 100)
                {
                    int x = prng.Next(0, terrainMap.width);
                    int y = prng.Next(0, terrainMap.height);
                    
                    // Check if this is a valid location (not water)
                    if (terrainMap.terrainData[x, y] == "Water")
                    {
                        attempts++;
                        continue;
                    }
                    
                    // Check distance from other seeds
                    bool tooClose = false;
                    foreach (var existingSeed in nationSeeds)
                    {
                        if (Vector2Int.Distance(new Vector2Int(x, y), existingSeed) < 
                            Mathf.Min(terrainMap.width, terrainMap.height) / (nationCount * 0.75f))
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
                        Debug.Log($"Placed seed for Nation {i+1} at ({x}, {y})");
                    }
                    else
                    {
                        attempts++;
                    }
                }
                
                if (attempts >= 100)
                {
                    Debug.LogWarning($"Failed to find valid placement for Nation {i+1} after 100 attempts");
                }
            }
            
            Debug.Log($"Placed {nationSeeds.Count} nation seeds");
        }

        // Expand nation territories
        private void ExpandNations()
        {
            bool changes;
            int iterations = 0;
            int maxIterations = terrainMap.width * terrainMap.height; // Safety limit
            
            do
            {
                changes = false;
                
                // Skip every other cell in a checkerboard pattern each iteration
                // This produces more natural-looking borders
                bool evenIteration = iterations % 2 == 0;
                
                for (int y = 0; y < terrainMap.height; y++)
                {
                    for (int x = 0; x < terrainMap.width; x++)
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
                                if (nx < 0 || nx >= terrainMap.width || ny < 0 || ny >= terrainMap.height)
                                    continue;
                                    
                                int neighborNation = nationMap[nx, ny];
                                if (neighborNation != -1)
                                {
                                    // Add more influence for cardinal directions than diagonals
                                    float influence = (dx == 0 || dy == 0) ? 1.5f : 1.0f;
                                    
                                    // Add terrain resistance
                                    string terrainType = terrainMap.terrainData[x, y];
                                    
                                    // Make it harder to expand into mountains and water
                                    if (terrainType == "Mountains")
                                        influence *= 0.5f;
                                    else if (terrainType == "Water")
                                        influence *= 0.2f; // Very hard to expand into water
                                    
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
                
                if (iterations % 10 == 0)
                {
                    Debug.Log($"Nation expansion iteration {iterations}, still expanding: {changes}");
                }
            } while (changes && iterations < maxIterations);
            
            Debug.Log($"Nation expansion complete after {iterations} iterations");
            
            // Count cells per nation
            int[] nationCellCounts = new int[nationCount];
            for (int y = 0; y < terrainMap.height; y++)
            {
                for (int x = 0; x < terrainMap.width; x++)
                {
                    int nationId = nationMap[x, y];
                    if (nationId >= 0 && nationId < nationCount)
                    {
                        nationCellCounts[nationId]++;
                    }
                }
            }
            
            for (int i = 0; i < nationCount; i++)
            {
                Debug.Log($"Nation {i+1} has {nationCellCounts[i]} cells");
            }
        }
        
        // Create MapDataSO from generated nation map
        private MapDataSO CreateMapData()
        {
            MapDataSO mapData = ScriptableObject.CreateInstance<MapDataSO>();
            
            // Create nation data
            mapData.nations = new MapDataSO.NationData[nationCount];
            
            // Create region lists for each nation
            List<MapDataSO.RegionData>[] nationRegions = new List<MapDataSO.RegionData>[nationCount];
            for (int i = 0; i < nationCount; i++)
            {
                nationRegions[i] = new List<MapDataSO.RegionData>();
            }
            
            // Assign regions to nations based on nationMap
            int regionCounter = 1;
            for (int y = 0; y < terrainMap.height; y++)
            {
                for (int x = 0; x < terrainMap.width; x++)
                {
                    int nationId = nationMap[x, y];
                    if (nationId >= 0 && nationId < nationCount)
                    {
                        // Create region data
                        MapDataSO.RegionData regionData = new MapDataSO.RegionData
                        {
                            regionName = "Region " + regionCounter++,
                            initialWealth = Random.Range(50, 200),
                            initialProduction = Random.Range(5, 20),
                            position = new Vector2(x, y),
                            terrainTypeName = terrainMap.terrainData[x, y]
                        };
                        
                        // Add to nation's region list
                        nationRegions[nationId].Add(regionData);
                    }
                }
            }
            
            // Assign regions to nations in the MapDataSO
            for (int i = 0; i < nationCount; i++)
            {
                mapData.nations[i] = new MapDataSO.NationData
                {
                    nationName = "Nation " + (i + 1),
                    nationColor = nationColors[i],
                    regions = nationRegions[i].ToArray()
                };
            }
            
            return mapData;
        }
    }
}