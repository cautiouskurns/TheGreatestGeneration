using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace V1.Utils
{ 
    /// CLASS PURPOSE:
    /// UITabButton represents a single interactive tab in a tab-based UI system.
    /// It manages its visual state and delegates interaction events to the parent TabGroup.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Handle mouse hover events to notify the parent tab group
    /// - Set its visual color based on active/inactive state
    /// - Store reference to the owning TabGroup for delegation
    ///
    /// KEY COLLABORATORS:
    /// - TabGroup: Manages the collection of UITabButtons and controls tab logic
    /// - UnityEngine.UI.Image: Used to visually update the tab's background color
    /// - UnityEngine.EventSystems: Enables pointer enter/exit handling
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Relies on background Image being present on the same GameObject
    /// - Uses color values provided by the TabGroup for visual consistency
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Add null checks or error handling for missing TabGroup or Image
    /// - Expose current state for querying or binding from other scripts
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Add visual transitions or animations for state changes
    /// - Support additional states (e.g., disabled, hovered) with separate styles
    /// - Integrate audio feedback or accessibility support
    /// <summary>
    /// Represents a selectable tab in a tab group
    /// </summary>
    public class UITabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private TabGroup tabGroup;
        private Image background;
        
        private void Awake()
        {
            background = GetComponent<Image>();
        }
        
        public void SetTabGroup(TabGroup newTabGroup)
        {
            tabGroup = newTabGroup;
        }
        
        public void SetColor(Color color)
        {
            if (background != null)
            {
                background.color = color;
            }
        }
        
        public void SetState(bool isActive)
        {
            if (background != null)
            {
                background.color = isActive ? tabGroup.tabActive : tabGroup.tabIdle;
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tabGroup != null)
            {
                tabGroup.OnTabEnter(this);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (tabGroup != null)
            {
                tabGroup.OnTabExit(this);
            }
        }
    }
}
