/// CLASS PURPOSE:
/// RegionClickHandler is a MonoBehaviour attached to individual region GameObjects
/// that enables click-based selection. It notifies the system via EventBus when the region is clicked.
/// 
/// CORE RESPONSIBILITIES:
/// - Handle mouse click input on the region
/// - Dispatch a "RegionClicked" event with the region's name as payload
/// 
/// KEY COLLABORATORS:
/// - EventBus: Broadcasts the "RegionClicked" event to any subscribed listeners
/// - MapController or UI Systems: Subscribe to the event to react to selection
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - Requires a Collider to receive OnMouseDown events
/// - Assumes regionName is assigned in the Inspector or during instantiation
/// 
/// REFACTORING SUGGESTIONS:
/// - Replace OnMouseDown with raycast-based input or new Input System for more flexibility
/// - Add null or validity checks for regionName before triggering the event
/// 
/// EXTENSION OPPORTUNITIES:
/// - Add visual or audio feedback on selection
/// - Support double-click or right-click variations for advanced interactions
/// - Integrate with selection highlight or tooltip display systems
using UnityEngine;

public class RegionClickHandler : MonoBehaviour
{
    public string regionName;

    void OnMouseDown()
    {
        Debug.Log($"RegionClickHandler: Mouse down on {regionName}");
        
        // Trigger the RegionClicked event that MapController listens for
        EventBus.Trigger("RegionClicked", regionName);
    }
}