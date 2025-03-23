using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public void EndTurn()
    {
        Debug.Log("Turn Ended. Processing Economy...");
        EventBus.Trigger("TurnStarted"); // Optional: Add this if you want pre-turn processing
        EventBus.Trigger("TurnEnded");
        EventBus.Trigger("TurnProcessed"); // Add this to signal UI updates after processing
    }
}

