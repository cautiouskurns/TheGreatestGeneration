using UnityEngine;

namespace V1.UI
{ 
    /// CLASS PURPOSE:
    /// DashboardToggle handles the visibility toggling of the in-game economic dashboard UI.
    /// It ensures the dashboard updates with fresh data whenever it becomes visible.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Toggle the dashboard panel's active state on user interaction
    /// - Trigger an immediate data update when the panel is shown
    ///
    /// KEY COLLABORATORS:
    /// - EconomicDashboard: Handles logic for updating and rendering economic data
    /// - Unity UI: Dashboard panel is part of the Unity Canvas system
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Tightly coupled to a specific dashboard and panel via public fields
    /// - Assumes the dashboard update logic is synchronous and lightweight
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Add null checks or validation in `Awake` to verify panel and dashboard references
    /// - Consider adding animation or transition handling for toggling
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Integrate with keyboard shortcuts or menu events
    /// - Trigger analytics/logging when dashboard is opened or closed
    /// - Support multiple dashboard types or dynamically switchable panels
    public class DashboardToggle : MonoBehaviour
    {
        public GameObject dashboardPanel;
        public EconomicDashboard dashboard;

        public void ToggleDashboard()
        {
            if (dashboardPanel != null)
            {
                bool newState = !dashboardPanel.activeSelf;
                dashboardPanel.SetActive(newState);
                
                // Update immediately when opened
                if (newState && dashboard != null)
                {
                    dashboard.UpdateDashboard();
                }
            }
        }
    }
}