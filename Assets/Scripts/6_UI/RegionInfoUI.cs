using UnityEngine;
using TMPro;

public class RegionInfoUI : MonoBehaviour
{
    public TextMeshProUGUI infoText;

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
        RegionEntity region = regionObj as RegionEntity;

        infoText.text = $"🏙️ {region.regionName}\n" +
                        $"💰 Wealth: {region.wealth}\n" +
                        $"🏭 Production: {region.production}";
    }
}

