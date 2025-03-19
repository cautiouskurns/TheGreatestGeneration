using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public void EndTurn()
    {
        Debug.Log("Turn Ended. Processing Economy...");
        EventBus.Trigger("TurnEnded");
    }
}
