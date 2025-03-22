using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public void EndTurn() // Must be public, non-static
    {
        Debug.Log("Turn Ended. Processing Economy...");
        EventBus.Trigger("TurnEnded");
    }
}

