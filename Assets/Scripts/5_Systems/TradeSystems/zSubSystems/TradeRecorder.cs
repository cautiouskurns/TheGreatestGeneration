using UnityEngine;
using System.Collections.Generic;

       // Class for recording and tracking trade history
public class TradeRecorder
{
    private Dictionary<string, List<TradeInfo>> recentImports = new Dictionary<string, List<TradeInfo>>();
    private Dictionary<string, List<TradeInfo>> recentExports = new Dictionary<string, List<TradeInfo>>();
    private Dictionary<string, int> regionTradeVolume = new Dictionary<string, int>();
    
    public void RecordTrade(TradeTransaction trade)
    {
        // Record as import for the importer
        RecordImport(trade.Importer.regionName, trade.Exporter.regionName, trade.ResourceName, trade.ReceivedAmount);
        
        // Record as export for the exporter
        RecordExport(trade.Exporter.regionName, trade.Importer.regionName, trade.ResourceName, trade.Amount);
        
        // Update trade volumes
        IncrementTradeVolume(trade.Importer.regionName);
        IncrementTradeVolume(trade.Exporter.regionName);
    }
    
    private void RecordImport(string importerName, string exporterName, string resourceName, float amount)
    {
        if (!recentImports.ContainsKey(importerName))
            recentImports[importerName] = new List<TradeInfo>();
        
        recentImports[importerName].Add(new TradeInfo
        {
            partnerName = exporterName,
            resourceName = resourceName,
            amount = amount
        });
    }
    
    private void RecordExport(string exporterName, string importerName, string resourceName, float amount)
    {
        if (!recentExports.ContainsKey(exporterName))
            recentExports[exporterName] = new List<TradeInfo>();
        
        recentExports[exporterName].Add(new TradeInfo
        {
            partnerName = importerName,
            resourceName = resourceName,
            amount = amount
        });
    }
    
    private void IncrementTradeVolume(string regionName)
    {
        if (!regionTradeVolume.ContainsKey(regionName))
            regionTradeVolume[regionName] = 0;
        
        regionTradeVolume[regionName]++;
    }
    
    public List<TradeInfo> GetRecentImports(string regionName)
    {
        return recentImports.ContainsKey(regionName) 
            ? recentImports[regionName] 
            : new List<TradeInfo>();
    }
    
    public List<TradeInfo> GetRecentExports(string regionName)
    {
        return recentExports.ContainsKey(regionName) 
            ? recentExports[regionName]
            : new List<TradeInfo>();
    }
    
    public Dictionary<string, List<TradeInfo>> GetAllExports()
    {
        return new Dictionary<string, List<TradeInfo>>(recentExports);
    }
    
    public Dictionary<string, List<TradeInfo>> GetAllImports()
    {
        return new Dictionary<string, List<TradeInfo>>(recentImports);
    }
    
    public int GetTradeVolume(string regionName)
    {
        return regionTradeVolume.ContainsKey(regionName) 
            ? regionTradeVolume[regionName] 
            : 0;
    }
    
    public Color GetRegionTradeColor(string regionName, Color minColor, Color maxColor)
    {
        if (!regionTradeVolume.ContainsKey(regionName))
            return Color.white;
        
        // Find max trade volume for normalization
        int maxVolume = 1;
        foreach (var volume in regionTradeVolume.Values)
        {
            if (volume > maxVolume)
                maxVolume = volume;
        }
        
        // Calculate normalized intensity
        float intensity = (float)regionTradeVolume[regionName] / maxVolume;
        
        // Return color from blue (low) to red (high)
        return Color.Lerp(minColor, maxColor, intensity);
    }
    
    public void ClearTradeData()
    {
        recentImports.Clear();
        recentExports.Clear();
        regionTradeVolume.Clear();
    }
}
