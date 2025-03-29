using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class EventDialogueManager : MonoBehaviour
{
    public static EventDialogueManager Instance { get; private set; }
    
    public GameObject eventDialoguePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI dialogueText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;
    
    private GameStateManager stateManager;
    private StoryEvent currentEvent;
    private string currentDialogueNodeId;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public static void ShowEventDialogue(StoryEvent storyEvent, GameStateManager state)
    {
        Instance.stateManager = state;
        Instance.currentEvent = storyEvent;
        
        // Setup panel
        Instance.eventDialoguePanel.SetActive(true);
        Instance.titleText.text = storyEvent.Title;
        Instance.descriptionText.text = storyEvent.Description;
        
        // Start dialogue if available
        if (storyEvent.DialogueNodes.Count > 0)
        {
            Instance.ShowDialogueNode(storyEvent.DialogueNodes[0].Id);
        }
        else
        {
            // If no dialogue, just show choices
            Instance.ShowEventChoices();
        }
    }
    
    private void ShowDialogueNode(string nodeId)
    {
        currentDialogueNodeId = nodeId;
        
        // Find the node
        var node = currentEvent.DialogueNodes.Find(n => n.Id == nodeId);
        if (node == null) return;
        
        // Process text with state variables
        string processedText = node.GetProcessedText(stateManager);
        dialogueText.text = processedText;
        
        // If this is an end node (no next nodes), show choices
        if (node.NextNodeIds.Count == 0)
        {
            ShowEventChoices();
        }
    }
    
    private void ShowEventChoices()
    {
        // Clear existing choices
        foreach (Transform child in choicesContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Add new choices
        foreach (var choice in currentEvent.Choices)
        {
            var choiceGO = Instantiate(choiceButtonPrefab, choicesContainer);
            var choiceText = choiceGO.GetComponentInChildren<TextMeshProUGUI>();
            
            // Process choice text with state variables
            string processedText = choice.GetProcessedText(stateManager);
            choiceText.text = processedText;
            
            // Add click handler
            var button = choiceGO.GetComponent<Button>();
            EventChoice capturedChoice = choice; // Capture for lambda
            button.onClick.AddListener(() => OnChoiceSelected(capturedChoice));
        }
    }
    
    private void OnChoiceSelected(EventChoice choice)
    {
        // Process all outcomes of this choice
        foreach (var outcome in choice.Outcomes)
        {
            ApplyOutcome(outcome);
        }
        
        // Close dialogue
        eventDialoguePanel.SetActive(false);
        
        // Record this decision in player history
        stateManager.History.SignificantDecisions.Add($"{currentEvent.Id}:{choice.Text}");
    }
    
    private void ApplyOutcome(EventOutcome outcome)
    {
        switch (outcome.Type)
        {
            case OutcomeType.AddResource:
                // Find region or nation and add resource
                break;
                
            case OutcomeType.ChangeNationRelation:
                if (stateManager.Diplomacy.NationRelations.ContainsKey(outcome.TargetId))
                    stateManager.Diplomacy.NationRelations[outcome.TargetId] += outcome.Value;
                else
                    stateManager.Diplomacy.NationRelations[outcome.TargetId] = outcome.Value;
                break;
                
            case OutcomeType.ModifyRegionSatisfaction:
                if (stateManager.RegionStates.ContainsKey(outcome.TargetId))
                    stateManager.RegionStates[outcome.TargetId].Satisfaction += outcome.Value;
                break;
                
            // More outcome types...
        }
    }
}
