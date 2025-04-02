using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnhancedEconomicSystem : MonoBehaviour
{
    [Header("Economic Model Configuration")]
    public EnhancedEconomicModelSO economicModel;

    // Reference to game systems
    private MapModel mapModel;
    private TradeSystem tradeSystem;
    private ResourceMarket resourceMarket;

    // Add a button in the Inspector to manually trigger debug
    [ContextMenu("Debug Economic State")]
    public void InvokeDebugEconomicState()
    {
        DebugEconomicState();
    }


    private void Awake()
    {
        // Ensure we have an economic model
        if (economicModel == null)
        {
            Debug.LogWarning("No Enhanced Economic Model assigned to Economic Engine!");
        }
    }

    private void OnEnable()
    {
        // Subscribe to turn-related events
        EventBus.Subscribe("TurnEnded", ProcessEconomicTurn);
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        EventBus.Unsubscribe("TurnEnded", ProcessEconomicTurn);
    }

    private void ProcessEconomicTurn(object _)
    {
        // Log the start of processing
        Debug.Log("EnhancedEconomicSystem: Beginning turn processing...");

        // Lazy initialization of systems
        InitializeSystems();

        // Check if economic model is assigned
        if (economicModel == null)
        {
            Debug.LogError("EnhancedEconomicSystem: No economic model assigned! Cannot process turn.");
            return;
        }

        // Start processing with timestamps
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        // Comprehensive economic turn processing
        Debug.Log("EnhancedEconomicSystem: Processing supply and demand...");
        ProcessSupplyAndDemand();

        Debug.Log("EnhancedEconomicSystem: Processing production...");
        ProcessProduction();

        Debug.Log("EnhancedEconomicSystem: Processing infrastructure...");
        ProcessInfrastructure();

        Debug.Log("EnhancedEconomicSystem: Processing population consumption...");
        ProcessPopulationConsumption();

        Debug.Log("EnhancedEconomicSystem: Processing economic cycle...");
        ProcessEconomicCycle();

        Debug.Log("EnhancedEconomicSystem: Processing price volatility...");
        ProcessPriceVolatility();

        stopwatch.Stop();
        Debug.Log($"EnhancedEconomicSystem: Turn processing completed in {stopwatch.ElapsedMilliseconds}ms");

        // Trigger custom event to notify about completion
        EventBus.Trigger("EnhancedEconomicProcessed", null);
    }


    private void InitializeSystems()
    {
        // Find necessary game systems if not already referenced
        if (mapModel == null)
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            mapModel = gameManager?.GetMapModel();
        }

        if (tradeSystem == null)
        {
            tradeSystem = FindFirstObjectByType<TradeSystem>();
        }

        if (resourceMarket == null)
        {
            resourceMarket = FindFirstObjectByType<ResourceMarket>();
        }
    }

    private void ProcessSupplyAndDemand()
    {
        var rules = economicModel.supplyDemandRules;

        foreach (var resourceType in rules.resourceElasticityMap.Keys)
        {
            // Calculate supply
            float supply = CalculateResourceSupply(resourceType);
            
            // Calculate demand
            float demand = CalculateResourceDemand(resourceType);
            
            // Adjust price based on supply-demand dynamics
            float basePrice = resourceMarket.GetBasePrice(resourceType);
            float elasticity = rules.resourceElasticityMap[resourceType];
            
            float priceAdjustment = basePrice * (1 + (demand - supply) / elasticity);
            
            // Apply price in resource market
            resourceMarket.AdjustPrice(resourceType, priceAdjustment);
        }
    }

    private float CalculateResourceSupply(string resourceType)
    {
        float totalSupply = 0f;
        
        foreach (var region in mapModel.GetAllRegions().Values)
        {
            // Get production for this resource type
            float regionSupply = region.resources.GetProductionRate(resourceType);
            totalSupply += regionSupply;
        }
        
        return totalSupply;
    }

    private float CalculateResourceDemand(string resourceType)
    {
        float totalDemand = 0f;
        
        foreach (var region in mapModel.GetAllRegions().Values)
        {
            // Calculate demand based on population, wealth, and infrastructure
            float populationFactor = region.laborAvailable;
            float wealthFactor = Mathf.Clamp01(region.wealth / 1000f);
            float infrastructureFactor = region.infrastructureLevel; // Assuming this exists
            
            float demandMultiplier = economicModel.supplyDemandRules.demandCalculationCurve.Evaluate(
                (populationFactor * wealthFactor * infrastructureFactor) / 1000f
            );
            
            float regionDemand = region.resources.GetConsumptionRate(resourceType) * demandMultiplier;
            totalDemand += regionDemand;
        }
        
        return totalDemand;
    }

    private void ProcessProduction()
    {
        var rules = economicModel.productionRules;

        foreach (var region in mapModel.GetAllRegions().Values)
        {
            // Cobb-Douglas production function
            float labor = region.laborAvailable;
            float capital = region.infrastructureLevel; // Assuming this exists
            
            // Get sector-specific multiplier
            string dominantSector = DetermineDominantSector(region);
            float sectorMultiplier = rules.sectorProductivityMultipliers.ContainsKey(dominantSector)
                ? rules.sectorProductivityMultipliers[dominantSector]
                : 1f;

            // Calculate production
            float production = rules.productivityFactor 
                * Mathf.Pow(labor, rules.laborElasticity) 
                * Mathf.Pow(capital, rules.capitalElasticity)
                * sectorMultiplier;

            // Update region's production
            region.production = Mathf.RoundToInt(production);
        }
    }

    private string DetermineDominantSector(RegionEntity region)
    {
        // Determine the dominant sector based on labor allocation or other metrics
        if (region.laborAllocation.TryGetValue("Agriculture", out float agricultureShare) && agricultureShare > 0.5f)
            return "Agriculture";
        if (region.laborAllocation.TryGetValue("Industry", out float industryShare) && industryShare > 0.5f)
            return "Industry";
        return "Commerce";
    }

    private void ProcessInfrastructure()
    {
        var rules = economicModel.infrastructureRules;

        foreach (var region in mapModel.GetAllRegions().Values)
        {
            // Calculate infrastructure efficiency
            float infrastructureLevel = region.infrastructureLevel;
            float efficiencyMultiplier = rules.infrastructureEfficiencyCurve.Evaluate(infrastructureLevel);
            
            // Apply infrastructure decay
            float decayAmount = infrastructureLevel * rules.infrastructureDecayRate;
            
            // Calculate maintenance cost
            float maintenanceCost = rules.maintenanceCostCurve.Evaluate(infrastructureLevel);
            
            // Reduce wealth based on maintenance
            region.wealth -= Mathf.RoundToInt(maintenanceCost);
            
            // Update infrastructure level (simplified)
            region.infrastructureLevel = Mathf.Max(0, infrastructureLevel - decayAmount);
        }
    }

    private void ProcessPopulationConsumption()
    {
        var rules = economicModel.populationConsumptionRules;

        foreach (var region in mapModel.GetAllRegions().Values)
        {
            // Calculate wealth per capita
            float wealthPerCapita = region.wealth / region.laborAvailable;
            
            // Calculate base consumption
            float baseConsumption = region.laborAvailable * rules.baseConsumptionRate;
            
            // Apply wealth elasticity
            float wealthElasticityFactor = Mathf.Pow(wealthPerCapita, rules.wealthConsumptionElasticity);
            float totalConsumption = baseConsumption * wealthElasticityFactor;

            // Track unmet demand
            float unmetDemand = CalculateUnmetDemand(region);
            
            // Calculate impact of unmet demand
            float unrestFactor = rules.unrestFromUnmetDemandCurve.Evaluate(unmetDemand);
            float migrationFactor = rules.migrationFromUnmetDemandCurve.Evaluate(unmetDemand);
            
            // Reduce population satisfaction
            region.satisfaction = Mathf.Clamp01(region.satisfaction - unrestFactor);
            
            // Potential population migration
            if (migrationFactor > 0)
            {
                int potentialMigration = Mathf.RoundToInt(region.laborAvailable * migrationFactor);
                region.laborAvailable -= potentialMigration;
                
                // You might want to add logic to transfer migrants to neighboring regions
                Debug.Log($"Region {region.regionName} experienced migration of {potentialMigration} due to unmet demand");
            }
        }
    }

        private float CalculateUnmetDemand(RegionEntity region)
    {
        float totalUnmetDemand = 0f;
        
        // Check unmet demand for each resource
        var resources = region.resources.GetAllResources();
        var consumption = region.resources.GetAllConsumptionRates();
        
        foreach (var resourceType in resources.Keys)
        {
            float available = resources[resourceType];
            float needed = consumption.ContainsKey(resourceType) ? consumption[resourceType] : 0;
            
            if (needed > available)
            {
                totalUnmetDemand += (needed - available) / needed;
            }
        }
        
        return Mathf.Clamp01(totalUnmetDemand);
    }

    private void ProcessEconomicCycle()
    {
        var rules = economicModel.economicCycleRules;
        
        // Retrieve current economic phase from GameStateManager
        var gameStateManager = GameStateManager.Instance;
        string currentPhase = gameStateManager.Economy.CurrentEconomicCyclePhase;
        
        // Get the multiplier for the current phase
        float phaseMultiplier = rules.cyclePhaseMultipliers.ContainsKey(currentPhase)
            ? rules.cyclePhaseMultipliers[currentPhase]
            : 1f;
        
        // Apply cycle effect to all regions
        foreach (var region in mapModel.GetAllRegions().Values)
        {
            // Modify production, wealth, and other metrics based on cycle phase
            float cycleEffect = rules.cycleEffectCoefficient * phaseMultiplier;
            
            // Adjust region metrics
            region.production = Mathf.RoundToInt(region.production * (1 + cycleEffect));
            region.wealth = Mathf.RoundToInt(region.wealth * (1 + cycleEffect * 0.5f));
        }
        
        // Optional: Increment turns in current phase
        gameStateManager.Economy.TurnsInCurrentPhase++;
        
        // Potential phase transition logic could be added here
        // This would depend on your specific economic cycle mechanics
    }

    private void ProcessPriceVolatility()
    {
        var rules = economicModel.priceVolatilityRules;
        
        // Iterate through all resources in the market
        foreach (var resourceType in resourceMarket.GetAllCurrentPrices().Keys)
        {
            float currentPrice = resourceMarket.GetCurrentPrice(resourceType);
            float basePrice = resourceMarket.GetBasePrice(resourceType);
            
            // Calculate supply and demand
            float supply = CalculateResourceSupply(resourceType);
            float demand = CalculateResourceDemand(resourceType);
            
            // Calculate supply shock and consumption trend
            float supplyShock = (supply - demand) / Mathf.Max(supply, 1f);
            float consumptionTrend = demand / Mathf.Max(supply, 1f);
            
            // Apply stochastic variation
            float stochasticShock = UnityEngine.Random.Range(
                -rules.stochasticShockMagnitude, 
                rules.stochasticShockMagnitude
            );
            
            // Calculate price adjustment
            float priceAdjustment = 
                rules.basePriceVolatility * 
                (supplyShock * rules.supplyShockSensitivity - 
                 consumptionTrend * rules.consumptionTrendImpact + 
                 stochasticShock);
            
            // Apply price adjustment
            float newPrice = currentPrice + priceAdjustment * basePrice;
            
            // Clamp price to prevent extreme fluctuations
            newPrice = Mathf.Clamp(
                newPrice, 
                basePrice * 0.5f, 
                basePrice * 2f
            );
            
            // Update resource market price
            resourceMarket.AdjustPrice(resourceType, newPrice);
        }
    }

    // Comprehensive debug method to output economic state
    public void DebugEconomicState()
    {
        Debug.Log("--- Comprehensive Economic State Debug ---");
        
        // Ensure all required systems are found if not already referenced
        if (mapModel == null)
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                mapModel = gameManager.GetMapModel(); // Ensure this method exists in GameManager
            }
        }
        
        if (resourceMarket == null)
        {
            resourceMarket = FindFirstObjectByType<ResourceMarket>();
        }
        
        var gameStateManager = GameStateManager.Instance;
        
        // Check for null references before accessing
        if (gameStateManager == null)
        {
            Debug.LogWarning("GameStateManager is null. Cannot retrieve economic phase information.");
            return;
        }
        
        // Economic Cycle Information
        Debug.Log($"Current Economic Phase: {gameStateManager.Economy?.CurrentEconomicCyclePhase ?? "Unknown"}");
        Debug.Log($"Turns in Current Phase: {gameStateManager.Economy?.TurnsInCurrentPhase ?? 0}");
        
        // Resource Market Overview
        if (resourceMarket != null)
        {
            Debug.Log("--- Resource Market Prices ---");
            var currentPrices = resourceMarket.GetAllCurrentPrices();
            if (currentPrices != null)
            {
                foreach (var resourceType in currentPrices.Keys)
                {
                    float currentPrice = resourceMarket.GetCurrentPrice(resourceType);
                    float basePrice = resourceMarket.GetBasePrice(resourceType);
                    
                    Debug.Log($"{resourceType}: Current Price = {currentPrice:F2}, Base Price = {basePrice:F2}");
                }
            }
            else
            {
                Debug.LogWarning("No resource prices available.");
            }
        }
        else
        {
            Debug.LogWarning("ResourceMarket is null. Cannot retrieve market prices.");
        }
        
        // Detailed Region Economic Breakdown
        if (mapModel != null)
        {
            Debug.Log("--- Region Economic Details ---");
            var regions = mapModel.GetAllRegions();
            if (regions != null)
            {
                foreach (var regionEntry in regions)
                {
                    var region = regionEntry.Value;
                    if (region == null) continue;

                    Debug.Log($"Region: {region.regionName}");
                    Debug.Log($"  Population: {region.laborAvailable}");
                    Debug.Log($"  Wealth: {region.wealth}");
                    Debug.Log($"  Production: {region.production}");
                    Debug.Log($"  Satisfaction: {region.satisfaction:P2}");
                    
                    // Resource details
                    if (region.resources != null)
                    {
                        Debug.Log("  Resource Details:");
                        var resources = region.resources.GetAllResources();
                        var production = region.resources.GetAllProductionRates();
                        var consumption = region.resources.GetAllConsumptionRates();
                        
                        foreach (var resourceType in resources.Keys)
                        {
                            Debug.Log($"    {resourceType}:");
                            Debug.Log($"      Amount: {resources[resourceType]:F2}");
                            Debug.Log($"      Production: {production.GetValueOrDefault(resourceType, 0):F2}");
                            Debug.Log($"      Consumption: {consumption.GetValueOrDefault(resourceType, 0):F2}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("No regions found in MapModel.");
            }
        }
        else
        {
            Debug.LogWarning("MapModel is null. Cannot retrieve region details.");
        }
    }
}