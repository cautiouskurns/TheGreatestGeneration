using UnityEngine;
using System.Collections.Generic;
using V1.Data;
using V1.Components;
using V1.Managers;


namespace V1.Entities
{
    /// CLASS PURPOSE:
    /// RegionEntity represents a runtime model of a region in the game, aggregating
    /// data from multiple components to simulate economic behavior, population dynamics,
    /// resource flow, and terrain impact.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Maintain and expose region-specific data such as wealth, production, and satisfaction
    /// - Coordinate updates across economic, population, and resource systems per turn
    /// - Integrate terrain and resource effects into economic simulation
    /// - Provide runtime descriptions and summaries for UI or debugging
    ///
    /// KEY COLLABORATORS:
    /// - RegionEconomyComponent: Calculates and applies wealth/production changes
    /// - RegionPopulationComponent: Manages labor, satisfaction, and population effects
    /// - ResourceComponent: Handles production, demand, and net resource flows
    /// - ProductionComponent: Converts inputs into economic output
    /// - EventBus: Dispatches region update notifications for other systems
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses component-based pattern with tight coupling via direct references
    /// - Exposes many properties directly for compatibility with existing systems
    /// - Includes compatibility accessors for economic and population metrics
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Introduce interfaces for major components to improve modularity and testing
    /// - Split complex logic in ProcessTurn into smaller strategy functions or delegates
    /// - Move GetDescription logic into a separate formatter or presenter class
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Track regional policy, cultural traits, or political stability
    /// - Add support for automation or AI decision-making at the regional level
    /// - Incorporate event history or turn-based logging per region
    /// 
    public class RegionEntity
    {
        // Basic identity properties
        public string regionName;
        public string ownerNationName;
        public Color regionColor;
        public TerrainTypeDataSO terrainType;
        
        // Components
        public RegionEconomyComponent economy;
        public RegionPopulationComponent population;
        public ResourceComponent resources;
        public ProductionComponent productionComponent;
        
        // Change tracking
        public bool hasChangedThisTurn = false;
        
        // Terrain effect
        public float landProductivity = 1.0f;  // Affected by terrain type
        
        // COMPATIBILITY PROPERTIES - Forward to components
        // Economy properties
        public int wealth { 
            get { return economy.wealth; }
            set { economy.wealth = value; }
        }
        
        public int production { 
            get { return economy.production; }
            set { economy.production = value; }
        }
        
        public float productionEfficiency {
            get { return economy.productionEfficiency; }
            set { economy.productionEfficiency = value; }
        }
        
        public float capitalInvestment {
            get { return economy.capitalInvestment; }
            set { economy.capitalInvestment = value; }
        }
        
        public int wealthDelta {
            get { return economy.wealthDelta; }
            set { economy.wealthDelta = value; }
        }
        
        public int productionDelta {
            get { return economy.productionDelta; }
            set { economy.productionDelta = value; }
        }
        
        public float infrastructureLevel {
            get { return economy.infrastructureLevel; }
            set { economy.infrastructureLevel = value; }
        }
        
        // Population properties
        public int laborAvailable {
            get { return population.laborAvailable; }
            set { population.laborAvailable = value; }
        }
        
        public float satisfaction {
            get { return population.satisfaction; }
            set { population.satisfaction = value; }
        }
        
        public Dictionary<string, float> laborAllocation {
            get { return population.laborAllocation; }
        }
        
        // Constructor for basic region
        public RegionEntity(string name, int initialWealth, int initialProduction, string nationName, Color color)
        {
            regionName = name;
            ownerNationName = nationName;
            regionColor = color;
            
            // Initialize components
            economy = new RegionEconomyComponent(this, initialWealth, initialProduction);
            population = new RegionPopulationComponent(this);
        }

        // Constructor that includes terrain
        public RegionEntity(string name, int initialWealth, int initialProduction, string nationName, Color color, TerrainTypeDataSO terrain)
            : this(name, initialWealth, initialProduction, nationName, color)
        {
            terrainType = terrain;
            
            // Initialize resource component
            resources = new ResourceComponent(this);
            productionComponent = new ProductionComponent(this, resources);

            // Initialize terrain-specific resources
            InitializeTerrainResources();
        }
        
        private void InitializeTerrainResources()
        {
            if (terrainType != null)
            {
                // Add different starting resources based on terrain type
                switch (terrainType.terrainName)
                {
                    case "Forest":
                        resources.AddResource("Wood", 50);
                        resources.AddResource("Food", 30);
                        break;
                    case "Mountains":
                        resources.AddResource("Coal", 50);
                        resources.AddResource("Iron Ore", 30);
                        break;
                    case "Plains":
                        resources.AddResource("Food", 50);
                        resources.AddResource("Wood", 20);
                        break;
                    default:
                        // Default starting resources
                        resources.AddResource("Food", 30);
                        resources.AddResource("Wood", 30);
                        break;
                }
            }
        }


        // Updated economy update method
        public void UpdateEconomy(int wealthChange, int productionChange)
        {
            // For now, temporarily store the requested changes
            int requestedWealthChange = wealthChange;
            int requestedProductionChange = productionChange;
            
            // Call the centralized processing
            ProcessTurn();
            
            // For backward compatibility, make sure the delta values match what was expected
            // This may not be necessary once all systems are updated to use ProcessTurn
            economy.wealthDelta = requestedWealthChange;
            economy.productionDelta = requestedProductionChange;
        }


