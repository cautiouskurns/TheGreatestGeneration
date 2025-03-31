using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewInfrastructureType", menuName = "Game/Infrastructure Type")]
public class InfrastructureTypeDataSO : ScriptableObject
{
    public string typeName;
    public Sprite icon;
    [TextArea(2, 4)] public string description;
    
    // Economic impacts per level
    [System.Serializable]
    public class LevelData
    {
        public int level;
        public string levelName;
        [TextArea(1, 2)] public string levelDescription;
        public Dictionary<string, float> sectorBonuses = new Dictionary<string, float>();
    }
    
    public LevelData[] levels;
    
    // Requirements for development
    public int baseCost;
    public float costMultiplierPerLevel = 1.5f;
    public int baseTurnsToComplete;
}