using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIController : MonoBehaviour
{
    [Header("AI Configuration")]
    public float turnDelay = 1.5f; // Time in seconds for the AI to "think"
    public GameObject aiThinkingIndicator; // Reference to the spinning wheel UI
    
    [Header("UI References")]
    public AIActionDisplay actionDisplay; // Reference to the action display
    
    private GameManager gameManager;
    private MapModel mapModel;
    
    private void Awake()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        // Hide the indicator at start
        if (aiThinkingIndicator != null)
        {
            aiThinkingIndicator.SetActive(false);
        }
    }
    
    private void OnEnable()
    {
        EventBus.Subscribe("PlayerTurnEnded", OnPlayerTurnEnded);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("PlayerTurnEnded", OnPlayerTurnEnded);
    }
    
    private void OnPlayerTurnEnded(object _)
    {
        // When player ends turn, start AI turns
        StartCoroutine(ProcessAITurns());
    }
    
    private IEnumerator ProcessAITurns()
    {
        // Show the thinking indicator
        if (aiThinkingIndicator != null)
        {
            aiThinkingIndicator.SetActive(true);
        }
        
        Debug.Log("AI turn started");
        
        // Get all nations from the map model
        MapDataSO mapData = gameManager.GetMapData();
        if (mapData != null)
        {
            foreach (var nation in mapData.nations)
            {
                // Skip player's nation
                if (nation.nationName == "Player Nation") // Replace with your way of identifying player nation
                    continue;
                
                // Display nation turn start
                if (actionDisplay != null)
                {
                    actionDisplay.DisplayAction($"Nation {nation.nationName} is taking its turn...", nation.nationColor);
                }
                
                // Simulate "thinking" time
                yield return new WaitForSeconds(turnDelay);
                
                // Make simple decisions for this AI nation
                ProcessNationTurn(nation);
            }
        }
        
        // Hide the thinking indicator
        if (aiThinkingIndicator != null)
        {
            aiThinkingIndicator.SetActive(false);
        }
        
        Debug.Log("AI turn completed");
        
        // Signal that AI turns are complete
        EventBus.Trigger("AITurnsCompleted", null);
    }
    
    private void ProcessNationTurn(MapDataSO.NationData nation)
    {
        Debug.Log($"Processing turn for AI nation: {nation.nationName}");
        
        // SIMPLE DECISION MAKING
        
        // 1. Random resource allocation
        SimulateResourceAllocation(nation);
        
        // 2. Random project selection
        SimulateProjectSelection(nation);
        
        // Log decisions
        Debug.Log($"AI {nation.nationName} completed its turn");
    }
    
    private void SimulateResourceAllocation(MapDataSO.NationData nation)
    {
        // Just log this for now
        Debug.Log($"AI {nation.nationName} is allocating resources");
        
        // Example decisions to log:
        string[] resources = { "Food", "Materials", "Wealth" };
        string[] allocations = { "Industry", "Research", "Military", "Infrastructure" };
        
        string resource = resources[Random.Range(0, resources.Length)];
        string allocation = allocations[Random.Range(0, allocations.Length)];
        
        // Display the action with nation color
        if (actionDisplay != null)
        {
            actionDisplay.DisplayAction($"{nation.nationName} allocated {resource} to {allocation}", nation.nationColor);
        }
        
        Debug.Log($"AI {nation.nationName} allocated {resource} to {allocation}");
    }
    
    private void SimulateProjectSelection(MapDataSO.NationData nation)
    {
        // Just log this for now
        Debug.Log($"AI {nation.nationName} is selecting projects");
        
        // Example projects to log:
        string[] projects = { "Road Network", "Research Center", "Farm Expansion", "Factory", "Mine", "Harbor" };
        
        // Select a random region and project
        if (nation.regions.Length > 0)
        {
            int regionIndex = Random.Range(0, nation.regions.Length);
            string regionName = nation.regions[regionIndex].regionName;
            string project = projects[Random.Range(0, projects.Length)];
            
            // Display the action with nation color
            if (actionDisplay != null)
            {
                actionDisplay.DisplayAction($"{nation.nationName} started '{project}' in {regionName}", nation.nationColor);
            }
            
            Debug.Log($"AI {nation.nationName} started project '{project}' in region {regionName}");
        }
    }
}