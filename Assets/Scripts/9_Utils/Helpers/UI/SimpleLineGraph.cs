/// CLASS PURPOSE:
/// SimpleLineGraph provides a lightweight UI-based visualization of numerical data as a line graph.
/// It uses UI Images to render points and connecting lines inside a Unity Canvas container.
///
/// CORE RESPONSIBILITIES:
/// - Convert a list of numerical values into scaled 2D positions
/// - Display points and connecting lines using UI Image elements
/// - Update min and max value labels, and optionally debug raw data
///
/// KEY COLLABORATORS:
/// - Unity UI (RectTransform, TextMeshProUGUI, Image): Renders graph and labels
/// - PointPrefab (optional): Custom visual prefab for each data point
///
/// CURRENT ARCHITECTURE NOTES:
/// - Generic UpdateData method supports any IConvertible numeric type
/// - Clears and re-generates all visuals each time new data is applied
/// - Uses direct anchoredPosition and size manipulation for layout
///
/// REFACTORING SUGGESTIONS:
/// - Cache and reuse points/lines instead of destroying and recreating
/// - Separate data normalization and UI rendering logic into helper classes
///
/// EXTENSION OPPORTUNITIES:
/// - Add axis lines, grid markers, or tooltip interactivity
/// - Support animations or transitions between datasets
/// - Enable color gradients or styles based on data characteristics

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// A simplified line graph that uses UI Images to create the visualization
/// </summary>
public class SimpleLineGraph : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public RectTransform graphContainer;
    public GameObject pointPrefab; // This should be a small UI Image
    public TextMeshProUGUI minValueText;
    public TextMeshProUGUI maxValueText;
    public TextMeshProUGUI dataDebugText; // ADDED: for debugging data
    
    [Header("Graph Settings")]
    public Color lineColor = new Color(0, 0.8f, 0.8f, 1f);
    public float pointSize = 10f;
    
    private List<GameObject> points = new List<GameObject>();
    private List<GameObject> lines = new List<GameObject>();
    
    public void SetTitle(string title)
    {
        if (titleText != null)
            titleText.text = title;
    }
    
    public void UpdateData<T>(List<T> dataPoints) where T : System.IConvertible
    {
        // Debug information - display data as text
        if (dataDebugText != null)
        {
            string dataStr = "Data: ";
            foreach (var point in dataPoints)
            {
                dataStr += System.Convert.ToSingle(point).ToString("F1") + ", ";
            }
            dataDebugText.text = dataStr;
            Debug.Log($"Graph {titleText.text}: {dataStr}");
        }
        
        if (dataPoints == null || dataPoints.Count < 2 || graphContainer == null) 
        {
            Debug.LogWarning($"SimpleLineGraph: Cannot update graph {(titleText != null ? titleText.text : "unknown")} - " +
                           $"dataPoints={dataPoints != null}, count={(dataPoints != null ? dataPoints.Count : 0)}, container={graphContainer != null}");
            return;
        }
        
        // Convert to floats and find min/max
        List<float> values = new List<float>();
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;
        
        foreach (var point in dataPoints)
        {
            float value = System.Convert.ToSingle(point);
            values.Add(value);
            
            minValue = Mathf.Min(minValue, value);
            maxValue = Mathf.Max(maxValue, value);
        }
        
        // Add padding to min/max
        float padding = (maxValue - minValue) * 0.1f;
        if (padding < 1f) padding = 1f; // Minimum padding
        
        minValue -= padding;
        maxValue += padding;
        
        // Update value texts
        if (minValueText != null)
            minValueText.text = minValue.ToString("F0");
            
        if (maxValueText != null)
            maxValueText.text = maxValue.ToString("F0");
        
        // Clear existing points and lines
        foreach (var point in points)
        {
            if (point != null)
                Destroy(point);
        }
        points.Clear();
        
        foreach (var line in lines)
        {
            if (line != null)
                Destroy(line);
        }
        lines.Clear();
        
        // Create new points and lines
        Vector2 previousPointPosition = Vector2.zero;
        bool firstPoint = true;
        
        float width = graphContainer.rect.width;
        float height = graphContainer.rect.height;
        
        for (int i = 0; i < values.Count; i++)
        {
            // Calculate normalized position
            float xPos = (float)i / (values.Count - 1);
            float yPos = Mathf.InverseLerp(minValue, maxValue, values[i]);
            
            // Convert to local position in the container
            Vector2 pointPos = new Vector2(xPos * width, yPos * height);
            
            // Create point
            GameObject point = CreatePoint(pointPos);
            points.Add(point);
            
            // Create connecting line (except for first point)
            if (!firstPoint)
            {
                GameObject line = CreateLine(previousPointPosition, pointPos);
                lines.Add(line);
            }
            else
            {
                firstPoint = false;
            }
            
            previousPointPosition = pointPos;
        }
    }
    
    private GameObject CreatePoint(Vector2 position)
    {
        GameObject point = null;
        
        if (pointPrefab != null)
        {
            // Instantiate from prefab
            point = Instantiate(pointPrefab, graphContainer);
        }
        else
        {
            // Create a basic point with an Image component
            point = new GameObject("Point", typeof(Image));
            point.transform.SetParent(graphContainer, false);
            
            Image image = point.GetComponent<Image>();
            image.color = lineColor;
        }
        
        // Set position and size
        RectTransform rt = point.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = new Vector2(pointSize, pointSize);
        
        return point;
    }
    
    private GameObject CreateLine(Vector2 from, Vector2 to)
    {
        // Create a line using an Image with a thin rectangle
        GameObject line = new GameObject("Line", typeof(Image));
        line.transform.SetParent(graphContainer, false);
        
        Image image = line.GetComponent<Image>();
        image.color = lineColor;
        
        // Calculate position and rotation for the line
        RectTransform rt = line.GetComponent<RectTransform>();
        
        // Set anchors and pivot
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(0, 0);
        rt.pivot = new Vector2(0, 0.5f); // Pivot at the start of the line
        
        // Position at the starting point
        rt.anchoredPosition = from;
        
        // Calculate the dimensions and angle of the line
        float width = Vector2.Distance(from, to);
        float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
        
        // Apply calculated values
        rt.sizeDelta = new Vector2(width, 2f); // Line thickness of 2 pixels
        rt.localRotation = Quaternion.Euler(0, 0, angle);
        
        return line;
    }
}