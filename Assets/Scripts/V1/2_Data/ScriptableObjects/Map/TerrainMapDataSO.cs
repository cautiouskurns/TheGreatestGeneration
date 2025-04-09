using UnityEngine;

namespace V1.Data
{
    /// CLASS PURPOSE:
    /// TerrainMapDataSO defines the configuration and raw terrain data for generating
    /// the map's elevation and moisture layout, which serves as a basis for biome and terrain generation.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Store terrain generation parameters such as map dimensions, noise scales, and seed
    /// - Provide a 2D array to hold string-based terrain classifications (e.g., "Plains", "Mountains")
    /// - Serve as a data bridge between terrain generation logic and visual representation tools
    ///
    /// KEY COLLABORATORS:
    /// - TerrainGenerator: Uses the parameters to generate terrain elevation and moisture values
    /// - MapDebugVisualizer: Reads from terrainData to render and debug the generated terrain
    /// - MapManager: May reference terrain classification when spawning map entities or applying effects
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Simple scalar values define perlin noise behavior for terrain shaping
    /// - terrainData array must be managed externally and populated by a generator
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace string[,] with a struct[,] or enum[,] for type safety and better performance
    /// - Validate array dimensions match width and height during runtime assignment
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add additional layers like temperature, vegetation, or fertility
    /// - Support terrain blending or transitions based on biome rules
    /// - Introduce metadata per cell for procedural event or resource generation

    [CreateAssetMenu(fileName = "TerrainMap", menuName = "Game/Terrain Map Data")]
    public class TerrainMapDataSO : ScriptableObject
    {
        public int width;
        public int height;
        public int seed;
        public float elevationScale;
        public float moistureScale;
        
        // Add this property to match what's being used in MapDebugVisualizer
        public string[,] terrainData;
    }
}