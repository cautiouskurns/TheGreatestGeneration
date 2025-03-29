using UnityEngine;
using System.Collections.Generic; 
using System; 

public class EventConditionEvaluator
{
    private GameStateManager stateManager;
    
    public EventConditionEvaluator(GameStateManager stateManager)
    {
        this.stateManager = stateManager;
    }
    
    public bool EvaluateCondition(EventCondition condition)
    {
        try
        {
            switch (condition.conditionType)
            {
                case EventCondition.ConditionType.EconomicCyclePhase:
                    // Check current economic cycle phase
                    string currentPhase = stateManager.GetCurrentEconomicCyclePhase();
                    return currentPhase == condition.targetValue;
                    
                case EventCondition.ConditionType.ResourceShortage:
                    // Check if specified resource is in shortage
                    return stateManager.IsResourceInShortage(condition.targetValue);
                    
                case EventCondition.ConditionType.ResourceSurplus:
                    // Check if specified resource is in surplus
                    return stateManager.IsResourceInSurplus(condition.targetValue);
                    
                case EventCondition.ConditionType.NationRelation:
                    // Get relation value with specified nation
                    float relationValue = stateManager.GetNationRelation(condition.targetValue);
                    return CompareValues(relationValue, condition.numericValue, condition.comparisonOperator);
                    
                case EventCondition.ConditionType.RegionSatisfaction:
                    // Get satisfaction for specified region
                    float satisfaction = stateManager.GetRegionSatisfaction(condition.targetValue);
                    return CompareValues(satisfaction, condition.numericValue, condition.comparisonOperator);
                    
                case EventCondition.ConditionType.TurnNumber:
                    // Compare current turn number
                    int currentTurn = stateManager.GetCurrentTurn();
                    return CompareValues(currentTurn, condition.numericValue, condition.comparisonOperator);
                    
                case EventCondition.ConditionType.GenerationNumber:
                    // Compare current generation number
                    int currentGeneration = stateManager.GetCurrentGeneration();
                    return CompareValues(currentGeneration, condition.numericValue, condition.comparisonOperator);
                    
                case EventCondition.ConditionType.ResourceAmount:
                    // Get amount of specified resource
                    float resourceAmount = stateManager.GetResourceAmount(condition.targetValue);
                    return CompareValues(resourceAmount, condition.numericValue, condition.comparisonOperator);
                    
                case EventCondition.ConditionType.InfrastructureLevel:
                    // Get level of specified infrastructure
                    float infrastructureLevel = stateManager.GetInfrastructureLevel(condition.targetValue);
                    return CompareValues(infrastructureLevel, condition.numericValue, condition.comparisonOperator);
                    
                case EventCondition.ConditionType.PreviousEventOccurred:
                    // Check if specified event has occurred
                    return stateManager.HasEventOccurred(condition.targetValue);
                    
                default:
                    Debug.LogWarning($"Unhandled condition type: {condition.conditionType}");
                    return false;
            }
        }
        catch (Exception ex)
        {
            // Log error but don't crash the game
            Debug.LogError($"Error evaluating condition {condition.conditionType}: {ex.Message}");
            return false;
        }
    }
    
    private bool CompareValues(float actual, float expected, EventCondition.ComparisonOperator op)
    {
        switch (op)
        {
            case EventCondition.ComparisonOperator.Equals:
                return Mathf.Approximately(actual, expected);
            case EventCondition.ComparisonOperator.NotEquals:
                return !Mathf.Approximately(actual, expected);
            case EventCondition.ComparisonOperator.GreaterThan:
                return actual > expected;
            case EventCondition.ComparisonOperator.LessThan:
                return actual < expected;
            case EventCondition.ComparisonOperator.GreaterThanOrEqual:
                return actual >= expected;
            case EventCondition.ComparisonOperator.LessThanOrEqual:
                return actual <= expected;
            default:
                Debug.LogWarning($"Unhandled comparison operator: {op}");
                return false;
        }
    }
    
    public bool CheckEventEligibility(StoryEventDataSO storyEvent)
    {
        // Check all conditions for this event
        foreach (var condition in storyEvent.conditions)
        {
            if (!EvaluateCondition(condition))
                return false;
        }
        
        // Check cooldown period for this event
        int lastOccurrenceTurn = stateManager.GetLastEventOccurrenceTurn(storyEvent.eventId);
        if (lastOccurrenceTurn >= 0)  // -1 could indicate event has never occurred
        {
            int currentTurn = stateManager.GetCurrentTurn();
            int turnsSince = currentTurn - lastOccurrenceTurn;
            
            if (turnsSince < storyEvent.cooldownTurns)
                return false;
        }
        
        return true;
    }
}