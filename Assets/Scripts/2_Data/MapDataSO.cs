using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "Game Data/Map Data")]
public class MapDataSO : ScriptableObject
{
    public NationDataSO[] nations;
}

[System.Serializable]
public class NationDataSO
{
    public string nationName;
    public Color nationColor;
    public RegionDataSO[] regions;
}

[System.Serializable]
public class RegionDataSO
{
    public string regionName;
    public int initialWealth;
    public int initialProduction;
}

