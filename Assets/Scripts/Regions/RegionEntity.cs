public class RegionEntity
{
    public string regionName;
    public int wealth;
    public int production;

    public RegionEntity(RegionDataSO data)
    {
        regionName = data.regionName;
        wealth = data.initialWealth;
        production = data.initialProduction;
    }

    public void UpdateEconomy(int wealthChange, int productionChange)
    {
        wealth += wealthChange;
        production += productionChange;
        EventBus.Trigger("RegionUpdated", this);
    }
}

