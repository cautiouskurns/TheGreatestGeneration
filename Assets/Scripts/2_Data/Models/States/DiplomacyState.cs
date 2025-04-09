/// CLASS PURPOSE:
/// DiplomacyState encapsulates the diplomatic relationships and trade partnerships
/// of a nation. It stores quantitative and qualitative data about foreign relations,
/// and provides access to this data for gameplay systems involving diplomacy or trade.
///
/// CORE RESPONSIBILITIES:
/// - Store numerical relationship values with other nations
/// - Track active trading partnerships on a per-region basis
///
/// KEY COLLABORATORS:
/// - NationEntity: Owns a DiplomacyState instance to track its external relations
/// - TradeSystem: Uses ActiveTradingPartners to determine eligible trade routes
/// - EventSystem or UI: Reads NationRelations for player feedback and event logic
///
/// CURRENT ARCHITECTURE NOTES:
/// - NationRelations uses nation names (string) as keys; consider replacing with IDs
/// - ActiveTradingPartners tracks region names to lists of partner region names
/// - No methods are currently defined for modifying or querying data
///
/// REFACTORING SUGGESTIONS:
/// - Add utility methods for adjusting relations and managing partnerships
/// - Normalize relationship bounds to a named constant or SO for clarity
/// - Separate trade partnerships from diplomatic relations if complexity grows
///
/// EXTENSION OPPORTUNITIES:
/// - Add diplomatic statuses (e.g., alliance, truce, embargo) as enums or flags
/// - Track historical trends in relations for narrative or AI purposes
/// - Introduce influence or favor systems tied to diplomacy

using UnityEngine;
using System.Collections.Generic;

public class DiplomacyState
{
    // Tracks relations with other nations (-100 to 100)
    public Dictionary<string, float> NationRelations { get; set; } = new Dictionary<string, float>();
    
    // Tracks active trading partners for each region
    public Dictionary<string, List<string>> ActiveTradingPartners { get; set; } = new Dictionary<string, List<string>>();
}
