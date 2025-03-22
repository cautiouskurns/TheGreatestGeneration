// RegionMapDataSO.cs
using UnityEngine;

[CreateAssetMenu(fileName = "RegionMap", menuName = "Game/Region Map")]
public class RegionMapDataSO : ScriptableObject
{
    [System.Serializable]
    public class RegionData
    {
        public string regionName;
        public Color regionColor;
        public Vector2 position;
    }

    public RegionData[] regions;
}