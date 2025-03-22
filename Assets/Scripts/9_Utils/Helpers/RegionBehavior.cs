// RegionBehavior.cs
using UnityEngine;

public class RegionBehavior : MonoBehaviour
{
    public string regionName;
    public MapManager mapManager;

    void OnMouseDown()
    {
        mapManager.SelectRegion(regionName);
    }
}