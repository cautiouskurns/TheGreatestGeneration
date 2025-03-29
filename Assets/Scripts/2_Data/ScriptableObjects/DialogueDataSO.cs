using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue")]
public class DialogueDataSO : ScriptableObject
{
    [Header("Dialogue Metadata")]
    public string dialogueId;
    public string title;
    public DialogueCategory category;
    
    [Header("Speakers")]
    public List<SpeakerData> speakers = new List<SpeakerData>();
    
    [Header("Dialogue Content")]
    public List<DialogueNodeData> dialogueNodes = new List<DialogueNodeData>();
    
    [Header("Choices")]
    public List<EventChoiceData> choices = new List<EventChoiceData>();

    public enum DialogueCategory
    {
        Advisor,
        Diplomat,
        Citizen,
        Cabinet,
        Tutorial
    }
}

[System.Serializable]
public class SpeakerData
{
    public string speakerId;
    public string speakerName;
    public Sprite portrait;
    [TextArea(1, 3)]
    public string description;
    public Color textColor = Color.white;
}
