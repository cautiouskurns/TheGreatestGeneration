using UnityEngine;
using System.Collections.Generic;

// Class for visualizing trade routes and activity
public class TradeVisualizer
{
    private readonly Color importColor;
    private readonly Color exportColor;
    private readonly float tradeLineWidth;
    private readonly float tradeLineDuration;
    private readonly float minimumTradeVolumeToShow;
    private readonly bool showTradeLines;
    
    private readonly List<GameObject> activeTradeLines = new List<GameObject>();
    
    public TradeVisualizer(
        Color importColor,
        Color exportColor,
        float tradeLineWidth,
        float tradeLineDuration,
        float minimumTradeVolumeToShow,
        bool showTradeLines)
    {
        this.importColor = importColor;
        this.exportColor = exportColor;
        this.tradeLineWidth = tradeLineWidth;
        this.tradeLineDuration = tradeLineDuration;
        this.minimumTradeVolumeToShow = minimumTradeVolumeToShow;
        this.showTradeLines = showTradeLines;
    }
    
    public void ShowTradeLine(RegionEntity from, RegionEntity to, Color color, float tradeAmount)
    {
        // Skip if trade lines are disabled
        if (!showTradeLines) return;
        
        // Only show lines for significant trades
        if (tradeAmount < minimumTradeVolumeToShow) return;
        
        // Scale line width based on trade volume
        float lineWidth = Mathf.Clamp(tradeAmount / 20f, 0.1f, 2.0f);
        
        // Get region positions from GameObjects (or use stored positions)
        GameObject fromObj = GameObject.Find(from.regionName);
        GameObject toObj = GameObject.Find(to.regionName);
        
        if (fromObj == null || toObj == null) return;
        
        Vector3 fromPos = fromObj.transform.position;
        Vector3 toPos = toObj.transform.position;
        
        // Create line between the regions
        GameObject lineObj = new GameObject($"TradeLine_{from.regionName}_{to.regionName}");
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        
        // Set line properties
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        line.positionCount = 2;
        line.SetPosition(0, fromPos);
        line.SetPosition(1, toPos);
        
        // Elevate slightly above map
        line.sortingOrder = 10;
        
        // Add to active lines list
        activeTradeLines.Add(lineObj);
        
        // Destroy after duration
        Object.Destroy(lineObj, tradeLineDuration);
    }
    
    public void ClearTradeLines()
    {
        foreach (var line in activeTradeLines)
        {
            if (line != null)
                Object.Destroy(line);
        }
        activeTradeLines.Clear();
    }
}
