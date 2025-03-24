using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RegionInfoUI : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public Image terrainIcon;

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
        
        if (terrainIcon != null)
        {
            terrainIcon.gameObject.SetActive(false);
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

        // Update the region info text with rich text formatting
        string infoString = $"<size=24><b>{region.regionName}</b></size>\n";
        
        // Add terrain info if available
        if (region.terrainType != null)
        {
            infoString += $"<color=#8888FF><b>Terrain:</b> {region.terrainType.terrainName}</color>\n";
        }
        
        infoString += $"<color=#FFD700><b>Wealth:</b> {region.wealth}</color>\n" +
                      $"<color=#87CEEB><b>Production:</b> {region.production}</color>\n" +
                      $"<b>Nation:</b> {region.ownerNationName}";
        
        // Add a simple growth trend indicator if available
        if (region.hasChangedThisTurn)
        {
            string trend = region.wealthDelta > 0 ? "<color=green>↑</color>" : 
                          (region.wealthDelta < 0 ? "<color=red>↓</color>" : "→");
            infoString += $"\n<b>Trend:</b> {trend}";
        }
        
        // Add terrain effects if available
        if (region.terrainType != null)
        {
            infoString += "\n\n<size=18><b>Terrain Effects:</b></size>\n";
            
            float agricultureMod = region.terrainType.GetMultiplierForSector("agriculture");
            float miningMod = region.terrainType.GetMultiplierForSector("mining");
            float industryMod = region.terrainType.GetMultiplierForSector("industry");
            float commerceMod = region.terrainType.GetMultiplierForSector("commerce");
            
            if (agricultureMod != 1.0f)
                infoString += FormatModifierText("Agriculture", agricultureMod);
            
            if (miningMod != 1.0f)
                infoString += FormatModifierText("Mining", miningMod);
            
            if (industryMod != 1.0f)
                infoString += FormatModifierText("Industry", industryMod);
            
            if (commerceMod != 1.0f)
                infoString += FormatModifierText("Commerce", commerceMod);
                
            // Add terrain description
            infoString += $"\n<size=16><i>{region.terrainType.description}</i></size>";
            
            // Display terrain icon if available
            if (terrainIcon != null && region.terrainType.terrainIcon != null)
            {
                terrainIcon.sprite = region.terrainType.terrainIcon;
                terrainIcon.color = region.terrainType.baseColor;
                terrainIcon.gameObject.SetActive(true);
            }
            else if (terrainIcon != null)
            {
                terrainIcon.gameObject.SetActive(false);
            }
        }
        else if (terrainIcon != null)
        {
            terrainIcon.gameObject.SetActive(false);
        }

        // Add resource information if available
        if (region.resources != null)
        {
            infoString += "\n\n<size=20><b>Resources:</b></size>\n";
            
            var allResources = region.resources.GetAllResources();
            var productionRates = region.resources.GetAllProductionRates();
            var consumptionRates = region.resources.GetAllConsumptionRates();
            
            foreach (var resource in allResources.Keys)
            {
                float amount = allResources[resource];
                float production = productionRates.ContainsKey(resource) ? productionRates[resource] : 0;
                float consumption = consumptionRates.ContainsKey(resource) ? consumptionRates[resource] : 0;
                float netChange = production - consumption;
                
                string colorCode = netChange >= 0 ? "green" : "red";
                string arrow = netChange > 0 ? "↑" : (netChange < 0 ? "↓" : "→");
                
                infoString += $"<b>{resource}:</b> {amount:F1} <color={colorCode}>{arrow} ({netChange:+0.0;-0.0})</color>\n";
            }
        }
        
        infoText.text = infoString;
    }
    
    // Format a modifier into rich text with appropriate coloring
    private string FormatModifierText(string sectorName, float modifier)
    {
        string effectText = "";
        float percentage = (modifier - 1.0f) * 100f;
        
        if (percentage > 0)
        {
            effectText = $"\n<color=green>{sectorName}: +{percentage:F0}%</color>";
        }
        else if (percentage < 0)
        {
            effectText = $"\n<color=red>{sectorName}: {percentage:F0}%</color>";
        }
        
        return effectText;
    }
}
