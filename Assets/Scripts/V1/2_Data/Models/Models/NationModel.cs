using System.Collections.Generic;
using UnityEngine;
using V1.Entities;
using V1.Managers;

namespace V1.Data
{
/// CLASS PURPOSE:
/// NationModel is responsible for managing all nations in the game world,
/// initializing their data from static map configuration and tracking nation-level information.
/// It acts as the central registry for nations and supports selection, updates, and turn progression.
///
/// CORE RESPONSIBILITIES:
/// - Initialize NationEntity instances from MapDataSO
/// - Register regions to appropriate nations
/// - Support nation selection and notify systems via EventBus
/// - Update nation-level economic and resource data each turn
/// - Aggregate global resource balances across all nations
///
/// KEY COLLABORATORS:
/// - MapDataSO: Supplies nation metadata (name, color)
/// - RegionEntity: Represents constituent parts of each nation
/// - NationEntity: Stores nation-level logic and aggregates
/// - EventBus: Dispatches changes like selection or updates
///
/// CURRENT ARCHITECTURE NOTES:
/// - Regions must call RegisterRegion after instantiation
/// - Uses dictionary for fast nation lookups by name
/// - Fires events on nation selection and update
///
/// REFACTORING SUGGESTIONS:
/// - Consider making selection logic event-driven (e.g., subscribe to RegionSelected)
/// - Add error handling for duplicate nation entries
/// - Move GetGlobalResourceBalance to a dedicated economy manager if it grows
///
/// EXTENSION OPPORTUNITIES:
/// - Add support for AI strategies and behaviors per nation
/// - Include diplomatic relationships or alliances
/// - Track nation-level historical metrics across turns
/// <summary>
/// Manages all nations in the game world
/// </summary>
/// 
    public class NationModel
    {
        private Dictionary<string, NationEntity> nations = new Dictionary<string, NationEntity>();
        private NationEntity selectedNation;
        
        // Initialize the nation model from map data
        public NationModel(MapDataSO mapData)
        {
            InitializeNations(mapData);
        }
        
        // Create nation entities from MapDataSO
        private void InitializeNations(MapDataSO mapData)
        {
            if (mapData == null || mapData.nations == null)
            {
                Debug.LogError("Invalid map data for nation initialization");
                return;
            }
            
            // Create nations from map data
            foreach (var nationData in mapData.nations)
            {
                NationEntity nation = new NationEntity(nationData.nationName, nationData.nationColor);
                nations.Add(nationData.nationName, nation);
            }
            
    //        Debug.Log($"NationModel: Initialized {nations.Count} nations");
        }
        
        // Add a region to its nation
        public void RegisterRegion(RegionEntity region)
        {
            if (region == null) return;
            
            string nationName = region.ownerNationName;
            if (string.IsNullOrEmpty(nationName)) return;
            
            if (nations.ContainsKey(nationName))
            {
                nations[nationName].AddRegion(region);
    //            Debug.Log($"Region {region.regionName} registered to nation {nationName}");
            }
            else
            {
                Debug.LogWarning($"Nation {nationName} not found for region {region.regionName}");
            }
        }
        
        // Get a nation by name
        public NationEntity GetNation(string nationName)
        {
            if (nations.ContainsKey(nationName))
            {
                return nations[nationName];
            }
            return null;
        }
        
        // Get all nations
        public Dictionary<string, NationEntity> GetAllNations()
        {
            return new Dictionary<string, NationEntity>(nations);
        }
        
        // Select a nation
        public void SelectNation(string nationName)
        {
            if (nations.ContainsKey(nationName))
            {
                selectedNation = nations[nationName];
                EventBus.Trigger("NationSelected", selectedNation);
            }
        }
        
        // Get currently selected nation
        public NationEntity GetSelectedNation()
        {
            return selectedNation;
        }
        
        // Update all nations' aggregated data
        public void UpdateAllNations()
        {
            foreach (var nation in nations.Values)
            {
                nation.UpdateAggregatedData();
            }
        }
        
        // Process turn for all nations
        public void ProcessTurn()
        {
            // This would be called after all regions have been processed
            // to update nation-level statistics
            UpdateAllNations();
            
            // Trigger event for UI updates
            EventBus.Trigger("NationModelUpdated", this);
        }
        
        // Get resource balance across all nations (for global market)
        public Dictionary<string, float> GetGlobalResourceBalance()
        {
            Dictionary<string, float> globalBalance = new Dictionary<string, float>();
            
            foreach (var nation in nations.Values)
            {
                var nationBalance = nation.GetResourceBalance();
                
                foreach (var entry in nationBalance)
                {
                    string resourceName = entry.Key;
                    float balance = entry.Value;
                    
                    if (!globalBalance.ContainsKey(resourceName))
                        globalBalance[resourceName] = 0;
                    
                    globalBalance[resourceName] += balance;
                }
            }
            
            return globalBalance;
        }
    }
}
