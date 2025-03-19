using System.Collections.Generic;
using UnityEngine;

public class EconomicSystem : MonoBehaviour
{
    private List<RegionEntity> regions = new List<RegionEntity>();

    private void Awake()
    {
        EventBus.Trigger("EconomicSystemReady", this);
    }

    private void OnEnable()
    {
        EventBus.Subscribe("TurnEnded", ProcessEconomy);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("TurnEnded", ProcessEconomy);
    }

    public void RegisterRegion(RegionEntity region)
    {
        regions.Add(region);
    }

    private void ProcessEconomy(object _)
    {
        Debug.Log("🔄 TURN PROCESSING STARTED 🔄");

        foreach (RegionEntity region in regions)
        {
            int wealthChange = region.production * 2;  // Wealth grows based on production
            int productionChange = UnityEngine.Random.Range(-2, 3); // Simulate fluctuation

            Debug.Log($"🏙️ {region.regionName}: Wealth {region.wealth} → {region.wealth + wealthChange}, Production {region.production} → {region.production + productionChange}");

            region.UpdateEconomy(wealthChange, productionChange);
        }

        Debug.Log("✅ TURN PROCESSING COMPLETE ✅");
    }
}

