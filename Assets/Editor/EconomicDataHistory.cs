using UnityEngine;
using System.Collections.Generic;
using V2.Entities;

namespace V2.Editor
{
    /// <summary>
    /// Class for managing and recording history of economic data for graphing
    /// </summary>
    public class EconomicDataHistory
    {
        // Common settings
        private const int maxHistoryPoints = 50;
        
        // Main economic metric histories
        public List<int> wealthHistory = new List<int>();
        public List<int> productionHistory = new List<int>();
        public List<float> satisfactionHistory = new List<float>();
        
        // Market dynamics histories
        public List<float> supplyHistory = new List<float>();
        public List<float> demandHistory = new List<float>();
        public List<float> imbalanceHistory = new List<float>();
        
        // Parameter histories
        public List<float> productivityHistory = new List<float>();
        public List<float> laborElasticityHistory = new List<float>();
        public List<float> capitalElasticityHistory = new List<float>();
        public List<float> cycleMultiplierHistory = new List<float>();
        
        /// <summary>
        /// Record current economic data from a region entity
        /// </summary>
        public void RecordHistory(RegionEntity region)
        {
            if (region == null) return;
            
            // Check if values have changed before adding to avoid duplicate entries
            bool shouldRecord = wealthHistory.Count == 0 || 
                               (region.Economy.Wealth != wealthHistory[wealthHistory.Count - 1] || 
                                region.Economy.Production != productionHistory[productionHistory.Count - 1] || 
                                !Mathf.Approximately(region.Population.Satisfaction, satisfactionHistory[satisfactionHistory.Count - 1]));
            
            if (shouldRecord)
            {
                wealthHistory.Add(region.Economy.Wealth);
                productionHistory.Add(region.Economy.Production);
                satisfactionHistory.Add(region.Population.Satisfaction);
                
                // Calculate and record supply, demand and imbalance
                float supply = region.Economy.Production * 0.8f;
                float demand = region.Population.LaborAvailable * 1.2f;
                float imbalance = supply - demand;
                
                supplyHistory.Add(supply);
                demandHistory.Add(demand);
                imbalanceHistory.Add(imbalance);
                
                // Keep lists at a reasonable size
                TruncateHistories();
            }
        }
        
        /// <summary>
        /// Record current parameter values
        /// </summary>
        public void RecordParameterHistory(EconomicParameters parameters)
        {
            if (parameters == null) return;
            
            // Add current parameter values to history
            productivityHistory.Add(parameters.productivityFactor);
            laborElasticityHistory.Add(parameters.laborElasticity);
            capitalElasticityHistory.Add(parameters.capitalElasticity);
            cycleMultiplierHistory.Add(parameters.cycleMultiplier);
            
            // Keep lists at a reasonable size
            TruncateParameterHistories();
        }
        
        /// <summary>
        /// Clear all history data
        /// </summary>
        public void ClearAll()
        {
            wealthHistory.Clear();
            productionHistory.Clear();
            satisfactionHistory.Clear();
            supplyHistory.Clear();
            demandHistory.Clear();
            imbalanceHistory.Clear();
            
            productivityHistory.Clear();
            laborElasticityHistory.Clear();
            capitalElasticityHistory.Clear();
            cycleMultiplierHistory.Clear();
        }
        
        /// <summary>
        /// Get parameter history by index
        /// </summary>
        public List<float> GetParameterHistoryByIndex(int index)
        {
            switch (index)
            {
                case 0: return productivityHistory;
                case 1: return laborElasticityHistory;
                case 2: return capitalElasticityHistory;
                case 3: return cycleMultiplierHistory;
                default: return productivityHistory;
            }
        }
        
        /// <summary>
        /// Get maximum values for all metrics (for scaling graphs)
        /// </summary>
        public void GetMaxValues(out int maxWealth, out int maxProduction, 
                                out float maxSupply, out float maxDemand, out float maxImbalance)
        {
            // Default minimum values
            maxWealth = 100;
            maxProduction = 50;
            maxSupply = 50f;
            maxDemand = 50f;
            maxImbalance = 50f;
            
            // Calculate actual maximum values if we have history data
            if (wealthHistory.Count > 0)
            {
                foreach (int value in wealthHistory)
                    maxWealth = Mathf.Max(maxWealth, value);
                
                foreach (int value in productionHistory)
                    maxProduction = Mathf.Max(maxProduction, value);
                
                // Find max values for supply/demand/imbalance
                foreach (float value in supplyHistory)
                    maxSupply = Mathf.Max(maxSupply, value);
                    
                foreach (float value in demandHistory)
                    maxDemand = Mathf.Max(maxDemand, value);
                
                // For imbalance, find the maximum absolute value
                foreach (float value in imbalanceHistory)
                    maxImbalance = Mathf.Max(maxImbalance, Mathf.Abs(value));
                    
                // Add some headroom (10%) so the max value isn't right at the top
                maxWealth = (int)(maxWealth * 1.1f);
                maxProduction = (int)(maxProduction * 1.1f);
                maxSupply *= 1.1f;
                maxDemand *= 1.1f;
                maxImbalance *= 1.1f;
            }
        }
        
        /// <summary>
        /// Keep main history lists at a reasonable size
        /// </summary>
        private void TruncateHistories()
        {
            if (wealthHistory.Count > maxHistoryPoints)
            {
                wealthHistory.RemoveAt(0);
                productionHistory.RemoveAt(0);
                satisfactionHistory.RemoveAt(0);
                supplyHistory.RemoveAt(0);
                demandHistory.RemoveAt(0);
                imbalanceHistory.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Keep parameter history lists at a reasonable size
        /// </summary>
        private void TruncateParameterHistories()
        {
            if (productivityHistory.Count > maxHistoryPoints)
            {
                productivityHistory.RemoveAt(0);
                laborElasticityHistory.RemoveAt(0);
                capitalElasticityHistory.RemoveAt(0);
                cycleMultiplierHistory.RemoveAt(0);
            }
        }
    }
}