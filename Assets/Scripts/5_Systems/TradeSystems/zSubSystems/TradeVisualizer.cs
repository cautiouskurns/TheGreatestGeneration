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
    
    // Parameters for curved lines
    private readonly float curveHeight = 0.3f;
    private readonly int curveSegments = 12;
    
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
        
        // Create a unique key for this trade line to check for duplicates
        string lineKey = $"{from.regionName}_{to.regionName}";
        string reverseLineKey = $"{to.regionName}_{from.regionName}";
        
        // Calculate curve direction based on whether it's an import or export and if a reverse line exists
        bool isReverseTradeExists = activeTradeLines.Exists(line => line.name == reverseLineKey);
        
        // Create line between the regions
        GameObject lineObj = new GameObject(lineKey);
        LineRenderer line = lineObj.AddComponent<LineRenderer>();
        
        // Set line properties
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = color;
        line.endColor = color;
        
        // Calculate the midpoint between regions
        Vector3 midPoint = (fromPos + toPos) / 2f;
        
        // Calculate a perpendicular direction for the curve
        Vector3 direction = (toPos - fromPos).normalized;
        Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0);
        
        // Adjust curve direction based on the line type and existing lines
        float curveDirection = 1.0f;
        if (color == importColor) curveDirection = -1.0f;
        if (isReverseTradeExists) curveDirection *= 1.5f; // Exaggerate curve when bidirectional trade exists
        
        // Calculate curve height based on distance between regions
        float distance = Vector3.Distance(fromPos, toPos);
        float actualCurveHeight = curveHeight * distance * curveDirection;
        
        // Calculate control point for Bezier curve
        Vector3 controlPoint = midPoint + perpendicular * actualCurveHeight;
        
        // Create a Bezier curve
        line.positionCount = curveSegments;
        for (int i = 0; i < curveSegments; i++)
        {
            float t = i / (float)(curveSegments - 1);
            Vector3 point = CalculateQuadraticBezierPoint(t, fromPos, controlPoint, toPos);
            line.SetPosition(i, point);
        }
        
        // Add arrow to show direction
        AddDirectionIndicator(fromPos, toPos, controlPoint, color, lineWidth);
        
        // Elevate slightly above map
        line.sortingOrder = 10;
        
        // Add to active lines list
        activeTradeLines.Add(lineObj);
        
        // Destroy after duration
        Object.Destroy(lineObj, tradeLineDuration);
    }
    
    // Calculate point on a quadratic Bezier curve
    private Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        // Quadratic Bezier formula: (1-t)²p0 + 2(1-t)tp1 + t²p2
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        
        Vector3 point = uu * p0;
        point += 2 * u * t * p1;
        point += tt * p2;
        
        return point;
    }
    
    // Add a small arrow indicator to show direction of trade
    private void AddDirectionIndicator(Vector3 from, Vector3 to, Vector3 controlPoint, Color color, float width)
    {
        // Create arrow at approximately 70% along the curve
        float t = 0.7f;
        Vector3 position = CalculateQuadraticBezierPoint(t, from, controlPoint, to);
        
        // Calculate the direction at this point on the curve
        float deltaT = 0.01f;
        Vector3 nextPosition = CalculateQuadraticBezierPoint(t + deltaT, from, controlPoint, to);
        Vector3 direction = (nextPosition - position).normalized;
        
        // Create arrow object
        GameObject arrowObj = new GameObject("DirectionIndicator");
        arrowObj.transform.position = position;
        
        // Create a simple triangle for the arrow
        MeshFilter meshFilter = arrowObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = arrowObj.AddComponent<MeshRenderer>();
        
        float arrowSize = width * 2.0f;
        
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            Vector3.zero,
            new Vector3(-arrowSize/2, -arrowSize, 0),
            new Vector3(arrowSize/2, -arrowSize, 0)
        };
        mesh.triangles = new int[] { 0, 1, 2 };
        meshFilter.mesh = mesh;
        
        // Set material and color
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"));
        meshRenderer.material.color = color;
        
        // Orient arrow in direction of trade
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        arrowObj.transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 to align with direction
        
        // Make sure arrow is above map
        meshRenderer.sortingOrder = 11;
        
        // Add to active lines so it gets cleaned up
        activeTradeLines.Add(arrowObj);
        
        // Destroy with the line
        Object.Destroy(arrowObj, tradeLineDuration);
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