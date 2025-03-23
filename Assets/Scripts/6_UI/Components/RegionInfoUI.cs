using UnityEngine;
using TMPro;

public class RegionInfoUI : MonoBehaviour
{
    public TextMeshProUGUI infoText;

    private void Start()
    {
        Debug.Log("RegionInfoUI Start method called");
        
        if (infoText == null)
        {
            Debug.LogError("InfoText reference is null in RegionInfoUI");
        }
        else
        {
            Debug.Log("InfoText reference is valid");
            infoText.text = "Waiting for region selection...";
        }
    }

    private void OnEnable()
    {
        EventBus.Subscribe("RegionSelected", UpdateInfo);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe("RegionSelected", UpdateInfo);
    }

    private void UpdateInfo(object regionObj)
    {
        Debug.Log("RegionSelected event received");
        
        RegionEntity region = regionObj as RegionEntity;
        if (region == null)
        {
            Debug.LogError("Region is null or not a RegionEntity");
            return;
        }

        Debug.Log($"Updating UI for region: {region.regionName}");

        if (infoText == null)
        {
            Debug.LogError("InfoText reference is null");
            return;
        }

        infoText.text = $"<b>{region.regionName}</b>\n" +
                        $"<color=#FFD700>Wealth: {region.wealth}</color>\n" +
                        $"<color=#87CEEB>Production: {region.production}</color>\n" +
                        $"Nation: {region.ownerNationName}";
        
        // Add a simple growth trend indicator if available
        if (region.hasChangedThisTurn)
        {
            string trend = region.wealthDelta > 0 ? "<color=green>↑</color>" : 
                          (region.wealthDelta < 0 ? "<color=red>↓</color>" : "→");
            infoText.text += $"\nTrend: {trend}";
        }
    }
}
