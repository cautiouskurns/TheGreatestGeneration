using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EventDialogueUI : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject dialoguePanel;
    public Image backgroundImage;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    
    [Header("Speaker References")]
    public Image speakerPortrait;
    public TextMeshProUGUI speakerNameText;
    
    [Header("Dialogue References")]
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    
    [Header("Choice References")]
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;
    
    [Header("Animation Settings")]
    public float typingSpeed = 0.05f;
    public bool useTypewriterEffect = true;
    
    // Event data
    private StoryEventDataSO currentEvent;
    private DialogueNodeData currentNode;
    private string currentNodeId;
    private GameStateManager stateManager;
    private Coroutine typewriterCoroutine;
    
    private void Awake()
    {
        // Hide panel by default
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);
            
        // Add listener to continue button
        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinueClicked);
    }
    
    public void ShowEvent(StoryEventDataSO eventData, GameStateManager stateManager)
    {
        this.stateManager = stateManager;
        currentEvent = eventData;
        
        // Set up panel
        dialoguePanel.SetActive(true);
        titleText.text = eventData.title;
        descriptionText.text = eventData.description;
        
        // Set background if available
        if (eventData.backgroundImage != null)
            backgroundImage.sprite = eventData.backgroundImage;
            
        // If there's dialogue, show the first node
        if (eventData.dialogueNodes.Count > 0)
        {
            ShowDialogueNode(eventData.dialogueNodes[0]);
        }
        else
        {
            // If no dialogue, show choices directly
            ShowChoices();
        }
    }
    
    public void ShowDialogueNode(DialogueNodeData node)
    {
        currentNode = node;
        currentNodeId = node.nodeId;
        
        // Find the speaker
        SpeakerData speaker = null;
        if (!string.IsNullOrEmpty(node.speakerId))
        {
            // You would get this from a speaker repository or the current event
            // This is placeholder code
            speaker = GetSpeakerById(node.speakerId);
        }
        
        // Set speaker information if available
        if (speaker != null)
        {
            speakerNameText.text = speaker.speakerName;
            speakerNameText.color = speaker.textColor;
            
            if (speaker.portrait != null)
            {
                speakerPortrait.gameObject.SetActive(true);
                speakerPortrait.sprite = speaker.portrait;
            }
            else
            {
                speakerPortrait.gameObject.SetActive(false);
            }
        }
        else
        {
            speakerNameText.text = "";
            speakerPortrait.gameObject.SetActive(false);
        }
        
        // Process the text with state variables
        string processedText = ProcessText(node.text);
        
        // Use typewriter effect if enabled
        if (useTypewriterEffect)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
                
            typewriterCoroutine = StartCoroutine(TypewriterEffect(processedText));
        }
        else
        {
            dialogueText.text = processedText;
        }
        
        // Show continue button if there are more nodes, otherwise show choices
        if (node.nextNodeIds.Count > 0)
        {
            continueButton.gameObject.SetActive(true);
            choicesContainer.SetActive(false);
        }
        else
        {
            continueButton.gameObject.SetActive(false);
            ShowChoices();
        }
    }
    
    private SpeakerData GetSpeakerById(string speakerId)
    {
        // This is placeholder code - you'd implement your actual speaker lookup
        // Typically from a repository or the current dialogue data
        return null;
    }
    
    private string ProcessText(string text)
    {
        // This would be where you replace variables like {Economy.CurrentPhase}
        // with actual values from the GameStateManager
        
        // Placeholder implementation
        if (stateManager != null)
        {
            // Example pattern replacement (you'd expand this significantly)
            text = text.Replace("{Economy.CurrentPhase}", 
                stateManager.Economy.CurrentEconomicCyclePhase);
        }
        
        return text;
    }
    
    private void ShowChoices()
    {
        // Clear existing choices
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Show choice container
        choicesContainer.SetActive(true);
        
        // Create buttons for each choice
        foreach (var choice in currentEvent.choices)
        {
            // Skip choices that don't meet conditions
            if (choice.hasConditions && !AreConditionsMet(choice.conditions))
                continue;
                
            // Instantiate button
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            DialogueChoiceButton choiceButton = choiceObj.GetComponent<DialogueChoiceButton>();
            
            if (choiceButton != null)
            {
                // Process text with state variables
                string processedText = ProcessText(choice.text);
                
                // Set up button
                choiceButton.SetChoice(processedText, choice);
                choiceButton.OnChoiceSelected += HandleChoiceSelected;
            }
        }
    }
    
    private bool AreConditionsMet(List<EventCondition> conditions)
    {
        // This would evaluate conditions against the game state
        // Placeholder implementation always returns true
        return true;
    }
    
    private void HandleChoiceSelected(EventChoiceData choice)
    {
        // Apply outcomes
        ApplyOutcomes(choice.outcomes);
        
        // Close the dialogue panel
        ClosePanel();
    }
    
    private void ApplyOutcomes(List<EventOutcomeData> outcomes)
    {
        // This would implement the outcomes in your game systems
        // Placeholder implementation
        foreach (var outcome in outcomes)
        {
            Debug.Log($"Applying outcome: {outcome.description}");
            
            // You would add actual implementation here
        }
    }
    
    private void OnContinueClicked()
    {
        if (currentNode != null && currentNode.nextNodeIds.Count > 0)
        {
            // Find and show the next node
            string nextNodeId = currentNode.nextNodeIds[0];
            DialogueNodeData nextNode = FindNodeById(nextNodeId);
            
            if (nextNode != null)
            {
                ShowDialogueNode(nextNode);
            }
            else
            {
                Debug.LogError($"Could not find dialogue node with ID: {nextNodeId}");
                ShowChoices(); // Fallback to choices
            }
        }
        else
        {
            ShowChoices();
        }
    }
    
    private DialogueNodeData FindNodeById(string nodeId)
    {
        return currentEvent.dialogueNodes.Find(node => node.nodeId == nodeId);
    }
    
    private void ClosePanel()
    {
        dialoguePanel.SetActive(false);
        
        // You might want to trigger an event to inform other systems the dialogue has ended
        // For example:
        // EventBus.Trigger("DialogueCompleted", currentEvent.eventId);
    }
    
    private IEnumerator TypewriterEffect(string text)
    {
        dialogueText.text = "";
        
        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
        
        typewriterCoroutine = null;
    }
}