using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace V1.Utils
{ 
    /// CLASS PURPOSE:
    /// LineGraph is a reusable UI component that visualizes a sequence of numeric data
    /// as a line graph with point markers. It dynamically scales to display a wide
    /// range of values and can be customized through public settings.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Convert a list of numeric values into normalized points on a 2D graph
    /// - Draw lines connecting each data point using LineRenderer
    /// - Instantiate point markers with adjustable size and color
    /// - Display minimum and maximum values as labeled UI text
    ///
    /// KEY COLLABORATORS:
    /// - Unity UI (RectTransform, TextMeshProUGUI, Image): Renders graph container and labels
    /// - LineRenderer: Renders the connecting line between data points
    /// - PointPrefab: Optional visual marker for each data point
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Supports generic numeric types through System.IConvertible constraint
    /// - Automatically recalculates min/max with a 10% padding
    /// - Cleans up previously rendered points before drawing new ones
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Separate data normalization from rendering logic
    /// - Provide line smoothing or interpolation options
    /// - Support dynamic updates or animations for live data
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add axis labels or gridlines for reference
    /// - Enable multi-series support with different colors/styles
    /// - Support tooltips or interactions for data points
    public class LineGraph : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI titleText;
        public RectTransform graphContainer;
        public LineRenderer lineRenderer;
        public GameObject pointPrefab;
        public TextMeshProUGUI minValueText;
        public TextMeshProUGUI maxValueText;
        
        [Header("Graph Settings")]
        public Color lineColor = Color.white;
        public Color pointColor = Color.white;
        public float lineWidth = 2f;
        public float pointSize = 5f;
        
        private List<GameObject> points = new List<GameObject>();
        
        public void SetTitle(string title)
        {
            if (titleText != null)
                titleText.text = title;
        }
        
        public void UpdateData<T>(List<T> dataPoints) where T : System.IConvertible
        {
            if (dataPoints == null || dataPoints.Count < 2 || graphContainer == null) 
                return;
            
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
            
            // Clear existing points
            foreach (var point in points)
            {
                if (point != null)
                    Destroy(point);
            }
            points.Clear();
            
            // Setup line renderer
            if (lineRenderer != null)
            {
                lineRenderer.startColor = lineColor;
                lineRenderer.endColor = lineColor;
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.positionCount = values.Count;
                
                for (int i = 0; i < values.Count; i++)
                {
                    // Calculate normalized position
                    float xPos = (float)i / (values.Count - 1);
                    float yPos = Mathf.InverseLerp(minValue, maxValue, values[i]);
                    
                    // Convert to local position in the container
                    float width = graphContainer.rect.width;
                    float height = graphContainer.rect.height;
                    
                    Vector3 pointPos = new Vector3(xPos * width, yPos * height, 0);
                    lineRenderer.SetPosition(i, pointPos);
                    
                    // Create point marker if prefab exists
                    if (pointPrefab != null)
                    {
                        GameObject point = Instantiate(pointPrefab, graphContainer);
                        point.transform.localPosition = pointPos;
                        
                        // Set point color
                        Image pointImage = point.GetComponent<Image>();
                        if (pointImage != null)
                        {
                            pointImage.color = pointColor;
                        }
                        
                        // Set point size
                        RectTransform rt = point.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.sizeDelta = new Vector2(pointSize, pointSize);
                        }
                        
                        points.Add(point);
                    }
                }
            }
        }
    }
}