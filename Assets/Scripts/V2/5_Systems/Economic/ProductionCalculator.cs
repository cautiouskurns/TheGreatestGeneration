using UnityEngine;
using V2.Entities;
using V2.Components;

namespace V2.Systems.Economic
{
    /// <summary>
    /// Handles production calculations using the Cobb-Douglas function
    /// </summary>
    public class ProductionCalculator : EconomicSubsystem
    {
        public ProductionCalculator(EconomicSystem system) : base(system)
        {
        }

        public override void Process(RegionEntity region)
        {
            Debug.Log("Processing Production...");
            
            float labor = region.Population.LaborAvailable;
            float capital = region.Infrastructure.Level;
            float productivityFactor = economicSystem.productivityFactor;
            float laborElasticity = economicSystem.laborElasticity;
            float capitalElasticity = economicSystem.capitalElasticity;

            // Cobb-Douglas production function
            float production = Calculate(labor, capital, productivityFactor, laborElasticity, capitalElasticity);

            // Update output in ProductionComponent
            int productionOutput = Mathf.RoundToInt(production);
            
            // Set output through method
            SetProductionOutput(region.Production, productionOutput);
            
            Debug.Log($"[Production] Labor: {labor}, Capital: {capital}, Production: {productionOutput}");
        }
        
        /// <summary>
        /// Calculates production using the Cobb-Douglas production function
        /// </summary>
        public float Calculate(float labor, float capital, float productivity, float laborElasticity, float capitalElasticity)
        {
            return productivity * Mathf.Pow(labor, laborElasticity) * Mathf.Pow(capital, capitalElasticity);
        }
        
        // Helper method to set production output without breaking encapsulation
        private void SetProductionOutput(ProductionComponent productionComponent, int output)
        {
            // This would ideally be replaced by ProductionComponent's own calculation
            typeof(ProductionComponent).GetField("output", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance)?.SetValue(productionComponent, output);
        }
    }
}