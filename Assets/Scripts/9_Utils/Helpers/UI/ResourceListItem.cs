/// CLASS PURPOSE:
/// ResourceListItem is a UI helper component that populates a single line item in a
/// resource display panel. It visualizes key metrics such as current amount, production,
/// consumption, and net balance for a given resource, along with associated icons and trends.
/// 
/// CORE RESPONSIBILITIES:
/// - Display resource name, quantity, and per-turn production/consumption
/// - Format and color-code the net resource balance
/// - Retrieve and assign resource icons from the ResourceRegistry
/// - Indicate trend direction with an arrow-style image
/// 
/// KEY COLLABORATORS:
/// - ResourceRegistry: Provides access to resource metadata including icons
/// - Unity UI (TextMeshProUGUI, Image): Displays textual and visual elements of each resource line
/// 
/// CURRENT ARCHITECTURE NOTES:
/// - Uses null checks for each UI reference to support flexible prefab setups
/// - Formats floats with one decimal precision and visual cues for balance direction
/// 
/// REFACTORING SUGGESTIONS:
/// - Move balance formatting logic into a helper method or formatter class
/// - Consider caching the icon lookup if used repeatedly for performance
/// 
/// EXTENSION OPPORTUNITIES:
/// - Add tooltip or extended data on hover
/// - Animate trend indicator for dynamic feedback
/// - Support different icon styles or rarity indicators per resource

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceListItem : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI resourceNameText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI productionText;
    public TextMeshProUGUI consumptionText;
    public TextMeshProUGUI balanceText;
    public Image resourceIcon;
    public Image trendIndicator;
    
    private string resourceName;
    
    public void Setup(string resourceName, float amount, float production, float consumption, float balance)
    {
        this.resourceName = resourceName;
        
        // Set texts
        if (resourceNameText != null)
            resourceNameText.text = resourceName;
        
        if (amountText != null)
            amountText.text = amount.ToString("F1");
        
        if (productionText != null)
            productionText.text = production.ToString("F1") + "/turn";
        
        if (consumptionText != null)
            consumptionText.text = consumption.ToString("F1") + "/turn";
        
        // Format balance with +/- prefix
        if (balanceText != null)
        {
            string prefix = balance >= 0 ? "+" : "";
            balanceText.text = $"{prefix}{balance:F1}/turn";
            balanceText.color = balance >= 0 ? Color.green : Color.red;
        }
        
        // Set icon if available
        if (resourceIcon != null)
        {
            // Try to get icon from ResourceRegistry
            Sprite icon = GetResourceIcon(resourceName);
            if (icon != null)
            {
                resourceIcon.sprite = icon;
                resourceIcon.gameObject.SetActive(true);
            }
            else
            {
                resourceIcon.gameObject.SetActive(false);
            }
        }
        
        // Set trend indicator direction
        if (trendIndicator != null)
        {
            // Rotate based on trend
            float rotation = balance > 0 ? 0 : 180;
            trendIndicator.transform.rotation = Quaternion.Euler(0, 0, rotation);
            trendIndicator.color = balance >= 0 ? Color.green : Color.red;
        }
    }
    
    private Sprite GetResourceIcon(string resourceName)
    {
        // Check if we have ResourceRegistry available
        ResourceRegistry registry = ResourceRegistry.Instance;
        if (registry != null)
        {
            ResourceDataSO resource = registry.GetResourceDefinition(resourceName);
            if (resource != null && resource.resourceIcon != null)
            {
                return resource.resourceIcon;
            }
        }
        
        return null;
    }
}
