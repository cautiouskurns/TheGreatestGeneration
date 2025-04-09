/// CLASS PURPOSE:
/// RegionState represents the persistent state data of an individual region.
/// It stores basic demographic and economic attributes needed by gameplay systems
/// such as population size, satisfaction levels, ownership, and dominant economic sector.
///
/// CORE RESPONSIBILITIES:
/// - Hold basic information about a region’s current condition
/// - Track which nation owns the region
/// - Store current dominant sector (used for color-coding, UI, and production logic)
/// - Maintain population and citizen satisfaction levels
///
/// KEY COLLABORATORS:
/// - RegionEntity: Uses this data for logic and economic processing
/// - NationModel: Aggregates regional states for national summaries
/// - MapView/UI: Displays region name, sector, and satisfaction to the player
///
/// CURRENT ARCHITECTURE NOTES:
/// - Designed as a plain data class with automatic properties
/// - Satisfaction is normalized on a 0 to 1 scale
/// - Default population starts at 100
///
/// REFACTORING SUGGESTIONS:
/// - Expand to include sector development levels and infrastructure states
/// - Introduce region ID system to decouple from name-based references
/// - Consider moving calculated fields (e.g., resource yield) into a separate runtime model
///
/// EXTENSION OPPORTUNITIES:
/// - Track recent events affecting satisfaction or development
/// - Add historical logging or turn-based snapshots
/// - Incorporate region traits (e.g., fertile, coastal, mountainous)

using UnityEngine;

public class RegionState
{
    public string RegionName { get; set; }
    public string OwnerNation { get; set; }
    public string DominantSector { get; set; }
    public float Satisfaction { get; set; } = 0.5f; // 0 to 1
    public int Population { get; set; } = 100;
}