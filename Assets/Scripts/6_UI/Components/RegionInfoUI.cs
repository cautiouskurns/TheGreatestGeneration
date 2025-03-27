using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RegionInfoUI : MonoBehaviour
{
    public TextMeshProUGUI infoText;
    public Image terrainIcon;

    private void Start()
    {
//        Debug.Log("RegionInfoUI Start method called");
        
        if (infoText == null)
        {
            Debug.LogError("InfoText reference is null in RegionInfoUI");
        }
        else
        {
////            Debug.Log("InfoText reference is valid");
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
        RegionEntity region = regionObj as RegionEntity;
        if (region == null || infoText == null) return;

        // Create a clean, structured layout with consistent styling
        string infoString = "";
        
        // HEADER SECTION
        infoString += $"<size=26><b>{region.regionName}</b></size>\n";
        infoString += $"<color=#DDDDDD><size=18>{region.ownerNationName}</size></color>\n\n";
        
        // STATISTICS SECTION
        infoString += "<size=20><b>Region Statistics</b></size>\n";
        infoString += "<color=#666666>──────────────────</color>\n";
        
        // Key statistics with trend indicators
        infoString += $"<color=#FFD700>Wealth:</color> <color=#FFFFFF>{region.wealth}</color>";
        if (region.hasChangedThisTurn)
        {
            string trendIcon = region.wealthDelta > 0 ? "↑" : (region.wealthDelta < 0 ? "↓" : "→");
            string trendColor = region.wealthDelta > 0 ? "#00FF00" : (region.wealthDelta < 0 ? "#FF0000" : "#FFFFFF");
            infoString += $" <color={trendColor}>{trendIcon} {region.wealthDelta}</color>";
        }
        infoString += "\n";
        
        infoString += $"<color=#87CEEB>Production:</color> <color=#FFFFFF>{region.production}</color>\n\n";
        
        // TERRAIN SECTION - Only if terrain type exists
        if (region.terrainType != null)
        {
            infoString += $"<size=20><b>Terrain: {region.terrainType.terrainName}</b></size>\n";
            infoString += "<color=#666666>──────────────────</color>\n";
            
            // Add terrain description
            infoString += $"<color=#FFFFFF><i>{region.terrainType.description}</i></color>\n\n";
            
            // Add terrain effects in a clean table format
            infoString += "<size=18><b>Terrain Effects</b></size>\n";
            
            // Only show modifiers that aren't 1.0 (neutral)
            bool hasModifiers = false;
            
            float agricultureMod = region.terrainType.GetMultiplierForSector("agriculture");
            float miningMod = region.terrainType.GetMultiplierForSector("mining");
            float industryMod = region.terrainType.GetMultiplierForSector("industry");
            float commerceMod = region.terrainType.GetMultiplierForSector("commerce");
            
            if (agricultureMod != 1.0f || miningMod != 1.0f || industryMod != 1.0f || commerceMod != 1.0f)
            {
                hasModifiers = true;
                
                if (agricultureMod != 1.0f)
                    infoString += $"• <color=#FFFFFF>Agriculture:</color> {FormatPercentage(agricultureMod)}\n";
                
                if (miningMod != 1.0f)
                    infoString += $"• <color=#FFFFFF>Mining:</color> {FormatPercentage(miningMod)}\n";
                
                if (industryMod != 1.0f)
                    infoString += $"• <color=#FFFFFF>Industry:</color> {FormatPercentage(industryMod)}\n";
                
                if (commerceMod != 1.0f)
                    infoString += $"• <color=#FFFFFF>Commerce:</color> {FormatPercentage(commerceMod)}\n";
            }
            
            if (!hasModifiers)
            {
                infoString += "<color=#AAAAAA>No special modifiers for this terrain.</color>\n";
            }
            
            infoString += "\n";
        }
        
        // RESOURCES SECTION
        if (region.resources != null)
        {
            var allResources = region.resources.GetAllResources();
            
            if (allResources.Count > 0)
            {
                infoString += "<size=20><b>Resources</b></size>\n";
                infoString += "<color=#666666>──────────────────</color>\n";
                
                var productionRates = region.resources.GetAllProductionRates();
                var consumptionRates = region.resources.GetAllConsumptionRates();
                
                foreach (var resource in allResources.Keys)
                {
                    float amount = allResources[resource];
                    float production = productionRates.ContainsKey(resource) ? productionRates[resource] : 0;
                    float consumption = consumptionRates.ContainsKey(resource) ? consumptionRates[resource] : 0;
                    float netChange = production - consumption;
                    
                    string colorCode = netChange > 0 ? "#00FF00" : (netChange < 0 ? "#FF5555" : "#FFFFFF");
                    string arrow = netChange > 0 ? "↑" : (netChange < 0 ? "↓" : "→");
                    
                    // Create cleaner resource entry
                    infoString += $"<b>{resource}:</b> <color=#FFFFFF>{amount:F1}</color>  <color={colorCode}>{arrow} {netChange:+0.0;-0.0}/turn</color>\n";
                }
            }
        }
        // Add to the UpdateInfo method of RegionInfoUI, after the Resources section
        if (region.productionComponent != null)
        {
            var activeRecipes = region.productionComponent.GetActiveRecipes();
            
            if (activeRecipes.Count > 0)
            {
                infoString += "\n<size=20><b>Production</b></size>\n";
                infoString += "<color=#666666>──────────────────</color>\n";
                
                foreach (var recipeName in activeRecipes)
                {
                    float progress = region.productionComponent.GetRecipeProgress(recipeName);
                    
                    // Display recipe with progress if it's a multi-turn recipe
                    if (progress > 0)
                    {
                        infoString += $"<b>{recipeName}:</b> <color=#FFFFFF>In Progress ({progress:F1})</color>\n";
                    }
                    else
                    {
                        infoString += $"<b>{recipeName}:</b> <color=#FFFFFF>Active</color>\n";
                    }
                }
            }
        }


        // Add economic section
        infoString += "\n\n<size=20><b>Economy</b></size>\n";
        infoString += "<color=#666666>──────────────────</color>\n";

        // Population and labor
        infoString += $"<color=#FFD700>Population:</color> <color=#FFFFFF>{region.laborAvailable}</color>";
        infoString += $"\n<color=#87CEEB>Capital:</color> <color=#FFFFFF>{region.capitalInvestment:F1}</color>";

        // Satisfaction with color coding
        string satisfactionColor = "#FF5555"; // Red for low satisfaction
        if (region.satisfaction > 0.7f)
            satisfactionColor = "#00FF00"; // Green for high satisfaction
        else if (region.satisfaction > 0.5f)
            satisfactionColor = "#FFFF00"; // Yellow for medium satisfaction

        infoString += $"\n<color=#FFFFFF>Satisfaction:</color> <color={satisfactionColor}>{region.satisfaction:P0}</color>";
        
        // TRADE SECTION
        infoString += "\n\n<size=20><b>Trade</b></size>\n";
        infoString += "<color=#666666>──────────────────</color>\n";

        // Get trade data
        var tradeSystem = FindFirstObjectByType<TradeSystem>();
        if (tradeSystem != null)
        {
            // Show imports
            var imports = tradeSystem.GetRecentImports(region.regionName);
            if (imports.Count > 0)
            {
                infoString += "<color=#FFFFFF><b>Imports:</b></color>\n";
                foreach (var trade in imports)
                {
                    infoString += $"• <color=#87CEEB>{trade.resourceName}</color> from <color=#DDDDDD>{trade.partnerName}</color>: <color=#FFFFFF>{trade.amount:F1}</color>\n";
                }
            }
            
            // Show exports
            var exports = tradeSystem.GetRecentExports(region.regionName);
            if (exports.Count > 0)
            {
                infoString += "\n<color=#FFFFFF><b>Exports:</b></color>\n";
                foreach (var trade in exports)
                {
                    infoString += $"• <color=#87CEEB>{trade.resourceName}</color> to <color=#DDDDDD>{trade.partnerName}</color>: <color=#FFFFFF>{trade.amount:F1}</color>\n";
                }
            }
            
            // If no trade
            if (imports.Count == 0 && exports.Count == 0)
            {
                infoString += "<color=#AAAAAA>No recent trade activity.</color>\n";
            }
        }
        else
        {
            infoString += "<color=#AAAAAA>Trade system not available.</color>\n";
        }


        // Display the formatted text
        infoText.text = infoString;
    }





    // Helper method for formatting percentages with color
    private string FormatPercentage(float modifier)
    {
        float percentage = (modifier - 1.0f) * 100f;
        string colorCode = percentage > 0 ? "#00FF00" : "#FF5555";
        string sign = percentage > 0 ? "+" : "";
        
        return $"<color={colorCode}>{sign}{percentage:F0}%</color>";
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

public class TradeInfo
{
    public string partnerName;
    public string resourceName;
    public float amount;
}