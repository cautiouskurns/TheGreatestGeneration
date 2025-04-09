/// CLASS PURPOSE:
/// TradeCalculator determines which trade transactions should occur between regions
/// each turn based on resource deficits, surpluses, distance, and partner limits.
///
/// CORE RESPONSIBILITIES:
/// - Calculate resource deficits for each region
/// - Identify valid trading partners with sufficient surplus within a given radius
/// - Generate trade transactions based on trade efficiency and partner caps
/// - Track and enforce max trading partners per region
///
/// KEY COLLABORATORS:
/// - RegionEntity: Supplies resource and consumption data for trade calculation
/// - TradeTransaction: Represents individual trade flows between regions
/// - GameObject/Transform: Used to determine spatial proximity of regions
///
/// CURRENT ARCHITECTURE NOTES:
/// - Partner selection prioritizes existing relationships, then surplus magnitude
/// - Trade efficiency reduces delivered goods based on config
/// - Partner limits enforced for both importers and exporters
///
/// REFACTORING SUGGESTIONS:
/// - Move distance and position logic to a region metadata provider or spatial service
/// - Replace region name strings with direct references or GUIDs for robustness
/// - Optimize by caching frequently used resource/accessor results per turn
///
/// EXTENSION OPPORTUNITIES:
/// - Add transport cost modeling or infrastructure influence
/// - Introduce dynamic trade agreements or policy-based partner selection
/// - Support batch processing or multithreading for large maps

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Class for handling trade calculations
public class TradeCalculator
{
    private readonly Dictionary<string, RegionEntity> regions;
    private readonly float tradeEfficiency;
    private readonly int maxTradingPartnersPerRegion;
    private readonly float tradeRadius;
    
    // Track trading partners for each region across all resources
    private Dictionary<string, HashSet<string>> tradingPartnersByRegion = new Dictionary<string, HashSet<string>>();
    
    public TradeCalculator(
        Dictionary<string, RegionEntity> regions,
        float tradeEfficiency,
        int maxTradingPartnersPerRegion,
        float tradeRadius)
    {
        this.regions = regions;
        this.tradeEfficiency = tradeEfficiency;
        this.maxTradingPartnersPerRegion = maxTradingPartnersPerRegion;
        this.tradeRadius = tradeRadius;
    }
    
    public List<TradeTransaction> CalculateTrades()
    {
        List<TradeTransaction> trades = new List<TradeTransaction>();
        
        // Reset trading partners tracking at the start of each turn
        tradingPartnersByRegion.Clear();
        
        // Initialize trading partner tracking for each region
        foreach (var region in regions.Values)
        {
            tradingPartnersByRegion[region.regionName] = new HashSet<string>();
        }
        
        // First pass: Calculate all deficits
        Dictionary<string, Dictionary<string, float>> allDeficits = new Dictionary<string, Dictionary<string, float>>();
        
        foreach (var region in regions.Values)
        {
            // Skip regions without resources
            if (region.resources == null) continue;
            
            // Calculate deficits for this region
            Dictionary<string, float> deficits = CalculateDeficits(region);
            
            if (deficits.Count > 0)
            {
                allDeficits[region.regionName] = deficits;
            }
        }
        
        // Process all regions' deficits
        foreach (var regionEntry in allDeficits.OrderBy(r => r.Value.Count))
        {
            string regionName = regionEntry.Key;
            Dictionary<string, float> deficits = regionEntry.Value;
            
            if (!regions.ContainsKey(regionName)) continue;
            
            RegionEntity importer = regions[regionName];
            HashSet<string> currentPartners = tradingPartnersByRegion[regionName];
            
            // First, prioritize critical resources (most deficit)
            foreach (var deficitEntry in deficits.OrderByDescending(d => d.Value))
            {
                string resourceName = deficitEntry.Key;
                float deficitAmount = deficitEntry.Value;
                
                // Stop if we've reached our partner limit
                if (currentPartners.Count >= maxTradingPartnersPerRegion)
                {
                    break;
                }
                
                // Find potential partners
                var potentialPartners = FindTradingPartners(importer, resourceName);
                
                // Sort partners (existing partners first, then by surplus)
                var orderedPartners = potentialPartners
                    .OrderByDescending(p => currentPartners.Contains(p.regionName) ? 1 : 0)
                    .ThenByDescending(p => CalculateSurplus(p, resourceName))
                    .ToList();
                
                // Process each potential partner
                foreach (var partner in orderedPartners)
                {
                    // Skip if would exceed limit for either region
                    if (!currentPartners.Contains(partner.regionName))
                    {
                        if (currentPartners.Count >= maxTradingPartnersPerRegion)
                        {
                            continue; // Skip - importer at max partners
                        }
                        
                        var partnerPartners = tradingPartnersByRegion[partner.regionName];
                        if (partnerPartners.Count >= maxTradingPartnersPerRegion && 
                            !partnerPartners.Contains(regionName))
                        {
                            continue; // Skip - exporter at max partners
                        }
                    }
                    
                    // Calculate how much can be traded
                    var (tradeAmount, actualAmount) = CalculateTradeAmount(partner, resourceName, deficitAmount);
                    
                    // Skip if trade amount is too small
                    if (actualAmount < 0.1f) continue;
                    
                    // Create transaction
                    TradeTransaction trade = new TradeTransaction
                    {
                        Exporter = partner,
                        Importer = importer,
                        ResourceName = resourceName,
                        Amount = tradeAmount,
                        ReceivedAmount = actualAmount
                    };
                    
                    // Add the trade
                    trades.Add(trade);
                    
                    // Update trading partners
                    if (!currentPartners.Contains(partner.regionName))
                    {
                        currentPartners.Add(partner.regionName);
                    }
                    
                    if (!tradingPartnersByRegion[partner.regionName].Contains(regionName))
                    {
                        tradingPartnersByRegion[partner.regionName].Add(regionName);
                    }
                    
                    // Reduce deficit
                    deficitAmount -= actualAmount;
                    
                    // Stop if deficit satisfied
                    if (deficitAmount <= 0) break;
                }
            }
        }
        
        // Verify partner limits weren't exceeded
        VerifyPartnerLimits();
        
        return trades;
    }
    
