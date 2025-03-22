using System;

public class RegionEntity
{
    public string regionName;
    public int wealth;
    public int production;

    // ✅ Correct Constructor with 3 Arguments
    public RegionEntity(string name, int initialWealth, int initialProduction)
    {
        regionName = name;
        wealth = initialWealth;
        production = initialProduction;
    }

    // Method to update economy and trigger an event for UI updates
    public void UpdateEconomy(int wealthChange, int productionChange)
    {
        wealth += wealthChange;
        production += productionChange;

        // Notify UI & other systems that this region has been updated
        EventBus.Trigger("RegionUpdated", this);
    }

}
