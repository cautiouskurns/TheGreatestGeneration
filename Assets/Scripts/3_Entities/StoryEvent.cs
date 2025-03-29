using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StoryEvent
{
    public string Id;
    public string Title;
    public string Description;
    public List<EventCondition> Conditions = new List<EventCondition>();
    public int CooldownTurns = 10;
    public List<DialogueNode> DialogueNodes = new List<DialogueNode>();
    public List<EventChoice> Choices = new List<EventChoice>();
}
