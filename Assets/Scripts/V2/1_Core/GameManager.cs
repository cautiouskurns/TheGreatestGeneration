using UnityEngine;
using V2.Entities;
using V2.Managers;
using V2.Systems;

/// CLASS PURPOSE:
/// GameManager is the central controller for initializing and managing the core simulation loop.
/// It holds and manages references to key simulation objects and responds to global game events.
/// Implemented as a singleton to allow global access throughout the application.
/// 
/// CORE RESPONSIBILITIES:
/// - Provide global access to game state and systems via singleton pattern
/// - Instantiate and maintain RegionEntities used in the simulation
/// - Listen for global "TurnEnded" events and trigger appropriate updates
/// - Relay region updates to the console or future UI components
/// 
/// KEY COLLABORATORS:
/// - RegionEntity: Encapsulates the economic and resource simulation per region
/// - EventBus: Manages communication of global events (e.g., turn progression)
/// - TurnManager: Issues global simulation triggers like TurnEnded
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - Single region setup for prototype; easily extendable to support multiple regions
/// - Event-driven architecture ensures decoupling from TurnManager and other systems
/// - Singleton pattern provides centralized access point but should be used judiciously
/// 
/// REFACTORING SUGGESTIONS:
/// - Abstract region management into a dedicated RegionManager for multi-region support
/// - Consider splitting simulation logic from UI response in future versions
/// - For larger projects, consider service locator or dependency injection alternatives
/// 
/// EXTENSION OPPORTUNITIES:
/// - Add support for initialization via ScriptableObjects
/// - Emit additional events for turn start/end or region-specific changes
/// - Integrate logging or analytics hooks to monitor economic progression

namespace V2.Core
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton Implementation
        private static GameManager _instance;

        /// <summary>
        /// Global access point for GameManager
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find existing instance
                    _instance = FindFirstObjectByType<GameManager>();

                    // Create new instance if none exists
                    if (_instance == null)
                    {
                        GameObject gameManagerObject = new GameObject("GameManager");
                        _instance = gameManagerObject.AddComponent<GameManager>();
                        Debug.Log("GameManager instance created dynamically");
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Enforce singleton pattern on awake
        /// </summary>
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                // Destroy duplicate instances
                Debug.LogWarning($"Multiple GameManager instances detected. Destroying duplicate on {gameObject.name}");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes

            // Initialize test region
            // Create shared region
            testRegion = new RegionEntity("Test Region", 100, 50);
            EconomicSystem.Instance.testRegion = testRegion;
            Debug.Log($"GameManager initialized with test region: {testRegion.Name}");
        }
        #endregion

        #region Game State
        public RegionEntity testRegion;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            
        }

        private void OnEnable()
        {
            EventBus.Subscribe("TurnEnded", OnTurnEnded);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe("TurnEnded", OnTurnEnded);
        }
        #endregion

        #region Event Handlers
        private void OnTurnEnded(object _)
        {
            testRegion.ProcessTurn();
            Debug.Log(testRegion.GetSummary());
            
            // Broadcast that regions have been updated
            EventBus.Trigger("RegionsUpdated", null);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Get the test region (will be expanded for multi-region support in future)
        /// </summary>
        /// <returns>The current test region entity</returns>
        public RegionEntity GetTestRegion()
        {
            return testRegion;
        }

        /// <summary>
        /// Reset the game state (useful for testing or new games)
        /// </summary>
        public void ResetGame()
        {
            testRegion = new RegionEntity("Test Region", 100, 50);
            Debug.Log("Game state has been reset");
            
            // Notify any listeners that the game state has been reset
            EventBus.Trigger("GameReset", null);
        }
        #endregion
    }
}
