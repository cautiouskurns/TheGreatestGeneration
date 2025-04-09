using UnityEngine;
using System.Collections.Generic;
using V1.Data;

namespace V1.Utils
{ 
    public class TerrainGenerator
    {
        private int width;
        private int height;
        private int seed;
        private float elevationScale = 30f;
        private float moistureScale = 50f;
        private Dictionary<string, TerrainTypeDataSO> terrainTypes;
        
        public TerrainGenerator(int width, int height, int seed)
        {
            this.width = width;
            this.height = height;
            this.seed = seed;
        }
        
        public void SetTerrainParameters(float elevationScale, float moistureScale)
        {
            this.elevationScale = elevationScale;
            this.moistureScale = moistureScale;
        }
        
        public void SetTerrainTypes(Dictionary<string, TerrainTypeDataSO> terrainTypes)
        {
            this.terrainTypes = terrainTypes;
        }
        
        public TerrainMapDataSO GenerateTerrainMap()
        {
            // Create a new TerrainMapDataSO
            TerrainMapDataSO terrainMap = ScriptableObject.CreateInstance<TerrainMapDataSO>();
            terrainMap.width = width;
            terrainMap.height = height;
            terrainMap.seed = seed;
            terrainMap.elevationScale = elevationScale;
            terrainMap.moistureScale = moistureScale;
            
            // Generate terrain data
            terrainMap.terrainData = new string[width, height];
            
            // Generate elevation and moisture maps
            float[,] elevationMap = NoiseGenerator.GenerateElevationMap(width, height, elevationScale, seed);
            float[,] moistureMap = NoiseGenerator.GenerateMoistureMap(width, height, moistureScale, seed);
            
            // Determine terrain type for each cell
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TerrainTypeDataSO terrain = NoiseGenerator.DetermineTerrainType(
                        elevationMap[x, y], 
                        moistureMap[x, y], 
                        terrainTypes
                    );
                    
                    terrainMap.terrainData[x, y] = terrain.terrainName;
                }
            }
            
            return terrainMap;
        }
    }
}