using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NationDashboardUI : MonoBehaviour
{
    public TextMeshProUGUI nationInfoText;
    public Image nationFlag;
    
    private void OnEnable()
    {
        EventBus.Subscribe("NationSelected", UpdateInfo);
        EventBus.Subscribe("NationUpdated", UpdateInfo);
    }
    
    private void OnDisable()
    {
        EventBus.Unsubscribe("NationSelected", UpdateInfo);
        EventBus.Unsubscribe("NationUpdated", UpdateInfo);
    }
    
    private void UpdateInfo(object nationObj)
    {
        NationEntity nation = nationObj as NationEntity;
        if (nation == null) return;
        
        // Update nation info text
        nationInfoText.text = nation.GetNationSummary();
        
        // Update nation color
        nationInfoText.color = nation.nationColor;
        
        // You could set a nation flag image here if available
    }
}