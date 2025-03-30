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
    }
    
    // Method to display dialogue with multiple lines
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
    
    // Method to show dialogue with choices
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
    
    // Helper method to display the current dialogue line
    private void DisplayCurrentLine()
    {
        // Check if we have dialogue to display
        if (currentDialogueLines == null || currentLineIndex >= currentDialogueLines.Length)
        {
            // End of dialogue, hide the panel
            dialoguePanel.SetActive(false);
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
    
    // Create and display choice buttons
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
            // End of dialogue, hide the panel
            dialoguePanel.SetActive(false);
            return;
        }
        
        // Display the next line
        DisplayCurrentLine();
    }
    
    // Handle choice selection
    private void OnChoiceSelected(int choiceIndex)
    {
        Debug.Log($"Selected choice {choiceIndex}");
        
        // Hide the dialogue panel
        dialoguePanel.SetActive(false);
        
        // You would normally process the choice here or trigger events
    }
    
    // Typewriter effect coroutine
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
    
    // Static helper methods for easy access
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
}