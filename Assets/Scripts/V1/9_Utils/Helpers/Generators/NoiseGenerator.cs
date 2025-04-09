using UnityEngine;
using System.Collections.Generic;
using V1.Data;

namespace V1.Utils
{  
    /// <summary>
    /// Utility class for generating various types of noise maps for procedural terrain generation
    /// </summary>
    public static class NoiseGenerator
    {
        /// <summary>
        /// Generates a Perlin noise map with the specified parameters
        /// </summary>
        /// <param name="width">Width of the noise map</param>
        /// <param name="height">Height of the noise map</param>
        /// <param name="scale">Scale of the noise (larger values = more zoomed in)</param>
        /// <param name="octaves">Number of noise layers to combine (more = more detailed)</param>
        /// <param name="persistence">How much each octave contributes (0-1)</param>
        /// <param name="lacunarity">How much detail is added at each octave (typically 2)</param>
        /// <param name="seed">Random seed for reproducible noise</param>
        /// <param name="offset">Offset position in the noise field</param>
        /// <returns>2D float array with values between 0 and 1</returns>
        public static float[,] GenerateNoiseMap(int width, int height, float scale, int octaves = 4, 
                                            float persistence = 0.5f, float lacunarity = 2f,
                                            int seed = 0, Vector2 offset = new Vector2())
        {
            float[,] noiseMap = new float[width, height];
            
            // Prevent division by zero
            if (scale <= 0)
                scale = 0.0001f;
            
            // Use seed to create reproducible noise
            System.Random prng = new System.Random(seed);
            Vector2[] octaveOffsets = new Vector2[octaves];
            
            for (int i = 0; i < octaves; i++)
            {
                float offsetX = prng.Next(-100000, 100000) + offset.x;
                float offsetY = prng.Next(-100000, 100000) + offset.y;
                
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }
            
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;
            
            // Calculate half width and height for zooming in on center of the map
            float halfWidth = width / 2f;
            float halfHeight = height / 2f;
            
            // Generate noise map
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;
                    
                    // Calculate combined noise from multiple octaves
                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                        
                        // Use Unity's Mathf.PerlinNoise and transform it to range -1 to 1
                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        
                        noiseHeight += perlinValue * amplitude;
                        
                        // Adjustments for next octave
                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }
                    
                    // Track min and max noise values
                    if (noiseHeight > maxNoiseHeight)
                        maxNoiseHeight = noiseHeight;
                    else if (noiseHeight < minNoiseHeight)
                        minNoiseHeight = noiseHeight;
                    
                    noiseMap[x, y] = noiseHeight;
                }
            }
            
            // Normalize values to range 0-1
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }
            
            return noiseMap;
        }
        
        /// <summary>
        /// Generates an elevation map suitable for terrain height
        /// </summary>
        /// <param name="width">Width of the map</param>
        /// <param name="height">Height of the map</param>
        /// <param name="elevationScale">Base scale of elevation features</param>
        /// <param name="seed">Random seed</param>
        /// <returns>2D float array with elevation values</returns>
        public static float[,] GenerateElevationMap(int width, int height, float elevationScale = 30f, int seed = 0)
        {
            // More octaves for elevation to get varied terrain with mountains and valleys
            return GenerateNoiseMap(width, height, elevationScale, octaves: 6, persistence: 0.5f, lacunarity: 2f, seed: seed);
        }
        
        /// <summary>
        /// Generates a moisture map for determining terrain wetness
        /// </summary>
        /// <param name="width">Width of the map</param>
        /// <param name="height">Height of the map</param>
        /// <param name="moistureScale">Base scale of moisture features</param>
        /// <param name="seed">Random seed</param>
        /// <returns>2D float array with moisture values</returns>
        public static float[,] GenerateMoistureMap(int width, int height, float moistureScale = 50f, int seed = 0)
        {
            // Moisture typically has larger, smoother features
            return GenerateNoiseMap(width, height, moistureScale, octaves: 4, persistence: 0.4f, lacunarity: 2f, seed: seed + 10000);
        }
        
        /// <summary>
        /// Determines the terrain type based on elevation and moisture
        /// </summary>
        /// <param name="elevation">Elevation value (0-1)</param>
        /// <param name="moisture">Moisture value (0-1)</param>
        /// <param name="terrainTypes">Dictionary of terrain types indexed by name</param>
        /// <returns>The appropriate terrain type</returns>
        public static TerrainTypeDataSO DetermineTerrainType(float elevation, float moisture, Dictionary<string, TerrainTypeDataSO> terrainTypes)
        {
            // Basic terrain type determination based on elevation and moisture
            if (elevation < 0.2f)
                return terrainTypes["Water"]; // Water for low elevation
            
            if (elevation < 0.5f)
            {
                // Low to mid elevation
                if (moisture < 0.3f)
                    return terrainTypes["Desert"]; // Dry lowlands become desert
                else if (moisture < 0.7f)
                    return terrainTypes["Plains"]; // Moderate moisture becomes plains
                else
                    return terrainTypes["Forest"]; // Wet lowlands become forest
            }
            else
            {
                // High elevation
                if (moisture < 0.5f)
                    return terrainTypes["Mountains"]; // Dry highlands become mountains
                else
                    return terrainTypes["Forest"]; // Wet highlands become forest
            }
        }
        
        /// <summary>
        /// Generates a complete terrain type map based on elevation and moisture
        /// </summary>
        /// <param name="width">Width of the map</param>
        /// <param name="height">Height of the map</param>
        /// <param name="elevationScale">Scale of elevation features</param>
        /// <param name="moistureScale">Scale of moisture features</param>
        /// <param name="terrainTypes">Dictionary of terrain types indexed by name</param>
        /// <param name="seed">Random seed</param>
        /// <returns>2D array of terrain types</returns>
        public static TerrainTypeDataSO[,] GenerateTerrainMap(int width, int height, float elevationScale, float moistureScale,
                                                            Dictionary<string, TerrainTypeDataSO> terrainTypes, int seed = 0)
        {
            // Generate base noise maps
            float[,] elevationMap = GenerateElevationMap(width, height, elevationScale, seed);
            float[,] moistureMap = GenerateMoistureMap(width, height, moistureScale, seed);
            
            // Create terrain type map
            TerrainTypeDataSO[,] terrainMap = new TerrainTypeDataSO[width, height];
            
            // Fill terrain map based on elevation and moisture
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    terrainMap[x, y] = DetermineTerrainType(elevationMap[x, y], moistureMap[x, y], terrainTypes);
                }
            }
            
            return terrainMap;
        }
    }
}