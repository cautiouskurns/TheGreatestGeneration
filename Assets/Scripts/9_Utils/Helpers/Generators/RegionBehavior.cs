/// CLASS PURPOSE:
/// RegionBehavior is a simple MonoBehaviour attached to region GameObjects
/// that allows them to be interactively selected by the player via mouse input.
///
/// CORE RESPONSIBILITIES:
/// - Handle mouse click events on the region's GameObject
/// - Notify the MapManager of the selected region by name
///
/// KEY COLLABORATORS:
/// - MapManager: Processes the region selection and updates game state/UI
///
/// CURRENT ARCHITECTURE NOTES:
/// - Uses Unity's built-in OnMouseDown for interaction, which requires a collider
/// - Assumes the regionName is unique and correctly assigned in the inspector
///
/// REFACTORING SUGGESTIONS:
/// - Use Unity's new input system or raycast-based interaction for more flexibility
/// - Consider caching references or using events instead of direct method calls
///
/// EXTENSION OPPORTUNITIES:
/// - Add hover effects, tooltip display, or region highlighting
/// - Integrate with a selection manager for complex selection logic
using UnityEngine;

public class RegionBehavior : MonoBehaviour
{
    public string regionName;
    public MapManager mapManager;

    void OnMouseDown()
    {
        mapManager.SelectRegion(regionName);
    }
}