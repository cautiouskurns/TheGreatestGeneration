using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AIActionDisplay : MonoBehaviour
{
    [Header("UI Configuration")]
    public TextMeshProUGUI actionText;
    public float messageDuration = 3f;
    public int maxMessages = 5;

    private Queue<string> messageQueue = new Queue<string>();
    private Coroutine displayCoroutine;

    private void Awake()
    {
        if (actionText == null)
        {
            Debug.LogError("Action Text reference is missing on AIActionDisplay");
        }
        else
        {
            actionText.text = "";
        }
    }

    public void DisplayAction(string message, Color nationColor)
    {
        // Add message to queue with rich text color formatting
        string colorHex = ColorUtility.ToHtmlStringRGB(nationColor);
        string coloredMessage = $"<color=#{colorHex}>{message}</color>";
        
        messageQueue.Enqueue(coloredMessage);
        
        // Trim queue if it gets too long
        while (messageQueue.Count > maxMessages)
        {
            messageQueue.Dequeue();
        }
        
        // Update the display
        UpdateDisplayText();
        
        // Start or reset the clear timer
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        displayCoroutine = StartCoroutine(ClearAfterDelay());
    }

    private void UpdateDisplayText()
    {
        if (actionText != null)
        {
            // Build the text from all messages in the queue
            string displayText = "";
            foreach (string message in messageQueue)
            {
                displayText += message + "\n";
            }
            actionText.text = displayText;
        }
    }

    private IEnumerator ClearAfterDelay()
    {
        // Wait longer when we have more messages
        yield return new WaitForSeconds(messageDuration * messageQueue.Count);
        
        // Clear all messages
        messageQueue.Clear();
        if (actionText != null)
        {
            actionText.text = "";
        }
        
        displayCoroutine = null;
    }
}
