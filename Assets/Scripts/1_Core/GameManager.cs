// GameManager.cs - Updated version
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MapView mapView;
    
    [Header("Map Generation")]
    public bool useProceduralMap = true;
    public MapDataSO predefinedMapData;
    public int mapWidth = 10;
    public int mapHeight = 8;
    public int nationCount = 3;
    public int regionsPerNation = 5;
    
    private MapModel mapModel;

    private void Awake()
    {
        // Generate or use predefined map
        MapDataSO mapData;
        if (useProceduralMap)
        {
            MapGenerator generator = new MapGenerator(mapWidth, mapHeight, nationCount, regionsPerNation);
            mapData = generator.GenerateMap();
        }
        else
        {
            mapData = predefinedMapData;
        }
        
        // Assign generated map data to MapView
        mapView.mapData = mapData;
        
        // Initialize model with the map data
        mapModel = new MapModel(mapData);
        
        Debug.Log("GameManager initialized with " + 
                  (useProceduralMap ? "procedurally generated" : "predefined") + 
                  " map");
    }

    private void OnEnable()
    {
        EventBus.Subscribe("TurnEnded", OnTurnEnded);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
    }

    private void OnTurnEnded(object _)
    {
        mapModel.ProcessTurn();
        Debug.Log("Turn ended, processing complete");
    }
    
    public void SelectRegion(string regionName)
    {
        mapModel.SelectRegion(regionName);
    }
    
    public RegionEntity GetRegion(string regionName)
    {
        return mapModel.GetRegion(regionName);
    }
}