    private Dictionary<string, float> CalculateDeficits(RegionEntity region)
    {
        Dictionary<string, float> deficits = new Dictionary<string, float>();
        
        // Get consumption rates
        var consumption = region.resources.GetAllConsumptionRates();
        
        // Get current resources
        var currentResources = region.resources.GetAllResources();
        
        // Calculate deficits (consumption > current resources)
        foreach (var entry in consumption)
        {
            string resourceName = entry.Key;
            float consumptionRate = entry.Value;
            
            float currentAmount = 0;
            if (currentResources.ContainsKey(resourceName))
                currentAmount = currentResources[resourceName];
            
            // If consumption exceeds current resource, it's a deficit
            if (consumptionRate > currentAmount)
            {
                deficits[resourceName] = consumptionRate - currentAmount;
            }
        }
        
        return deficits;
    }
    
    private List<RegionEntity> FindTradingPartners(RegionEntity region, string resourceName)
    {
        List<RegionEntity> partners = new List<RegionEntity>();
        
        // Skip if no position data (we need it to calculate distance)
        Vector2 regionPos = GetRegionPosition(region.regionName);
        bool useDistance = regionPos != Vector2.negativeInfinity;
        
        // Find partners with surplus
        foreach (var potentialPartner in regions.Values)
        {
            // Skip self
            if (potentialPartner.regionName == region.regionName)
                continue;
                
            // Skip regions without resources
            if (potentialPartner.resources == null)
                continue;
            
            // Check distance
            if (useDistance)
            {
                Vector2 partnerPos = GetRegionPosition(potentialPartner.regionName);
                if (partnerPos != Vector2.negativeInfinity)
                {
                    float distance = Vector2.Distance(regionPos, partnerPos);
                    if (distance > tradeRadius)
                    {
                        continue; // Too far to trade
                    }
                }
            }
                
            // Check if partner has a surplus of this resource
            var partnerResources = potentialPartner.resources.GetAllResources();
            var partnerConsumption = potentialPartner.resources.GetAllConsumptionRates();
            
            if (partnerResources.ContainsKey(resourceName))
            {
                float available = partnerResources[resourceName];
                float needed = 0;
                
                if (partnerConsumption.ContainsKey(resourceName))
                    needed = partnerConsumption[resourceName];
                
                // If partner has more than it needs, it can trade
                if (available > needed * 1.2f) // 20% buffer
                {
                    partners.Add(potentialPartner);
                }
            }
        }
        
        return partners;
    }
    
    // Get region position from GameObject
    private Vector2 GetRegionPosition(string regionName)
    {
        GameObject regionObj = GameObject.Find(regionName);
        if (regionObj != null)
        {
            return regionObj.transform.position;
        }
        return Vector2.negativeInfinity;
    }
    
    // Calculate how much can be traded
    private (float, float) CalculateTradeAmount(RegionEntity exporter, string resourceName, float deficitAmount)
    {
        var resources = exporter.resources.GetAllResources();
        var consumption = exporter.resources.GetAllConsumptionRates();
        
        float available = resources.ContainsKey(resourceName) ? resources[resourceName] : 0;
        float needed = consumption.ContainsKey(resourceName) ? consumption[resourceName] : 0;
        
        // Calculate surplus (keep 20% buffer)
        float surplus = available - needed * 1.2f;
        
        // Can't trade if no surplus
        if (surplus <= 0) return (0, 0);
        
        // Calculate trade amount
        float tradeAmount = Mathf.Min(surplus, deficitAmount);
        
        // Apply trade efficiency
        float actualAmount = tradeAmount * tradeEfficiency;
        
        return (tradeAmount, actualAmount);
    }
    
    private float CalculateSurplus(RegionEntity region, string resourceName)
    {
        var resources = region.resources.GetAllResources();
        var consumption = region.resources.GetAllConsumptionRates();
        
        float available = resources.ContainsKey(resourceName) ? resources[resourceName] : 0;
        float needed = consumption.ContainsKey(resourceName) ? consumption[resourceName] : 0;
        
        return available - needed * 1.2f;
    }
    
    // Verification method to catch any violations
    private void VerifyPartnerLimits()
    {
        foreach (var entry in tradingPartnersByRegion)
        {
            string regionName = entry.Key;
            HashSet<string> partners = entry.Value;
            
            if (partners.Count > maxTradingPartnersPerRegion)
            {
                Debug.LogError($"LIMIT EXCEEDED: Region {regionName} has {partners.Count} partners, max is {maxTradingPartnersPerRegion}!");
                
                // Log all partners for debugging
                Debug.LogError($"Partners for {regionName}: {string.Join(", ", partners)}");
            }
        }
    }
    
    // For debugging
    public Dictionary<string, int> GetTradingPartnersCount()
    {
        Dictionary<string, int> counts = new Dictionary<string, int>();
        
        foreach (var entry in tradingPartnersByRegion)
        {
            counts[entry.Key] = entry.Value.Count;
        }
        
        return counts;
    }
}