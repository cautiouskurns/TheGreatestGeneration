// MapGenerator.cs - Place in 9_Utils/Helpers folder
using UnityEngine;
using System.Collections.Generic;

public class MapGenerator
{
    private int width;
    private int height;
    private int regionCount;
    private int nationCount;
    private Color[] nationColors;

    public MapGenerator(int width, int height, int nationCount, int regionsPerNation)
    {
        this.width = width;
        this.height = height;
        this.nationCount = nationCount;
        this.regionCount = nationCount * regionsPerNation;
        
        // Generate distinct colors for nations
        GenerateNationColors();
    }

    private void GenerateNationColors()
    {
        nationColors = new Color[nationCount];
        
        // Use the HSV color model to generate evenly distributed hues
        for (int i = 0; i < nationCount; i++)
        {
            // Distribute hue evenly around the color wheel (0 to 1)
            float hue = (float)i / nationCount;
            
            // Use a consistent saturation and value for all nations
            float saturation = 0.7f;  // Fairly saturated but not too extreme
            float value = 0.9f;       // Bright but not blindingly so
            
            // Convert HSV to RGB
            Color color = Color.HSVToRGB(hue, saturation, value);
            
            nationColors[i] = color;
        }
    }

    public MapDataSO GenerateMap()
    {
        // Create a new MapDataSO instance
        MapDataSO mapData = ScriptableObject.CreateInstance<MapDataSO>();
        
        // Setup nations
        mapData.nations = new MapDataSO.NationData[nationCount];
        
        int regionsPerNation = regionCount / nationCount;
        
        // Generate nations
        for (int n = 0; n < nationCount; n++)
        {
            mapData.nations[n] = new MapDataSO.NationData
            {
                nationName = "Nation " + (n + 1),
                nationColor = nationColors[n],
                regions = new MapDataSO.RegionData[regionsPerNation]
            };
            
            // Generate regions for this nation
            for (int r = 0; r < regionsPerNation; r++)
            {
                mapData.nations[n].regions[r] = new MapDataSO.RegionData
                {
                    regionName = "Region " + (n * regionsPerNation + r + 1),
                    initialWealth = Random.Range(50, 200),
                    initialProduction = Random.Range(5, 20)
                };
            }
        }
        
        return mapData;
    }
}
