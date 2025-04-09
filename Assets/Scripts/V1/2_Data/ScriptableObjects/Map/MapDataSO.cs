using UnityEngine;

namespace V1.Data
{

    /// CLASS PURPOSE:
    /// MapDataSO stores the static map structure of the game world, including nations,
    /// their regions, initial economic stats, and basic geographical properties.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Define nations and their constituent regions
    /// - Store initial conditions such as wealth, production, and terrain type
    /// - Provide a centralized data source for map generation and simulation initialization
    ///
    /// KEY COLLABORATORS:
    /// - MapManager: Uses this data to instantiate the map layout in the game
    /// - GameInitializer: Loads this data to set up initial game state
    /// - RegionController: References region properties like position and economic stats
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Simple hierarchical structure: nations contain regions with relevant metadata
    /// - All data is serialized for easy editing via Unity’s Inspector
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Consider splitting nation and region data into separate ScriptableObjects if reuse or modularity becomes a concern
    /// - Add validation methods to ensure data consistency across the hierarchy
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Include infrastructure, population, or faction ownership per region
    /// - Support dynamic terrain modifiers or event hooks during gameplay
    /// - Add support for region adjacency or connection graphs

    [CreateAssetMenu(fileName = "NewMapData", menuName = "Game Data/Map Data")]
    public class MapDataSO : ScriptableObject
    {
        [System.Serializable]
        public class NationData
        {
            public string nationName;
            public Color nationColor;
            public RegionData[] regions;
        }

        [System.Serializable]
        public class RegionData
        {
            public string regionName;
            public int initialWealth;
            public int initialProduction;
            public Vector2 position;
            public string terrainTypeName;
        }

        public NationData[] nations;
    }
}