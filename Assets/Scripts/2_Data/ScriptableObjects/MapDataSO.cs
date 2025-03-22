using UnityEngine;

[CreateAssetMenu(fileName = "NewMapData", menuName = "Game Data/Map Data")]
public class MapDataSO : ScriptableObject
{
    [System.Serializable]
    public class NationData
    {
        public string nationName;
        public Color nationColor;
        public RegionData[] regions;
    }

    [System.Serializable]
    public class RegionData
    {
        public string regionName;
        public int initialWealth;
        public int initialProduction;
    }

    public NationData[] nations;
}