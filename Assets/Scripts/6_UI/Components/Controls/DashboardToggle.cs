using UnityEngine;

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