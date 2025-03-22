using UnityEngine;

[CreateAssetMenu(fileName = "NewNation", menuName = "Game/Nation")]
public class NationDataSO : ScriptableObject
{
    public string nationName;
    public Color nationColor;
    public Sprite nationFlag;
    [TextArea(3, 5)] public string description;
    
    // Starting resources
    public int initialWealth;
    public int initialFood;
    public int initialMaterials;
    
    // National traits
    [System.Serializable]
    public class NationalTrait
    {
        public string traitName;
        [TextArea(2, 3)] public string description;
        public string affectedSector;
        public float sectorModifier;
    }
    
    public NationalTrait[] nationalTraits;
    
    // Starting regions (references to RegionDataSO)
    public RegionDataSO[] startingRegions;
}
