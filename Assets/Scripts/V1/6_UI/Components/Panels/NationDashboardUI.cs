using UnityEngine;
using TMPro;
using UnityEngine.UI;
using V1.Managers;
using V1.Entities;

namespace V1.UI
{   
    /// CLASS PURPOSE:
    /// NationDashboardUI manages the UI display for selected nation details,
    /// updating text and flag visuals when nation-related events are triggered.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Listen for nation selection and update events
    /// - Display nation summary information in a TextMeshPro field
    /// - Optionally update flag or visual representation of the nation
    ///
    /// KEY COLLABORATORS:
    /// - NationEntity: Provides summary text and nation metadata
    /// - EventBus: Subscribes to selection and update events to trigger UI refresh
    /// - Unity UI (TextMeshProUGUI, Image): Renders the nation information panel
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Updates occur only when this panel is active via OnEnable
    /// - Nation flag support is noted but not currently implemented
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Add null safety and error handling for missing UI references
    /// - Move flag assignment to a separate method when image resources are available
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Integrate additional data such as traits, satisfaction, or influence
    /// - Add animation or highlighting for selected nation transitions
    /// - Support localization of nation summary text


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
            //nationInfoText.color = nation.nationColor;
            
            // You could set a nation flag image here if available
        }
    }
}