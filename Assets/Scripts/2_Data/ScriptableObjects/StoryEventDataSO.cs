using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStoryEvent", menuName = "Game/Story Event")]
public class StoryEventDataSO : ScriptableObject
{
    [Header("Event Metadata")]
    public string eventId;
    public string title;
    [TextArea(3, 5)]
    public string description;
    public EventCategory category;
    public int cooldownTurns = 10;
    
    [Header("Trigger Conditions")]
    public List<EventCondition> conditions = new List<EventCondition>();
    
    [Header("Content")]
    public List<DialogueNodeData> dialogueNodes = new List<DialogueNodeData>();
    public List<EventChoiceData> choices = new List<EventChoiceData>();
    
    [Header("Visuals")]
    public Sprite backgroundImage;
    public AudioClip musicTheme;

    public enum EventCategory
    {
        Economic,
        Diplomatic,
        Regional,
        Cabinet,
        Random
    }
}

[System.Serializable]
public class EventCondition
{
    public ConditionType conditionType;
    public string targetValue;
    [Tooltip("For numeric comparisons, defines how to compare values")]
    public ComparisonOperator comparisonOperator;
    [Tooltip("For numeric comparisons, the value to compare against")]
    public float numericValue;

    public enum ConditionType
    {
        EconomicCyclePhase,
        ResourceShortage,
        ResourceSurplus,
        NationRelation,
        RegionSatisfaction,
        TurnNumber,
        GenerationNumber,
        ResourceAmount,
        InfrastructureLevel,
        PreviousEventOccurred
    }

    public enum ComparisonOperator
    {
        Equals,
        NotEquals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }
}

[System.Serializable]
public class DialogueNodeData
{
    public string nodeId;
    [TextArea(2, 5)]
    public string text;
    public string speakerId;
    public List<string> nextNodeIds = new List<string>();
    public List<DialogueVariantData> variants = new List<DialogueVariantData>();
}

[System.Serializable]
public class DialogueVariantData
{
    [TextArea(2, 5)]
    public string text;
    public List<EventCondition> conditions = new List<EventCondition>();
}

[System.Serializable]
public class EventChoiceData
{
    [TextArea(1, 3)]
    public string text;
    public List<EventOutcomeData> outcomes = new List<EventOutcomeData>();
    public List<DialogueVariantData> variants = new List<DialogueVariantData>();
    [Tooltip("If true, this choice is only shown when specific conditions are met")]
    public bool hasConditions = false;
    public List<EventCondition> conditions = new List<EventCondition>();
}

[System.Serializable]
public class EventOutcomeData
{
    public OutcomeType outcomeType;
    public string targetId;
    public float value;
    [TextArea(1, 2)]
    public string description;

    public enum OutcomeType
    {
        AddResource,
        RemoveResource,
        ChangeNationRelation,
        ModifyRegionSatisfaction,
        StartProject,
        UnlockTechnology,
        AddDecision,
        ModifyInfrastructure
    }
}
