using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MapDataSO mapData;
    
    private MapModel mapModel;

    private void Awake()
    {
        mapModel = new MapModel(mapData);
        Debug.Log("GameManager initialized");
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