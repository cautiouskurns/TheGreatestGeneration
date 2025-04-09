using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using V1.Managers;
using V1.Entities;
using V1.Systems;

namespace V1.Core
{
    /// CLASS PURPOSE:
    /// EventDialogueManager controls all dialogue-based interactions during events.
    /// It manages UI rendering, typewriter effects, choice presentation, and event outcomes,
    /// acting as a central system for narrative delivery and decision integration.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Display dialogue panels with optional typewriter effects
    /// - Show branching dialogue choices with conditional visibility
    /// - Trigger outcomes based on player decisions (e.g., economy, relations, satisfaction)
    /// - Integrate dialogue with GameStateManager for dynamic text and effects
    /// - Broadcast events such as DialogueEnded for downstream listeners
    ///
    /// KEY COLLABORATORS:
    /// - GameStateManager: Provides contextual data and tracks decision outcomes
    /// - UI Elements (TextMeshPro, Buttons): Used to render and manage dialogue/choice interactions
    /// - EventBus: Emits global events tied to dialogue lifecycle
    /// - SimpleDialogueEvent, DialogueOutcome: Data structures that define event flows
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Singleton pattern for centralized control
    /// - Integrates UI logic with game logic (SRP concerns)
    /// - Uses reflection-like variable replacement for dynamic narrative text
    /// - Event-driven, but heavily coupled to UI layout
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Consider separating core logic from UI (DialogueUIController)
    /// - Expand outcome types to support additional gameplay mechanics
    /// - Move narrative logic into a dialogue scripting asset format for scalability
    /// - Enable more dynamic and reusable dialogue blocks via templates or tokens
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Skill check integration (RPG elements)
    /// - Procedural or reactive dialogue generation
    /// - Branch visualization or debugging tools for designers
    /// 

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
        
        public void ShowDialogueEvent(SimpleDialogueEvent dialogueEvent)
        {
            currentEvent = dialogueEvent;
            
            // Make sure we have the latest reference to the state manager
            if (stateManager == null)
                stateManager = GameStateManager.Instance;
            
            // Process lines with state variables
            if (dialogueEvent.lines.Count > 0)
            {
                List<string> processedLines = new List<string>();
                foreach (var line in dialogueEvent.lines)
                {
                    // IMPORTANT: Use the line's GetProcessedText method instead
                    string processedText = line.GetProcessedText(stateManager);
                    processedLines.Add(processedText);
                    
                    // Log for debugging
                    Debug.Log($"Original line: '{line.text}'");
                    Debug.Log($"Processed line: '{processedText}'");
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
            if (stateManager == null)
            {
                stateManager = GameStateManager.Instance;
                if (stateManager == null)
                {
                    Debug.LogWarning("GameStateManager not found for text processing");
                    return text;
                }
            }
            
            string processed = text;
            
            // Direct replacements (same as in DialogueLine.GetProcessedText)
            processed = processed.Replace("{EconomicPhase}", stateManager.Economy.CurrentEconomicCyclePhase);
            processed = processed.Replace("{CurrentTurn}", stateManager.GetCurrentTurn().ToString());
            
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

                case DialogueOutcome.OutcomeType.ModifyProductionEfficiency:
                    ModifyRegionProduction(outcome.targetId, outcome.value, outcome.durationTurns);
                break;
                
                // case DialogueOutcome.OutcomeType.ModifyInfrastructure:
                //     ModifyRegionInfrastructure(outcome.targetId, outcome.value);
                //     break;
                    
                case DialogueOutcome.OutcomeType.ModifyResourcePrice:
                    ModifyResourcePrice(outcome.targetId, outcome.value);
                    break;
                    
                case DialogueOutcome.OutcomeType.GrantProject:
                    GrantProjectToRegion(outcome.targetId, outcome.value);
                    break;
                    
                // case DialogueOutcome.OutcomeType.ChangeLabor:
                //     ChangeRegionLabor(outcome.targetId, (int)outcome.value);
                //     break;
            }
        }
        
        // End the dialogue
        private void EndDialogue()
        {
            dialoguePanel.SetActive(false);
            currentEvent = null;

            EventBus.Trigger("DialogueEnded");
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

        // Helper methods to implement economic outcomes
        private void ModifyRegionProduction(string regionName, float modifier, int durationTurns)
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null) return;
            
            RegionEntity region = gameManager.GetRegion(regionName);
            if (region != null)
            {
                // Apply temporary production boost/penalty
                region.productionEfficiency += modifier;
                
                // Record this effect for duration tracking if temporary
                if (durationTurns > 0)
                {
                    TrackTemporaryEffect("production", regionName, modifier, durationTurns);
                }
                
                Debug.Log($"Applied production modifier {modifier:+0.00;-0.00} to {regionName} for {durationTurns} turns");
            }
        }

        private void ModifyResourcePrice(string resourceName, float priceAdjustment)
        {
            var resourceMarket = ResourceMarket.Instance;
            if (resourceMarket != null)
            {
                // Apply a market price adjustment
                float currentPrice = resourceMarket.GetCurrentPrice(resourceName);
                float newPrice = currentPrice * (1 + priceAdjustment);
                
                resourceMarket.AdjustPrice(resourceName, newPrice);
                Debug.Log($"Adjusted price of {resourceName} by {priceAdjustment:P0}");
            }
        }

        private void GrantProjectToRegion(string regionName, float projectId)
        {
            var gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager == null) return;
            
            RegionEntity region = gameManager.GetRegion(regionName);
            if (region != null)
            {
                // Find the project by ID and grant it
                // This is a simplified implementation - you would need to adapt to your project system
                string projectName = $"Project_{(int)projectId}";
                
                // Assuming your production component has an ActivateRecipe method
                if (region.productionComponent != null)
                {
                    region.productionComponent.ActivateRecipe(projectName);
                    Debug.Log($"Granted project {projectName} to {regionName}");
                }
            }
        }

        // Method to track temporary effects that need to expire after X turns
        private void TrackTemporaryEffect(string effectType, string targetId, float value, int duration)
        {
            // Create a temporary effect record in GameStateManager
            TemporaryEffect effect = new TemporaryEffect
            {
                Type = effectType,
                TargetId = targetId,
                Value = value,
                RemainingTurns = duration
            };
            
            // Add to state manager's list of temporary effects
            stateManager.AddTemporaryEffect(effect);
        }
    }
}