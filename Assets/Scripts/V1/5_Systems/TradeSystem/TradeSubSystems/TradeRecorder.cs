using UnityEngine;
using System.Collections.Generic;

namespace V1.Systems
{ 
    /// CLASS PURPOSE:
    /// TradeRecorder tracks historical trade data across the game world, including
    /// imports, exports, and trade volume per region. It is used to drive trade analytics,
    /// influence gameplay systems, or provide visual feedback.
    /// 
    /// CORE RESPONSIBILITIES:
    /// - Record and store trade transactions for both importers and exporters
    /// - Track trade volume per region to support data-driven gameplay
    /// - Provide access to recent trade history and aggregate statistics
    /// - Compute trade volume-based coloring for visual overlays
    /// 
    /// KEY COLLABORATORS:
    /// - TradeTransaction: Supplies trade data per turn
    /// - RegionEntity: Indirectly associated through trade activity
    /// - UI Systems: Use trade history and coloring for visual overlays and summaries
    /// 
    /// CURRENT ARCHITECTURE NOTES:
    /// - Internally uses dictionaries keyed by region name for fast lookup
    /// - TradeInfo used as a lightweight container for trade entries
    /// - Color interpolation based on relative trade activity
    /// 
    /// REFACTORING SUGGESTIONS:
    /// - Replace region name strings with GUIDs or references to avoid key mismatches
    /// - Add limits or rolling windows to bound trade history lists
    /// - Abstract color logic to a separate visual layer
    /// 
    /// EXTENSION OPPORTUNITIES:
    /// - Support trade partner preferences or diplomatic modifiers
    /// - Track historical changes over time for economic trend graphs
    /// - Integrate with AI decision systems or achievements
    /// 
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
}