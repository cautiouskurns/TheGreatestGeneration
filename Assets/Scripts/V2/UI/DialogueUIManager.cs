using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using V2.Systems.DialogueSystem;

namespace V2.UI
{
    public class DialogueUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;
        
        private DialogueEventManager dialogueManager;
        private List<GameObject> choiceButtons = new List<GameObject>();
        
        private void Awake()
        {
            dialogueManager = FindFirstObjectByType<DialogueEventManager>();
            
            // Hide panel initially
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
        
        private void OnEnable()
        {
            // Subscribe to events
            EventBus.Subscribe("DisplayEvent", OnDisplayEvent);
            EventBus.Subscribe("EventCompleted", OnEventCompleted);
        }
        
        private void OnDisable()
        {
            // Unsubscribe from events
            EventBus.Unsubscribe("DisplayEvent", OnDisplayEvent);
            EventBus.Unsubscribe("EventCompleted", OnEventCompleted);
        }
        
        private void OnDisplayEvent(object data)
        {
            if (data is DialogueEvent dialogueEvent)
            {
                DisplayEvent(dialogueEvent);
            }
        }
        
        private void OnEventCompleted(object data)
        {
            // Hide the dialogue panel
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }
        
        private void DisplayEvent(DialogueEvent dialogueEvent)
        {
            // Show the panel
            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);
                
            // Set the text
            if (titleText != null)
                titleText.text = dialogueEvent.title;
                
            if (descriptionText != null)
                descriptionText.text = dialogueEvent.description;
                
            // Clear existing choice buttons
            ClearChoiceButtons();
            
            // Create new choice buttons
            CreateChoiceButtons(dialogueEvent);
        }
        
        private void ClearChoiceButtons()
        {
            foreach (GameObject button in choiceButtons)
            {
                Destroy(button);
            }
            
            choiceButtons.Clear();
        }
        
        private void CreateChoiceButtons(DialogueEvent dialogueEvent)
        {
            if (choicesContainer == null || choiceButtonPrefab == null)
                return;
                
            for (int i = 0; i < dialogueEvent.choices.Count; i++)
            {
                EventChoice choice = dialogueEvent.choices[i];
                GameObject buttonObj = Instantiate(choiceButtonPrefab, choicesContainer);
                
                // Set button text
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = choice.text;
                    
                // Set button click handler
                Button button = buttonObj.GetComponent<Button>();
                if (button != null)
                {
                    int choiceIndex = i; // Capture for lambda
                    button.onClick.AddListener(() => {
                        OnChoiceSelected(choiceIndex);
                    });
                }
                
                choiceButtons.Add(buttonObj);
            }
        }
        
        private void OnChoiceSelected(int choiceIndex)
        {
            if (dialogueManager != null && dialogueManager.CurrentEvent != null)
            {
                // Make the choice
                dialogueManager.MakeChoice(choiceIndex);
            }
        }
    }
}