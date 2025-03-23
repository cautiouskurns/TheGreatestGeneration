using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private bool isPlayerTurn = true;
    
    void Start()
    {
        // Start with player's turn
        isPlayerTurn = true;
    }
    
    public void EndTurn()
    {
        if (isPlayerTurn)
        {
            Debug.Log("Player Turn Ended. AI's turn now.");
            EventBus.Trigger("TurnStarted"); // Optional: Add this if you want pre-turn processing
            EventBus.Trigger("TurnEnded");
            EventBus.Trigger("PlayerTurnEnded"); // New event for AI to listen to
            
            isPlayerTurn = false;
        }
    }
    
    // This will be called by the AIController when all AI turns are done
    private void OnEnable()
    {
        EventBus.Subscribe("AITurnsCompleted", OnAITurnsCompleted);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("AITurnsCompleted", OnAITurnsCompleted);
    }
    
    private void OnAITurnsCompleted(object _)
    {
        Debug.Log("AI Turns Completed. Player's turn now.");
        EventBus.Trigger("TurnProcessed"); // Update UI after all processing
        
        isPlayerTurn = true;
    }
}
