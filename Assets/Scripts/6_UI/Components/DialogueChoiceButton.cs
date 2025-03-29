using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class DialogueChoiceButton : MonoBehaviour
{
    public TextMeshProUGUI choiceText;
    public event Action<EventChoiceData> OnChoiceSelected;
    
    private EventChoiceData choiceData;
    private Button button;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
        
        if (choiceText == null)
            choiceText = GetComponentInChildren<TextMeshProUGUI>();
    }
    
    public void SetChoice(string text, EventChoiceData data)
    {
        choiceData = data;
        
        if (choiceText != null)
            choiceText.text = text;
    }
    
    private void HandleClick()
    {
        OnChoiceSelected?.Invoke(choiceData);
    }
    
    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}