using UnityEngine;

[CreateAssetMenu(fileName = "NewRegionType", menuName = "Game/Region Type")]
public class RegionTypeDataSO : ScriptableObject
{
    public string regionTypeName;
    public Color mapColor;
    [Range(0, 10)] public int agriculturePotential;
    [Range(0, 10)] public int industryPotential;
    [Range(0, 10)] public int commercePotential;
    [Range(0, 10)] public int miningPotential;
    [Range(0, 10)] public int researchPotential;
    public Sprite regionIcon;
    [TextArea(3, 5)] public string description;
}