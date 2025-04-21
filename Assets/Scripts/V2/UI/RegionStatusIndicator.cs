using UnityEngine;
using UnityEngine.UI;
using V2.Entities;
using V2.Systems;
using V2.Managers;

namespace V2.UI
{
    public class RegionStatusIndicator : MonoBehaviour
    {
        [SerializeField] private Image satisfactionFill;
        [SerializeField] private Image wealthFill;
        [SerializeField] private Image productionFill;
        
        private RegionView parentRegion;
        
        private void Awake()
        {
            parentRegion = GetComponentInParent<RegionView>();
        }
        
        private void Update()
        {
            // if (parentRegion != null && parentRegion.RegionEntity != null)
            // {
            //     UpdateStatusIndicators(parentRegion.RegionEntity);
            // }
        }
        
        private void UpdateStatusIndicators(RegionEntity region)
        {
            // Update satisfaction indicator (0-1 value)
            if (satisfactionFill != null)
            {
                satisfactionFill.fillAmount = region.Population.Satisfaction;
                
                // Color coding (red for low, yellow for medium, green for high)
                if (region.Population.Satisfaction < 0.3f)
                    satisfactionFill.color = Color.red;
                else if (region.Population.Satisfaction < 0.7f)
                    satisfactionFill.color = Color.yellow;
                else
                    satisfactionFill.color = Color.green;
            }
            
            // Update wealth indicator (normalized to some max value)
            if (wealthFill != null)
            {
                float maxWealth = 500f; // Adjust based on your game's scale
                wealthFill.fillAmount = Mathf.Clamp01(region.Economy.Wealth / maxWealth);
                wealthFill.color = Color.cyan;
            }
            
            // Update production indicator
            if (productionFill != null)
            {
                float maxProduction = 200f; // Adjust based on your game's scale
                productionFill.fillAmount = Mathf.Clamp01(region.Economy.Production / maxProduction);
                productionFill.color = Color.magenta;
            }
        }
    }
}