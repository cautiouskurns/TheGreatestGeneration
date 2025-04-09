/// CLASS PURPOSE:
/// TradeInfo serves as a simple data container for tracking individual trade transactions
/// between regions, capturing the partner involved, resource traded, and quantity exchanged.
///
/// CORE RESPONSIBILITIES:
/// - Store the name of the trade partner
/// - Record which resource was traded
/// - Track the quantity of the resource exchanged
///
/// KEY COLLABORATORS:
/// - TradeCalculator: Generates and fills instances of TradeInfo during simulation
/// - TradeSystem / TradeLogger: May store or display trade history based on this data
/// - UI Systems: Can visualize past or pending trade records using TradeInfo
///
/// CURRENT ARCHITECTURE NOTES:
/// - Pure data class with public fields for simplicity
/// - Does not yet include metadata such as direction, timestamps, or region IDs
///
/// REFACTORING SUGGESTIONS:
/// - Convert to struct if used frequently and immutably
/// - Replace string references with region or resource IDs for type safety and efficiency
/// - Consider encapsulating in a richer TradeTransaction class with context and logic
///
/// EXTENSION OPPORTUNITIES:
/// - Add timestamp, direction (import/export), and status flags
/// - Support historical analysis, visual overlays, or player trade controls
/// - Track transaction source (AI policy, event, player choice)

using UnityEngine;

// Trade info class for storing trade records
public class TradeInfo
{
    public string partnerName;
    public string resourceName;
    public float amount;
}
