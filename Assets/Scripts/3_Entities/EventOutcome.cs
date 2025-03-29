using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EventOutcome
{
    public OutcomeType Type;
    public string TargetId; // Resource name, nation name, etc.
    public float Value;
    public string Description;
}

public enum OutcomeType
{
    AddResource,
    RemoveResource,
    ChangeNationRelation,
    ModifyRegionSatisfaction,
    UnlockTechnology,
    StartProject,
    AddPlayerDecision,
    // etc.
}
