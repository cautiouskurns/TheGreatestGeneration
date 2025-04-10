using UnityEngine;
using V2.Managers;
using System.Collections;

/// CLASS PURPOSE:
/// TurnManager handles real-time ticking and global turn-based progression,
/// with support for pausing and customizable tick speed.
///
/// CORE RESPONSIBILITIES:
/// - Continuously trigger game ticks at specified intervals
/// - Broadcast "TurnEnded" event using the EventBus each tick
/// - Support pause/resume and time scale adjustment
///
/// KEY COLLABORATORS:
/// - EventBus: Used to publish the "TurnEnded" event
/// - GameManager: Subscribes to this event to trigger region updates
///
/// CURRENT ARCHITECTURE NOTES:
/// - Uses coroutine to simulate time-based progression
/// - Easily extensible to support multiple tick speeds or event-driven interruptions
///
/// REFACTORING SUGGESTIONS:
/// - Extract tick timing to external config or ScriptableObject
/// - Consider integrating with Unity’s Time.timeScale for better simulation control
///
/// EXTENSION OPPORTUNITIES:
/// - Support different time units (e.g., days, months)
/// - Integrate with UI to show game time
/// - Pause on key in-game events or decision points

namespace V2.Core
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private float tickIntervalSeconds = 1.0f;  // Duration of one tick
        [SerializeField] private float timeScale = 1.0f;            // Multiplier for tick speed

        private bool isPaused = false;
        private Coroutine tickRoutine;

        private int currentDay = 1;
        private int currentYear = 1;
        private const int DaysPerYear = 365;

        private void Start()
        {
            tickRoutine = StartCoroutine(TickLoop());
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isPaused) Resume();
                else Pause();
            }
        }

        private IEnumerator TickLoop()
        {
            while (true)
            {
                if (!isPaused)
                {
                    Debug.Log("=============================================================================================================");
                    Debug.Log($"Game Date: Day {currentDay}, Year {currentYear}");

                    EventBus.Trigger("TurnEnded");

                    currentDay++;
                    if (currentDay > DaysPerYear)
                    {
                        currentDay = 1;
                        currentYear++;
                    }
                }

                yield return new WaitForSeconds(tickIntervalSeconds / timeScale);
            }
        }

        public void Pause()
        {
            Debug.Log("Simulation paused");
            isPaused = true;
        }

        public void Resume()
        {
            Debug.Log("Simulation resumed");
            isPaused = false;
        }

        public void SetTimeScale(float newTimeScale)
        {
            timeScale = Mathf.Max(0.1f, newTimeScale);  // Avoid zero or negative scale
        }
    }
}