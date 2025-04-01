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