        // Reset changes after visualization
        public void ResetChangeFlags()
        {
            hasChangedThisTurn = false;
            economy.ResetChangeFlags();
        }
        
        // Description methods
        public string GetDescription()
        {
            string description = $"Region: {regionName}\n" +
                                $"Nation: {ownerNationName}\n" +
                                $"Wealth: {economy.wealth}\n" +
                                $"Production: {economy.production}";
            
            if (terrainType != null)
            {
                description += $"\n\nTerrain: {terrainType.terrainName}\n" + 
                            $"{terrainType.description}\n\n" + 
                            $"{GetTerrainEffectsDescription()}";
            }
            
            return description;
        }
        
        public string GetDetailedDescription()
        {
            string description = GetDescription();
            
            // Add resource information if available
            if (resources != null)
            {
                description += "\n\n=== Resources ===\n";
                
                var allResources = resources.GetAllResources();
                var productionRates = resources.GetAllProductionRates();
                var consumptionRates = resources.GetAllConsumptionRates();
                
                foreach (var resource in allResources.Keys)
                {
                    float amount = allResources[resource];
                    float production = productionRates.ContainsKey(resource) ? productionRates[resource] : 0;
                    float consumption = consumptionRates.ContainsKey(resource) ? consumptionRates[resource] : 0;
                    float netChange = production - consumption;
                    
                    string changeIndicator = netChange > 0 ? "↑" : (netChange < 0 ? "↓" : "→");
                    description += $"{resource}: {amount:F1} {changeIndicator} ({netChange:+0.0;-0.0})\n";
                }
            }
            
            return description;
        }
        
        private string GetTerrainEffectsDescription()
        {
            if (terrainType == null)
                return "";
                
            string effects = "Economic Effects:";
            
            float agricultureMod = terrainType.GetMultiplierForSector("agriculture");
            if (agricultureMod != 1.0f)
                effects += $"\nAgriculture: {FormatModifier(agricultureMod)}";
                
            float miningMod = terrainType.GetMultiplierForSector("mining");
            if (miningMod != 1.0f)
                effects += $"\nMining: {FormatModifier(miningMod)}";
                
            float industryMod = terrainType.GetMultiplierForSector("industry");
            if (industryMod != 1.0f)
                effects += $"\nIndustry: {FormatModifier(industryMod)}";
                
            float commerceMod = terrainType.GetMultiplierForSector("commerce");
            if (commerceMod != 1.0f)
                effects += $"\nCommerce: {FormatModifier(commerceMod)}";
                
            return effects;
        }
        
        private string FormatModifier(float modifier)
        {
            float percentage = (modifier - 1.0f) * 100f;
            string sign = percentage >= 0 ? "+" : "";
            return $"{sign}{percentage:F0}%";
        }



        public void ProcessTurn()
        {
            // Calculate base economic changes
            int wealthChange = economy.CalculateBaseWealthChange();
            int productionChange = economy.CalculateBaseProductionChange();
            
            // Calculate region "size" for resource processing
            float regionSize = economy.production / 10.0f;
            
            // Process resources
            if (resources != null)
            {
                resources.CalculateProduction();
                resources.CalculateDemand();
                resources.ProcessTurn(economy.wealth, regionSize);
                
                // Calculate satisfaction based on needs being met
                Dictionary<string, float> needsSatisfaction = resources.GetConsumptionSatisfaction();
                
                // Update satisfaction and population
                population.UpdateSatisfaction(needsSatisfaction);
                population.UpdatePopulation();
                
                // Apply population effects to economy
                economy.ApplySatisfactionEffects(population.satisfaction);
                
                // Calculate resource balance effects on economy
                Dictionary<string, float> resourceBalance = CalculateResourceBalance();
                int resourceWealthEffect = economy.CalculateResourceEffect(resourceBalance);
                wealthChange += resourceWealthEffect;
            }
            
            // Process production
            if (productionComponent != null)
            {
                productionComponent.ProcessProduction();
            }
            
            // Apply economic changes
            economy.ApplyChanges(wealthChange, productionChange);
            
            // Mark as changed for visualization
            hasChangedThisTurn = true;
            
            // Notify systems
            EventBus.Trigger("RegionUpdated", this);
        }


        private Dictionary<string, float> CalculateResourceBalance()
        {
            Dictionary<string, float> balance = new Dictionary<string, float>();
            
            if (resources == null) return balance;
            
            var productionRates = resources.GetAllProductionRates();
            var consumptionRates = resources.GetAllConsumptionRates();
            
            // Combine all resource names
            HashSet<string> resourceNames = new HashSet<string>();
            foreach (var key in productionRates.Keys) resourceNames.Add(key);
            foreach (var key in consumptionRates.Keys) resourceNames.Add(key);
            
            // Calculate balance for each resource
            foreach (var resource in resourceNames)
            {
                float production = productionRates.ContainsKey(resource) ? 
                    productionRates[resource] : 0;
                float consumption = consumptionRates.ContainsKey(resource) ? 
                    consumptionRates[resource] : 0;
                
                balance[resource] = production - consumption;
            }
            
            return balance;
        }
    }
}