/// CLASS PURPOSE:
/// RegionMapDataSO defines a static map layout for regions in the game world,
/// specifying their names, positions, and visual identifiers (colors).
///
/// CORE RESPONSIBILITIES:
/// - Store and expose region metadata for map generation and rendering
/// - Provide positional data for placement and interaction logic
/// - Serve as a visual reference layer for identifying and interacting with regions
///
/// KEY COLLABORATORS:
/// - MapManager: Uses this data to draw regions on the map at correct positions
/// - RegionController: Links region visuals and names with game entities
/// - UI systems: Reference color and label data for overlays or tooltips
///
/// CURRENT ARCHITECTURE NOTES:
/// - Data is kept minimal for editor configurability and visual prototyping
/// - Each region includes a name, color, and position in 2D space
///
/// REFACTORING SUGGESTIONS:
/// - Add region identifiers or unique IDs for more reliable linking to game logic
/// - Include additional fields like size, adjacency, or terrain type if needed
///
/// EXTENSION OPPORTUNITIES:
/// - Expand to support dynamic region states or overlays
/// - Link to gameplay mechanics like ownership, development, or economic influence
/// - Support importing/exporting map data from external sources

using UnityEngine;

[CreateAssetMenu(fileName = "RegionMap", menuName = "Game/Region Map")]
public class RegionMapDataSO : ScriptableObject
{
    [System.Serializable]
    public class RegionData
    {
        public string regionName;
        public Color regionColor;
        public Vector2 position;
    }

    public RegionData[] regions;
}