using UnityEngine;
public class RegionEntity
{
    public string regionName;
    public int wealth;
    public int production;
    public string ownerNationName;
    public Color regionColor;

    public RegionEntity(string name, int initialWealth, int initialProduction, string nationName, Color color)
    {
        regionName = name;
        wealth = initialWealth;
        production = initialProduction;
        ownerNationName = nationName;
        regionColor = color;
    }

    public void UpdateEconomy(int wealthChange, int productionChange)
    {
        wealth += wealthChange;
        production += productionChange;

        // Notify UI & other systems that this region has been updated
        EventBus.Trigger("RegionUpdated", this);
    }
}

