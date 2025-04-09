using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace V1.Utils
{ 
    /// CLASS PURPOSE:
    /// TabGroup manages a set of UITabButtons and associated content panels,
    /// handling tab selection, visual state updates, and page activation logic
    /// to support tab-based UI navigation in Unity.
    ///
    /// CORE RESPONSIBILITIES:
    /// - Register and initialize UITabButtons and their corresponding content panels
    /// - Handle tab selection and invoke appropriate visual updates
    /// - Control panel visibility to reflect the currently selected tab
    /// - Provide hover feedback via OnTabEnter and OnTabExit
    ///
    /// KEY COLLABORATORS:
    /// - UITabButton: Individual tab elements that report user interaction
    /// - Unity UI (Button, GameObject): Used to trigger and display tab pages
    ///
    /// CURRENT ARCHITECTURE NOTES:
    /// - Uses lambda with index capture to ensure correct tab linkage
    /// - Defaults to selecting the first tab if available on Start
    /// - Handles tabs and pages in parallel lists based on index alignment
    ///
    /// REFACTORING SUGGESTIONS:
    /// - Replace parallel list management with a data structure linking tabs to pages
    /// - Add null checks and validation for mismatched list lengths
    /// - Provide external API for programmatic tab switching
    ///
    /// EXTENSION OPPORTUNITIES:
    /// - Animate transitions between tab pages
    /// - Add keyboard/gamepad navigation support
    /// - Track selected tab state for persistence or analytics
    /// <summary>
    /// Manages a group of tabs for UI navigation
    /// </summary>
    public class TabGroup : MonoBehaviour
    {
        [Header("Tab References")]
        public List<UITabButton> tabButtons = new List<UITabButton>();
        public List<GameObject> tabPages = new List<GameObject>();
        
        [Header("Tab Styling")]
        public Color tabIdle = new Color(0.5f, 0.5f, 0.5f);
        public Color tabHover = new Color(0.7f, 0.7f, 0.7f);
        public Color tabActive = Color.white;
        
        private UITabButton selectedTab;
        
        private void Start()
        {
            // Initialize tabs
            for (int i = 0; i < tabButtons.Count && i < tabPages.Count; i++) 
            {
                // Ensure tab buttons are set up
                if (tabButtons[i] != null)
                {
                    tabButtons[i].SetTabGroup(this);
                    // Use i for closure in lambda
                    int tabIndex = i;
                    tabButtons[i].GetComponent<Button>().onClick.AddListener(() => OnTabSelected(tabButtons[tabIndex]));
                }
            }
            
            // Initialize first tab as selected if available
            if (tabButtons.Count > 0 && tabButtons[0] != null)
            {
                OnTabSelected(tabButtons[0]);
            }
        }
        
        public void OnTabSelected(UITabButton button)
        {
            selectedTab = button;
            
            // Update tab visuals
            for (int i = 0; i < tabButtons.Count; i++)
            {
                if (tabButtons[i] != null)
                {
                    tabButtons[i].SetState(tabButtons[i] == selectedTab);
                    
                    // Show/hide corresponding page
                    if (i < tabPages.Count && tabPages[i] != null)
                    {
                        tabPages[i].SetActive(tabButtons[i] == selectedTab);
                    }
                }
            }
        }
        
        public void OnTabEnter(UITabButton button)
        {
            // Only highlight if not the selected tab
            if (button != selectedTab)
            {
                button.SetColor(tabHover);
            }
        }
        
        public void OnTabExit(UITabButton button)
        {
            // Restore idle color if not the selected tab
            if (button != selectedTab)
            {
                button.SetColor(tabIdle);
            }
        }
    }
}