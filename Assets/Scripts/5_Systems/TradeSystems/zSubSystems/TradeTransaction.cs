using UnityEngine;

public class TradeTransaction
{
    public RegionEntity Exporter { get; set; }
    public RegionEntity Importer { get; set; }
    public string ResourceName { get; set; }
    public float Amount { get; set; }
    public float ReceivedAmount { get; set; } // After efficiency applied
    
    // Execute this trade (transfer resources and generate wealth)
    public void Execute()
    {
        // Transfer resources
        Exporter.resources.RemoveResource(ResourceName, Amount);
        Importer.resources.AddResource(ResourceName, ReceivedAmount);
        
        // Generate wealth for both parties based on trade
        float tradeValue = ReceivedAmount * 0.5f; // Simple wealth generation from trade
        Exporter.wealth += Mathf.RoundToInt(tradeValue);
        Importer.wealth += Mathf.RoundToInt(tradeValue * 0.5f); // Importer gets some wealth too
    }
}