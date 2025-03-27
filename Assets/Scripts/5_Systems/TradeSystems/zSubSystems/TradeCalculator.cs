using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Class for handling trade calculations
public class TradeCalculator
{
    private readonly Dictionary<string, RegionEntity> regions;
    private readonly float tradeEfficiency;
    private readonly int maxTradingPartnersPerRegion;
    private readonly float tradeRadius; // Added trade radius parameter
    
    // Track trading partners for each region across all resources
    private Dictionary<string, HashSet<string>> tradingPartnersByRegion = new Dictionary<string, HashSet<string>>();
    
    public TradeCalculator(
        Dictionary<string, RegionEntity> regions,
        float tradeEfficiency,
        int maxTradingPartnersPerRegion,
        float tradeRadius) // Added trade radius parameter
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
        
        // Debug check - log all regions with deficits
        Debug.Log($"TradeCalculator: Found {allDeficits.Count} regions with resource deficits");
        
        // Second pass: Process trades in order of most critical needs first
        // Process regions with the fewest deficits first to give them priority in partner selection
        foreach (var regionEntry in allDeficits.OrderBy(r => r.Value.Count))
        {
            string regionName = regionEntry.Key;
            var deficits = regionEntry.Value;
            
            if (!regions.ContainsKey(regionName))
            {
                Debug.LogWarning($"TradeCalculator: Region {regionName} not found in regions dictionary");
                continue;
            }
            
            RegionEntity importer = regions[regionName];
            
            // Debug check - count current trading partners
            int currentPartnerCount = tradingPartnersByRegion[regionName].Count;
            Debug.Log($"TradeCalculator: Processing {regionName} with {deficits.Count} deficits and {currentPartnerCount} current partners");
            
            // Skip if already at max partners
            if (currentPartnerCount >= maxTradingPartnersPerRegion)
            {
                Debug.Log($"TradeCalculator: Region {regionName} already at max partners ({maxTradingPartnersPerRegion})");
                continue;
            }
            
            // Process each deficit for this region
            foreach (var entry in deficits.OrderByDescending(d => d.Value))
            {
                string resourceName = entry.Key;
                float deficitAmount = entry.Value;
                
                // Skip if already at max partners after processing previous resources
                if (tradingPartnersByRegion[regionName].Count >= maxTradingPartnersPerRegion)
                {
                    Debug.Log($"TradeCalculator: Region {regionName} reached max partners while processing resource {resourceName}");
                    break;
                }
                
                // Find potential trade partners for this resource
                var tradingPartners = FindTradingPartners(importer, resourceName);
                
                // Process imports while respecting partner limits
                var newTrades = ProcessImportsFromPartners(importer, resourceName, deficitAmount, tradingPartners);
                
                // Add new trades to the result list
                trades.AddRange(newTrades);
                
                // Debug check - update partner count
                currentPartnerCount = tradingPartnersByRegion[regionName].Count;
                Debug.Log($"TradeCalculator: After processing {resourceName}, {regionName} now has {currentPartnerCount} partners");
            }
        }
        
        // Final verification of partner constraints
        VerifyPartnerConstraints();
        
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
        if (regionPos == Vector2.negativeInfinity) 
        {
            // Fall back to checking all regions if positions are not available
            return FindAllPotentialPartners(region, resourceName);
        }
        
        // Find partners within trade radius that have a surplus
        foreach (var potentialPartner in regions.Values)
        {
            // Skip self
            if (potentialPartner.regionName == region.regionName)
                continue;
                
            // Skip regions without resources
            if (potentialPartner.resources == null)
                continue;
            
            // Check distance (if we have position data)
            Vector2 partnerPos = GetRegionPosition(potentialPartner.regionName);
            if (partnerPos != Vector2.negativeInfinity)
            {
                float distance = Vector2.Distance(regionPos, partnerPos);
                if (distance > tradeRadius)
                {
                    // Too far to trade
                    continue;
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
    
    // Fallback method to find partners without using distance
    private List<RegionEntity> FindAllPotentialPartners(RegionEntity region, string resourceName)
    {
        List<RegionEntity> partners = new List<RegionEntity>();
        
        // Check all regions for ones with a surplus
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
    
    // Get region position, returns Vector2.negativeInfinity if no position data available
    private Vector2 GetRegionPosition(string regionName)
    {
        // Look for a GameObject with the region name
        GameObject regionObj = GameObject.Find(regionName);
        if (regionObj != null)
        {
            return regionObj.transform.position;
        }
        
        return Vector2.negativeInfinity;
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
        
        // Make sure we don't exceed the maximum number of partners
        int availablePartnerSlots = maxTradingPartnersPerRegion - currentPartners.Count;
        if (availablePartnerSlots <= 0)
        {
            // Already at max partners, no more trades possible
            Debug.Log($"TradeCalculator: {importer.regionName} already has {currentPartners.Count} partners, max is {maxTradingPartnersPerRegion}");
            return trades;
        }
        
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
            
        // Limit new partners to available slots
        if (newPartners.Count > availablePartnerSlots)
        {
            newPartners = newPartners.Take(availablePartnerSlots).ToList();
        }
        
        // Add these to our ordered list
        orderedPartners.AddRange(newPartners);
        
        float remainingDeficit = deficitAmount;
        
        foreach (var partner in orderedPartners)
        {
            // Make absolutely sure we don't exceed partner limits
            if (!currentPartners.Contains(partner.regionName) && 
                currentPartners.Count >= maxTradingPartnersPerRegion)
            {
                Debug.Log($"TradeCalculator: Skipping new partner {partner.regionName} for {importer.regionName} due to partner limit");
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
    
    // Verify that no region has more partners than allowed
    private void VerifyPartnerConstraints()
    {
        foreach (var entry in tradingPartnersByRegion)
        {
            string regionName = entry.Key;
            int partnerCount = entry.Value.Count;
            
            if (partnerCount > maxTradingPartnersPerRegion)
            {
                Debug.LogError($"TradeCalculator: Region {regionName} has {partnerCount} partners, exceeding max of {maxTradingPartnersPerRegion}");
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