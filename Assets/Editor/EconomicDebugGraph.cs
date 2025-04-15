using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace V2.Editor
{
    /// <summary>
    /// Utility class for drawing economic debug graphs in the editor
    /// </summary>
    public class EconomicDebugGraph
    {
        // Graph colors
        public Color wealthColor = new Color(0.2f, 0.8f, 0.2f);
        public Color productionColor = new Color(0.8f, 0.4f, 0.1f);
        public Color satisfactionColor = new Color(0.2f, 0.4f, 0.8f);
        public Color supplyColor = new Color(0.2f, 0.8f, 0.8f); // Cyan
        public Color demandColor = new Color(0.9f, 0.3f, 0.7f); // Magenta
        public Color imbalanceColor = new Color(0.9f, 0.9f, 0.2f); // Yellow
        public Color parameterColor = new Color(1f, 0.5f, 0.8f);
        
        // Graph visibility settings
        public bool showWealthGraph = true;
        public bool showProductionGraph = true;
        public bool showSatisfactionGraph = true;
        public bool showSupplyGraph = true;
        public bool showDemandGraph = true;
        public bool showImbalanceGraph = true;
        public bool showParameterGraph = true;
        
        // Draw a line graph for any type of data
        public void DrawLineGraph<T>(List<T> data, float maxValue, Color color, Rect graphRect)
        {
            if (data.Count < 2) return;
            
            Handles.color = color;
            
            // Calculate step size - ensure entire history fits in the graph width
            float xStep = graphRect.width / (data.Count - 1 > 0 ? data.Count - 1 : 1);
            
            for (int i = 0; i < data.Count - 1; i++)
            {
                float value1 = System.Convert.ToSingle(data[i]) / maxValue;
                float value2 = System.Convert.ToSingle(data[i + 1]) / maxValue;
                
                // Clamp values to [0,1] range
                value1 = Mathf.Clamp01(value1);
                value2 = Mathf.Clamp01(value2);
                
                // Calculate points
                Vector3 p1 = new Vector3(
                    graphRect.x + i * xStep,
                    graphRect.y + graphRect.height - (value1 * graphRect.height),
                    0
                );
                
                Vector3 p2 = new Vector3(
                    graphRect.x + (i + 1) * xStep,
                    graphRect.y + graphRect.height - (value2 * graphRect.height),
                    0
                );
                
                Handles.DrawLine(p1, p2);
            }
        }
        
        // Draw graph axes with tick marks and labels
        public void DrawAxes(Rect graphRect, string xLabel, string yLabel, float maxYValue, int dataLength)
        {
            // Draw axes
            Handles.color = Color.gray;
            
            // X-axis
            Handles.DrawLine(
                new Vector3(graphRect.x, graphRect.y + graphRect.height, 0),
                new Vector3(graphRect.x + graphRect.width, graphRect.y + graphRect.height, 0)
            );
            
            // Y-axis
            Handles.DrawLine(
                new Vector3(graphRect.x, graphRect.y, 0),
                new Vector3(graphRect.x, graphRect.y + graphRect.height, 0)
            );
            
            // X-axis label - position it centered below x-axis
            GUI.contentColor = Color.white;
            float xLabelWidth = 60;
            float xLabelX = graphRect.x + (graphRect.width / 2) - (xLabelWidth / 2); // Center it
            GUI.Label(new Rect(xLabelX, graphRect.y + graphRect.height + 25, xLabelWidth, 20), xLabel);
            
            // Y-axis label without rotation - position it to the left of Y axis
            float yLabelWidth = 80;
            GUI.Label(new Rect(graphRect.x - yLabelWidth - 5, graphRect.y + (graphRect.height / 2) - 10, yLabelWidth, 20), yLabel);
            
            // Y-axis tick marks and values
            int tickCount = 5;
            for (int i = 0; i <= tickCount; i++)
            {
                float yPos = graphRect.y + graphRect.height - (i * (graphRect.height / tickCount));
                float value = (float)i * maxYValue / tickCount;
                
                // Draw tick
                Handles.DrawLine(
                    new Vector3(graphRect.x - 2, yPos, 0),
                    new Vector3(graphRect.x + 2, yPos, 0)
                );
                
                // Draw value - further left with right alignment
                var valueStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleRight };
                GUI.Label(new Rect(graphRect.x - 75, yPos - 10, 70, 20), value.ToString("F1"), valueStyle);
            }
            
            // Only draw x-axis labels if we have data
            if (dataLength > 0)
            {
                // Determine optimal tick spacing based on data length
                int majorTickInterval;
                if (dataLength <= 10) 
                    majorTickInterval = 1;
                else if (dataLength <= 20) 
                    majorTickInterval = 2;
                else if (dataLength <= 50) 
                    majorTickInterval = 5;
                else 
                    majorTickInterval = 10;
                
                // Draw minor ticks for each data point
                for (int i = 0; i < dataLength; i++)
                {
                    // Calculate position (the full history is shown, index 0 at the left)
                    float xPos = graphRect.x + ((float)i / (dataLength - 1 > 0 ? dataLength - 1 : 1)) * graphRect.width;
                        
                    // Draw minor tick
                    Handles.DrawLine(
                        new Vector3(xPos, graphRect.y + graphRect.height, 0),
                        new Vector3(xPos, graphRect.y + graphRect.height + 2, 0)
                    );
                }
                
                // Draw major ticks and labels
                for (int i = 0; i < dataLength; i += majorTickInterval)
                {
                    // Calculate position
                    float xPos = graphRect.x + ((float)i / (dataLength - 1 > 0 ? dataLength - 1 : 1)) * graphRect.width;
                    
                    // Also show the last tick explicitly if it's not already included
                    if (i > 0 && i + majorTickInterval >= dataLength && i != dataLength - 1)
                    {
                        int lastIndex = dataLength - 1;
                        float lastPos = graphRect.x + ((float)lastIndex / (dataLength - 1)) * graphRect.width;
                        
                        // Draw major tick
                        Handles.DrawLine(
                            new Vector3(lastPos, graphRect.y + graphRect.height, 0),
                            new Vector3(lastPos, graphRect.y + graphRect.height + 5, 0)
                        );
                        
                        // Draw tick label - centered below tick
                        var valueStyle = new GUIStyle(GUI.skin.label) { 
                            alignment = TextAnchor.UpperCenter,
                            fontSize = 10
                        };
                        // Show as a tick number
                        GUI.Label(new Rect(lastPos - 15, graphRect.y + graphRect.height + 6, 30, 20), 
                            $"{lastIndex+1}", valueStyle);
                    }
                    
                    // Draw major tick
                    Handles.DrawLine(
                        new Vector3(xPos, graphRect.y + graphRect.height, 0),
                        new Vector3(xPos, graphRect.y + graphRect.height + 5, 0)
                    );
                    
                    // Draw tick label - centered below tick
                    var style = new GUIStyle(GUI.skin.label) { 
                        alignment = TextAnchor.UpperCenter,
                        fontSize = 10
                    };
                    
                    // Show as a tick number
                    GUI.Label(new Rect(xPos - 15, graphRect.y + graphRect.height + 6, 30, 20), 
                        $"{i+1}", style);
                }
            }
        }
        
        // Draw a colored background box
        public Texture2D MakeTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            
            Texture2D texture = new Texture2D(width, height);
            texture.SetPixels(pixels);
            texture.Apply();
            return texture;
        }
        
        // Find the maximum value in a float list
        public float MaxList(List<float> list)
        {
            if (list == null || list.Count == 0)
                return 0f;
                
            float max = list[0];
            foreach (var val in list)
            {
                if (val > max) max = val;
            }
            return max;
        }
    }
}