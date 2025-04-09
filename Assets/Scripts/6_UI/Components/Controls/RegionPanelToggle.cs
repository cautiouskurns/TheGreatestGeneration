/// CLASS PURPOSE:
/// RegionPanelToggle controls the visibility of the region-specific dashboard panel,
/// allowing users to toggle regional data UI on or off during gameplay.
///
/// CORE RESPONSIBILITIES:
/// - Toggle the active state of the region information panel
/// - Maintain reference to the associated RegionInfoUI for potential updates
///
/// KEY COLLABORATORS:
/// - RegionInfoUI: Displays detailed statistics and information about a selected region
/// - Unity UI: Dashboard panel managed through standard GameObject activation
///
/// CURRENT ARCHITECTURE NOTES:
/// - Simple toggle logic using `SetActive`
/// - No data refresh or state persistence implemented yet
///
/// REFACTORING SUGGESTIONS:
/// - Add safety checks or logging if dashboard reference is missing
/// - Decouple visual toggle from data update logic for separation of concerns
///
/// EXTENSION OPPORTUNITIES:
/// - Automatically refresh data on panel activation
/// - Animate panel transitions for better user feedback
/// - Add context-based toggle behavior (e.g., deselecting closes panel)

using UnityEngine;

public class RegionPanelToggle : MonoBehaviour
{
    public GameObject dashboardPanel;
    public RegionInfoUI dashboard;

    public void ToggleRegionPanel()
    {
        if (dashboardPanel != null)
        {
            bool newState = !dashboardPanel.activeSelf;
            dashboardPanel.SetActive(newState);
        }
    }
}