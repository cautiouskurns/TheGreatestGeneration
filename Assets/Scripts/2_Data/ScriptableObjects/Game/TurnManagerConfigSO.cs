using UnityEngine;

/// <summary>
/// Configuration for TurnManager
/// </summary>
[CreateAssetMenu(fileName = "TurnManagerConfig", menuName = "Game/Turn Manager Configuration")]
public class TurnManagerConfigSO : ScriptableObject
{
    [Header("Auto-Simulation Settings")]
    public bool defaultAutoSimulation = false;
    public float timeBetweenTurns = 3.0f;
    public int maxAutoTurns = 100;
}
