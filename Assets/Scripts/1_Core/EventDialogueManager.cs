using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class EventDialogueManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;
    public GameObject choicesContainer;
    public GameObject choiceButtonPrefab;   
    
    [Header("Text Settings")]
    public float typingSpeed = 0.05f;
    public bool useTypewriterEffect = true;
    
    // Static instance for easy access
    public static EventDialogueManager Instance { get; private set; }
    
    // Track current dialogue state
    private string[] currentDialogueLines;
    private int currentLineIndex;
    private Coroutine typewriterCoroutine;
    
    // New properties for GameStateManager integration
    private GameStateManager stateManager;
    private SimpleDialogueEvent currentEvent;
    
    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Set up continue button
        continueButton.onClick.AddListener(OnContinueClicked);
        
        // Hide panel on start
        dialoguePanel.SetActive(false);
        
        // Get GameStateManager
        stateManager = GameStateManager.Instance;
        if (stateManager == null)
        {
            Debug.LogWarning("GameStateManager not found. State-based features will not work.");
        }
    }
    
    // Original method to display dialogue with multiple lines (preserve existing functionality)
    public void ShowDialogue(string title, string[] lines)
    {
        // Store dialogue data
        currentDialogueLines = lines;
        currentLineIndex = 0;
        
        // Set the title
        titleText.text = title;
        
        // Show panel
        dialoguePanel.SetActive(true);
        
        // Hide choices until needed
        choicesContainer.SetActive(false);
        continueButton.gameObject.SetActive(true);
        
        // Display first line
        DisplayCurrentLine();
    }
    
    // Original method to show dialogue with choices (preserve existing functionality)
    public void ShowDialogueWithChoices(string title, string message, string[] choiceTexts)
    {
        // Set the title and message
        titleText.text = title;
        
        // Show panel
        dialoguePanel.SetActive(true);
        
        // Display message with typewriter effect if enabled
        if (useTypewriterEffect)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
                
            typewriterCoroutine = StartCoroutine(TypewriterEffect(message));
        }
        else
        {
            dialogueText.text = message;
        }
        
        // Hide continue button
        continueButton.gameObject.SetActive(false);
        
        // Display choices
        DisplayChoices(choiceTexts);
    }
    
    // NEW: Method to show a dialogue event with state integration
    public void ShowDialogueEvent(SimpleDialogueEvent dialogueEvent)
    {
        currentEvent = dialogueEvent;
        
        // Process lines with state variables
        if (dialogueEvent.lines.Count > 0)
        {
            List<string> processedLines = new List<string>();
            foreach (var line in dialogueEvent.lines)
            {
                processedLines.Add(ProcessTextWithStateVariables(line.text));
            }
            
            // Use existing ShowDialogue method with processed lines
            ShowDialogue(dialogueEvent.title, processedLines.ToArray());
        }
        else if (dialogueEvent.choices.Count > 0)
        {
            // If no lines but has choices, show choices directly
            ShowEventChoices();
        }
        else
        {
            // Simple message
            ShowSimpleDialogue(dialogueEvent.title, ProcessTextWithStateVariables(dialogueEvent.description));
        }
    }
    
    // Helper method to process text with state variables
    private string ProcessTextWithStateVariables(string text)
    {
        if (stateManager == null) return text;
        
        string processed = text;
        
        // Replace economic phase
        processed = processed.Replace("{EconomicPhase}", 
            stateManager.Economy.CurrentEconomicCyclePhase);
        
        // Replace turn count
        processed = processed.Replace("{CurrentTurn}", 
            stateManager.GetCurrentTurn().ToString());
        
        // Replace generation
        processed = processed.Replace("{Generation}", 
            stateManager.History.GenerationNumber.ToString());
        
        // Add more replacements as needed
        
        return processed;
    }
    
    // Helper method to display the current dialogue line
    private void DisplayCurrentLine()
    {
        // Check if we have dialogue to display
        if (currentDialogueLines == null || currentLineIndex >= currentDialogueLines.Length)
        {
            // End of dialogue
            if (currentEvent != null && currentEvent.choices.Count > 0)
            {
                // If we have an active event with choices, show them
                ShowEventChoices();
            }
            else
            {
                // Otherwise just hide the panel
                dialoguePanel.SetActive(false);
            }
            return;
        }
        
        // Get the current line
        string currentLine = currentDialogueLines[currentLineIndex];
        
        // Display with typewriter effect if enabled
        if (useTypewriterEffect)
        {
            if (typewriterCoroutine != null)
                StopCoroutine(typewriterCoroutine);
                
            typewriterCoroutine = StartCoroutine(TypewriterEffect(currentLine));
        }
        else
        {
            dialogueText.text = currentLine;
        }
    }
    
    // Create and display simple choice buttons (original method)
    private void DisplayChoices(string[] choiceTexts)
    {
        // Clear existing choices
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Show choices container
        choicesContainer.SetActive(true);
        
        // Create buttons for each choice
        for (int i = 0; i < choiceTexts.Length; i++)
        {
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            TextMeshProUGUI choiceTextComponent = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (choiceTextComponent != null)
                choiceTextComponent.text = choiceTexts[i];
                
            // Add click listener with the choice index
            Button choiceButton = choiceObj.GetComponent<Button>();
            int choiceIndex = i; // Capture for the lambda
            choiceButton.onClick.AddListener(() => OnChoiceSelected(choiceIndex));
        }
    }
    
    // NEW: Method to show choices for the current event
    private void ShowEventChoices()
    {
        // Clear existing choices
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Show choices container, hide continue button
        choicesContainer.SetActive(true);
        continueButton.gameObject.SetActive(false);
        
        // If no event, show a default button
        if (currentEvent == null || currentEvent.choices.Count == 0)
        {
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            TextMeshProUGUI choiceTextComponent = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (choiceTextComponent != null)
                choiceTextComponent.text = "Continue";
                
            Button choiceButton = choiceObj.GetComponent<Button>();
            choiceButton.onClick.AddListener(() => EndDialogue());
            
            return;
        }
        
        // Create buttons for each valid choice
        for (int i = 0; i < currentEvent.choices.Count; i++)
        {
            var choice = currentEvent.choices[i];
            
            // Skip choices that don't meet conditions
            if (choice.hasCondition && !CheckChoiceCondition(choice))
                continue;
                
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            TextMeshProUGUI choiceTextComponent = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (choiceTextComponent != null)
                choiceTextComponent.text = ProcessTextWithStateVariables(choice.text);
                
            // Add click listener with the choice index
            Button choiceButton = choiceObj.GetComponent<Button>();
            int choiceIndex = i; // Capture for the lambda
            choiceButton.onClick.AddListener(() => OnEventChoiceSelected(choiceIndex));
        }
        
        // If no valid choices were added, add a default continue button
        if (choicesContainer.transform.childCount == 0)
        {
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            TextMeshProUGUI choiceTextComponent = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (choiceTextComponent != null)
                choiceTextComponent.text = "Continue";
                
            Button choiceButton = choiceObj.GetComponent<Button>();
            choiceButton.onClick.AddListener(() => EndDialogue());
        }
    }
    
    // NEW: Check if a choice condition is met
    private bool CheckChoiceCondition(DialogueChoice choice)
    {
        if (!choice.hasCondition || stateManager == null)
            return true;
            
        // This is a simple implementation - expand as needed
        switch (choice.requiredState)
        {
            case "EconomicPhase":
                return stateManager.Economy.CurrentEconomicCyclePhase == choice.requiredValue.ToString();
                
            case "ResourceShortage":
                return stateManager.IsResourceInShortage(choice.requiredValue.ToString());
                
            case "NationRelation":
                string[] parts = choice.requiredValue.ToString().Split(':');
                if (parts.Length == 2)
                {
                    string nationName = parts[0];
                    float minValue = float.Parse(parts[1]);
                    return stateManager.GetNationRelation(nationName) >= minValue;
                }
                break;
        }
        
        return true; // Default to showing the choice if condition isn't recognized
    }
    
    // Handle continue button click
    private void OnContinueClicked()
    {
        // If typewriter is still running, complete it immediately
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            dialogueText.text = currentDialogueLines[currentLineIndex];
            typewriterCoroutine = null;
            return;
        }
        
        // Move to the next line
        currentLineIndex++;
        
        // Check if we've reached the end
        if (currentLineIndex >= currentDialogueLines.Length)
        {
            if (currentEvent != null && currentEvent.choices.Count > 0)
            {
                // If we have an active event with choices, show them
                ShowEventChoices();
            }
            else
            {
                // End of dialogue, hide the panel
                dialoguePanel.SetActive(false);
                currentEvent = null;
            }
            return;
        }
        
        // Display the next line
        DisplayCurrentLine();
    }
    
    // Original choice selection handler (maintain existing functionality)
    private void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"Selected choice {choiceIndex}");
        
        // Hide the dialogue panel
        dialoguePanel.SetActive(false);
        
        // Reset event
        currentEvent = null;
    }
    
    // NEW: Handle event choice selection and process outcomes
    private void OnEventChoiceSelected(int choiceIndex)
    {
        if (currentEvent == null || choiceIndex < 0 || choiceIndex >= currentEvent.choices.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueChoice choice = currentEvent.choices[choiceIndex];
        
        // Process all outcomes of this choice
        foreach (var outcome in choice.outcomes)
        {
            ProcessOutcome(outcome);
        }
        
        // Record this decision in player history
        if (stateManager != null)
        {
            stateManager.History.SignificantDecisions.Add($"{currentEvent.id}:{choiceIndex}");
            Debug.Log($"Recorded decision: {currentEvent.id}:{choiceIndex}");
        }
        
        EndDialogue();
    }
    
    // NEW: Process a single outcome
    private void ProcessOutcome(DialogueOutcome outcome)
    {
        if (stateManager == null)
        {
            Debug.LogWarning("Cannot process outcome: GameStateManager is null");
            return;
        }
        
        Debug.Log($"Processing outcome: {outcome.type} - {outcome.targetId} - {outcome.value}");
        
        switch (outcome.type)
        {
            case DialogueOutcome.OutcomeType.AddResource:
                // Would normally modify resource amounts
                Debug.Log($"Would add {outcome.value} of resource {outcome.targetId}");
                break;
                
            case DialogueOutcome.OutcomeType.RemoveResource:
                // Would normally modify resource amounts
                Debug.Log($"Would remove {outcome.value} of resource {outcome.targetId}");
                break;
                
            case DialogueOutcome.OutcomeType.ChangeRelation:
                // Update relation with nation
                if (stateManager.Diplomacy.NationRelations.ContainsKey(outcome.targetId))
                    stateManager.Diplomacy.NationRelations[outcome.targetId] += outcome.value;
                else
                    stateManager.Diplomacy.NationRelations[outcome.targetId] = outcome.value;
                    
                Debug.Log($"Changed relations with {outcome.targetId} by {outcome.value}");
                break;
                
            case DialogueOutcome.OutcomeType.ChangeSatisfaction:
                // Update region satisfaction
                if (stateManager.RegionStates.ContainsKey(outcome.targetId))
                {
                    stateManager.RegionStates[outcome.targetId].Satisfaction += outcome.value;
                    
                    // Clamp satisfaction between 0 and 1
                    stateManager.RegionStates[outcome.targetId].Satisfaction = 
                        Mathf.Clamp01(stateManager.RegionStates[outcome.targetId].Satisfaction);
                        
                    Debug.Log($"Changed satisfaction in {outcome.targetId} by {outcome.value}");
                }
                break;
                
            case DialogueOutcome.OutcomeType.SetEconomicPhase:
                // Change economic phase
                stateManager.Economy.CurrentEconomicCyclePhase = outcome.targetId;
                stateManager.Economy.TurnsInCurrentPhase = 0;
                Debug.Log($"Changed economic phase to {outcome.targetId}");
                break;
                
            case DialogueOutcome.OutcomeType.RecordDecision:
                // Just record this decision
                stateManager.History.SignificantDecisions.Add(outcome.targetId);
                Debug.Log($"Recorded decision: {outcome.targetId}");
                break;
        }
    }
    
    // End the dialogue
    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        currentEvent = null;
    }
    
    // Typewriter effect coroutine (unchanged)
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
    
    // Original static helper methods (preserve existing functionality)
    public static void ShowSimpleDialogue(string title, string message)
    {
        if (Instance != null)
            Instance.ShowDialogue(title, new string[] { message });
    }
    
    public static void ShowMultiLineDialogue(string title, string[] lines)
    {
        if (Instance != null)
            Instance.ShowDialogue(title, lines);
    }
    
    public static void ShowChoiceDialogue(string title, string message, string[] choices)
    {
        if (Instance != null)
            Instance.ShowDialogueWithChoices(title, message, choices);
    }
    
    // NEW: Static helper method for showing dialogue events
    public static void ShowEvent(SimpleDialogueEvent dialogueEvent)
    {
        if (Instance != null)
            Instance.ShowDialogueEvent(dialogueEvent);
    }
}