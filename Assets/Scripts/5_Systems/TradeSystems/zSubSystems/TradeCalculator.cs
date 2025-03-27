using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Class for handling trade calculations
public class TradeCalculator
{
    private readonly Dictionary<string, RegionEntity> regions;
    private readonly float tradeEfficiency;
    private readonly int maxTradingPartnersPerRegion;
    
    // Track trading partners for each region across all resources
    private Dictionary<string, HashSet<string>> tradingPartnersByRegion = new Dictionary<string, HashSet<string>>();
    
    public TradeCalculator(
        Dictionary<string, RegionEntity> regions,
        float tradeEfficiency,
        int maxTradingPartnersPerRegion)
    {
        this.regions = regions;
        this.tradeEfficiency = tradeEfficiency;
        this.maxTradingPartnersPerRegion = maxTradingPartnersPerRegion;
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
        
        // Second pass: Process trades in order of most critical needs first
        // Process regions with the fewest deficits first to give them priority in partner selection
        foreach (var regionEntry in allDeficits.OrderBy(r => r.Value.Count))
        {
            string regionName = regionEntry.Key;
            var deficits = regionEntry.Value;
            RegionEntity importer = regions[regionName];
            
            // Process each deficit for this region
            foreach (var entry in deficits)
            {
                string resourceName = entry.Key;
                float deficitAmount = entry.Value;
                
                // Find potential trade partners for this resource
                var tradingPartners = FindTradingPartners(importer, resourceName);
                
                // Process imports while respecting partner limits
                trades.AddRange(
                    ProcessImportsFromPartners(importer, resourceName, deficitAmount, tradingPartners));
            }
        }
        
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
        
        // In a simple implementation, just check all regions
        // In a more complex version, you'd use a region adjacency graph
        foreach (var potentialPartner in regions.Values)
        {
            // Skip self
            if (potentialPartner.regionName == region.regionName)
                continue;
                
            // Skip regions without resources
            if (potentialPartner.resources == null)
                continue;
                
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
    
    private List<TradeTransaction> ProcessImportsFromPartners(
        RegionEntity importer, 
        string resourceName, 
        float deficitAmount, 
        List<RegionEntity> partners)
    {
        List<TradeTransaction> trades = new List<TradeTransaction>();
        
        // Get current trading partners for this importer
        HashSet<string> currentPartners = tradingPartnersByRegion[importer.regionName];
        
        // First, prioritize partners we're already trading with
        List<RegionEntity> orderedPartners = new List<RegionEntity>();
        
        // Add existing partners first
        foreach (var partner in partners)
        {
            if (currentPartners.Contains(partner.regionName))
            {
                orderedPartners.Add(partner);
            }
        }
        
        // Then add new potential partners, sorted by surplus
        List<RegionEntity> newPartners = partners
            .Where(p => !currentPartners.Contains(p.regionName))
            .OrderByDescending(p => CalculateSurplus(p, resourceName))
            .ToList();
            
        // Add these to our ordered list
        orderedPartners.AddRange(newPartners);
        
        float remainingDeficit = deficitAmount;
        
        foreach (var partner in orderedPartners)
        {
            // Check if we've reached our partner limit AND this isn't an existing partner
            if (currentPartners.Count >= maxTradingPartnersPerRegion && 
                !currentPartners.Contains(partner.regionName))
            {
                // Skip this partner because we're at our limit
                continue;
            }
            
            if (remainingDeficit <= 0)
                break;
                
            // Calculate how much this partner can provide
            var partnerResources = partner.resources.GetAllResources();
            var partnerConsumption = partner.resources.GetAllConsumptionRates();
            
            float partnerAvailable = partnerResources.ContainsKey(resourceName) ? 
                                    partnerResources[resourceName] : 0;
            float partnerNeeded = partnerConsumption.ContainsKey(resourceName) ? 
                                partnerConsumption[resourceName] : 0;
            
            // Skip if partner doesn't have this resource
            if (partnerAvailable <= 0) continue;
            
            float surplus = partnerAvailable - partnerNeeded * 1.2f; // Keep 20% buffer
            
            // Only trade if there's an actual surplus
            if (surplus <= 0) continue;
            
            float tradeAmount = Mathf.Min(surplus, remainingDeficit);
            
            // Apply trade efficiency (representing logistics/transport cost)
            float actualTradeAmount = tradeAmount * tradeEfficiency;
            
            // Skip meaningless trades
            if (actualTradeAmount < 0.1f) continue;
            
            // Create a trade transaction
            TradeTransaction trade = new TradeTransaction
            {
                Exporter = partner,
                Importer = importer,
                ResourceName = resourceName,
                Amount = tradeAmount,
                ReceivedAmount = actualTradeAmount
            };
            
            trades.Add(trade);
            
            // Add to trading partners for both regions
            currentPartners.Add(partner.regionName);
            tradingPartnersByRegion[partner.regionName].Add(importer.regionName);
            
            // Update remaining deficit
            remainingDeficit -= actualTradeAmount;
        }
        
        return trades;
    }
    
    private float CalculateSurplus(RegionEntity region, string resourceName)
    {
        var resources = region.resources.GetAllResources();
        var consumption = region.resources.GetAllConsumptionRates();
        
        float available = resources.ContainsKey(resourceName) ? resources[resourceName] : 0;
        float needed = consumption.ContainsKey(resourceName) ? consumption[resourceName] : 0;
        
        return available - needed * 1.2f;
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