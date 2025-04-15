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
        private const int maxHistoryPoints = 5000;
        
        // Main economic metric histories
        public List<int> wealthHistory = new List<int>();
        public List<int> productionHistory = new List<int>();
        public List<float> satisfactionHistory = new List<float>();
        
        // Market dynamics histories
        public List<float> supplyHistory = new List<float>();
        public List<float> demandHistory = new List<float>();
        public List<float> imbalanceHistory = new List<float>();
        
        // Parameter histories - dictionary approach for more flexible parameter tracking
        private Dictionary<string, List<float>> parameterHistories = new Dictionary<string, List<float>>();

        public EconomicDataHistory()
        {
            // Initialize parameter histories for all tracked parameters
            foreach (string paramName in EconomicParameters.ParameterNames)
            {
                parameterHistories[paramName.ToLower()] = new List<float>();
            }
        }
        
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
            
            // Add each parameter to its respective history
            AddParameterValue("productivity", parameters.productivityFactor);
            AddParameterValue("labor elasticity", parameters.laborElasticity);
            AddParameterValue("capital elasticity", parameters.capitalElasticity);
            AddParameterValue("cycle multiplier", parameters.cycleMultiplier);
            AddParameterValue("wealth growth rate", parameters.wealthGrowthRate);
            AddParameterValue("price volatility", parameters.priceVolatility);
            AddParameterValue("decay rate", parameters.decayRate);
            AddParameterValue("maintenance cost multiplier", parameters.maintenanceCostMultiplier);
            AddParameterValue("labor consumption rate", parameters.laborConsumptionRate);
            
            // Keep lists at a reasonable size
            TruncateParameterHistories();
        }
        
        /// <summary>
        /// Add a value to a parameter history
        /// </summary>
        private void AddParameterValue(string parameterKey, float value)
        {
            if (!parameterHistories.ContainsKey(parameterKey))
            {
                parameterHistories[parameterKey] = new List<float>();
            }
            
            parameterHistories[parameterKey].Add(value);
        }
        
        /// <summary>
        /// Get parameter history by name
        /// </summary>
        public List<float> GetParameterHistory(string parameterName)
        {
            string key = parameterName.ToLower();
            if (parameterHistories.ContainsKey(key))
            {
                return parameterHistories[key];
            }
            
            // If not found, create empty list for this parameter
            parameterHistories[key] = new List<float>();
            return parameterHistories[key];
        }
        
        /// <summary>
        /// Get all parameter histories
        /// </summary>
        public Dictionary<string, List<float>> GetAllParameterHistories()
        {
            return parameterHistories;
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
            
            // Clear all parameter histories
            foreach (var history in parameterHistories.Values)
            {
                history.Clear();
            }
        }
        
        /// <summary>
        /// Get parameter history by index (for backward compatibility)
        /// </summary>
        public List<float> GetParameterHistoryByIndex(int index)
        {
            string paramName = EconomicParameters.ParameterNames[index].ToLower();
            return GetParameterHistory(paramName);
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
        /// Get maximum value for a specific parameter history
        /// </summary>
        public float GetParameterMaxValue(string parameterName)
        {
            List<float> history = GetParameterHistory(parameterName.ToLower());
            
            if (history == null || history.Count == 0)
                return 1.0f;
                
            float max = 0f;
            foreach (float value in history)
            {
                max = Mathf.Max(max, value);
            }
            
            // Add some headroom
            return max * 1.1f;
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
            foreach (var history in parameterHistories.Values)
            {
                if (history.Count > maxHistoryPoints)
                {
                    history.RemoveAt(0);
                }
            }
        }
    }
}