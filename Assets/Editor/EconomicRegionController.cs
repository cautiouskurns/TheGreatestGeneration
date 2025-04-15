using UnityEngine;
using V2.Entities;
using V2.Components;
using System.Reflection;

namespace V2.Editor
{
    /// <summary>
    /// Helper class for controlling region parameters in the economic simulation
    /// </summary>
    public class EconomicRegionController
    {
        // Region control values
        public int laborAvailable = 100;
        public int infrastructureLevel = 1;
        
        /// <summary>
        /// Apply region control values to a region entity
        /// </summary>
        public void ApplyToRegion(RegionEntity region, bool simulationActive)
        {
            if (region == null || !simulationActive) return;
            
            // Apply labor (if different)
            if (region.Population.LaborAvailable != laborAvailable)
            {
                region.Population.UpdateLabor(laborAvailable - region.Population.LaborAvailable);
            }
            
            // Apply infrastructure (if different)
            if (region.Infrastructure.Level != infrastructureLevel)
            {
                SetInfrastructureLevel(region.Infrastructure, infrastructureLevel);
            }
        }
        
        /// <summary>
        /// Sync control values from a region entity
        /// </summary>
        public void SyncFromRegion(RegionEntity region)
        {
            if (region == null) return;
            
            laborAvailable = region.Population.LaborAvailable;
            infrastructureLevel = region.Infrastructure.Level;
        }
        
        /// <summary>
        /// Reset a region to default values
        /// </summary>
        public void ResetRegion(RegionEntity region)
        {
            if (region == null) return;
            
            // Reset component values
            region.Economy.Wealth = 100;
            region.Economy.Production = 50;
            region.Population.UpdateSatisfaction(1.0f);
            
            // Reset population and infrastructure
            region.Population.UpdateLabor(100 - region.Population.LaborAvailable);
            SetInfrastructureLevel(region.Infrastructure, 1);
            
            // Sync values back to controller
            SyncFromRegion(region);
        }
        
        /// <summary>
        /// Set infrastructure level using reflection to access Upgrade method
        /// </summary>
        private void SetInfrastructureLevel(InfrastructureComponent component, int level)
        {
            if (component == null) return;
            
            // Use Upgrade method if available
            MethodInfo method = typeof(InfrastructureComponent).GetMethod("Upgrade");
            if (method != null)
            {
                // Call Upgrade method until we reach desired level
                while (component.Level < level)
                {
                    method.Invoke(component, null);
                }
            }
        }
    }
}