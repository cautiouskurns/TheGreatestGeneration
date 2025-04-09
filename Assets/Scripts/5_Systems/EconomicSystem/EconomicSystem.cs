/// CLASS PURPOSE:
/// EconomicSystem coordinates the economic simulation each turn by triggering updates
/// across all registered regions. It functions as a centralized system-level controller.
///
/// CORE RESPONSIBILITIES:
/// - Register regions into the economic simulation
/// - Listen for turn completion events and trigger regional economic updates
/// - Apply basic economic logic (e.g., production-based wealth growth, fluctuations)
///
/// KEY COLLABORATORS:
/// - RegionEntity: Receives per-turn wealth and production updates
/// - EventBus: Handles turn-based event subscriptions and dispatching
/// - MapView / MapManager: May respond to "EconomicSystemReady" trigger to initialize UI or data flows
///
/// CURRENT ARCHITECTURE NOTES:
/// - Currently uses hardcoded economic logic for demonstration purposes
/// - Event subscriptions are commented out, suggesting early prototyping or pending feature completion
///
/// REFACTORING SUGGESTIONS:
/// - Abstract economic logic into a separate service for scalability and reuse
/// - Enable/disable simulation via configuration flags or external control
///
/// EXTENSION OPPORTUNITIES:
/// - Add support for policy effects, economic doctrines, or random events
/// - Integrate with historical logging or analytics for post-turn summaries
/// - Enable region grouping or sector-wide economic influence

using System.Collections.Generic;
using UnityEngine;

public class EconomicSystem : MonoBehaviour
{
    private List<RegionEntity> regions = new List<RegionEntity>();

    private void Awake()
    {
//        Debug.Log("EconomicSystem initializing...");
        // Trigger this at the start to ensure MapView can pick it up
        EventBus.Trigger("EconomicSystemReady", this);
    }

    private void Start()
    {
        // In case MapView subscribes after Awake, trigger again in Start
//        Debug.Log("EconomicSystem ready!");
        EventBus.Trigger("EconomicSystemReady", this);
    }

    private void OnEnable()
    {
        //EventBus.Subscribe("TurnEnded", ProcessEconomy);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("TurnEnded", ProcessEconomy);
    }

    public void RegisterRegion(RegionEntity region)
    {
        Debug.Log($"Registering region: {region.regionName}");
        regions.Add(region);
    }

    private void ProcessEconomy(object _)
    {
        Debug.Log($"🔄 TURN PROCESSING STARTED 🔄 (Regions: {regions.Count})");

        foreach (RegionEntity region in regions)
        {
            int wealthChange = region.production * 2;  // Wealth grows based on production
            int productionChange = Random.Range(-2, 3); // Simulate fluctuation

            Debug.Log($"🏙️ {region.regionName}: Wealth {region.wealth} → {region.wealth + wealthChange}, Production {region.production} → {region.production + productionChange}");

            region.UpdateEconomy(wealthChange, productionChange);
        }

        Debug.Log("✅ TURN PROCESSING COMPLETE ✅");
    }
}
