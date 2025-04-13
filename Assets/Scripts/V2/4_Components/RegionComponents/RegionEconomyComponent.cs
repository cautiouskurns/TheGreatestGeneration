using UnityEngine;

namespace V2.Components
{
    public class RegionEconomyComponent
    {
        // Core properties
        public int Wealth { get; set; }
        public int Production { get; set; }

        public RegionEconomyComponent(int initialWealth, int initialProduction)
        {
            Wealth = initialWealth;
            Production = initialProduction;
        }

        public void UpdateEconomy(int productionOutput)
        {
            Production = productionOutput;
            Wealth += Production;
            
            Debug.Log($"Economy updated: Wealth = {Wealth}, Production = {Production}");
        }
    }
}