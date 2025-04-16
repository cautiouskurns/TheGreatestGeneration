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
            
            // Get inputs for production calculation
            float labor = region.Population.LaborAvailable;
            float capital = region.Infrastructure.Level;
            float productivityFactor = economicSystem.productivityFactor;
            float laborElasticity = economicSystem.laborElasticity;
            float capitalElasticity = economicSystem.capitalElasticity;

            // Cobb-Douglas production function
            float production = Calculate(labor, capital, productivityFactor, laborElasticity, capitalElasticity);

            // Update output in both Production and Economy components
            int productionOutput = Mathf.RoundToInt(production);
            
            // Update the Production component using the new SetOutput method
            region.Production.SetOutput(productionOutput);
            
            // Update the Economy component's Production value
            region.Economy.Production = productionOutput;
            
            Debug.Log($"[Production] Labor: {labor}, Capital: {capital}, Productivity: {productivityFactor}, " +
                     $"Labor Elasticity: {laborElasticity}, Capital Elasticity: {capitalElasticity}, " +
                     $"Production Output: {productionOutput}");
        }
        
        /// <summary>
        /// Calculates production using the Cobb-Douglas production function
        /// </summary>
        public float Calculate(float labor, float capital, float productivity, float laborElasticity, float capitalElasticity)
        {
            // Guard against zero values to prevent NaN results
            labor = Mathf.Max(1f, labor);
            capital = Mathf.Max(1f, capital);
            
            // Cobb-Douglas production function: Y = A * L^α * K^β
            // Where:
            // Y = Production output
            // A = Total factor productivity
            // L = Labor input
            // K = Capital input
            // α = Output elasticity of labor
            // β = Output elasticity of capital
            return productivity * Mathf.Pow(labor, laborElasticity) * Mathf.Pow(capital, capitalElasticity);
        }
    }
}